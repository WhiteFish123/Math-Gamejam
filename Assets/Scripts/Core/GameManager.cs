using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;

namespace GameCore
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance;

        [Header("Config")]
        public LevelConfig config;
        public PlayerController player;
        public float moveDuration = 0.15f;
        public GameObject winPanel;
        public GameObject allClearPanel;
        public GameObject settingsPanel;
        public TileBase defaultFloorTile;
        [Tooltip("下一关的场景名（用于胜利面板的「下一关」按钮）")]
        public string nextSceneName;

        public MapManager map;
        private UndoManager undo;
        private ConditionManager conditions;
        private List<Box> boxes = new List<Box>();
        private bool isAnimating;

        [Header("Debug")]
        public bool showDebug = true;
        public bool showMapDebug = true;

        void Awake()
        {
            instance = this;
            map = new MapManager();
            undo = new UndoManager();
            conditions = new ConditionManager();
        }

        void Start()
        {
            map.Init(config);
            map.showDebug = showMapDebug;
            conditions.Init(config, map);

            if (showDebug)
            {
                Debug.Log($"[GameManager] 地图范围：width={config.gridWidth}, height={config.gridHeight}, 格子索引范围=([0~{config.gridWidth-1}], [0~{config.gridHeight-1}])");
                Debug.Log($"[GameManager] Grid name={config.grid.name}, pos={config.grid.transform.position}, cellSize={config.grid.cellSize}, InstanceID={config.grid.GetInstanceID()}");
            }

            Vector3Int playerCell = config.grid.WorldToCell(player.transform.position);
            if (showDebug) Debug.Log($"[GameManager] 玩家世界坐标={player.transform.position}, 对应格子=({playerCell.x},{playerCell.y})");

            boxes.Clear();
            boxes.AddRange(FindObjectsByType<Box>(FindObjectsSortMode.None));

            foreach (var box in boxes)
            {
                Vector3Int cell = config.grid.WorldToCell(box.transform.position);
                Vector2Int pos = new Vector2Int(cell.x, cell.y);
                if (showDebug) Debug.Log($"[GameManager] 箱子 {box.name}：localPos={box.transform.localPosition}, worldPos={box.transform.position}, parent={(box.transform.parent ? box.transform.parent.name : "无")}, 对应格子=({pos.x},{pos.y})");
                box.Init(config.grid, pos);
                box.isAlive = true;

                if (!map.IsAntiStain(pos))
                    map.SetColor(pos, box.boxColor);
            }

            if (winPanel != null) winPanel.SetActive(false);
            undo.Clear();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                ToggleSettings();
            if (settingsPanel != null && settingsPanel.activeSelf)
                return;
            if (Input.GetKeyDown(KeyCode.E))
                Undo();
            if (Input.GetKeyDown(KeyCode.R))
                ReloadLevel();
        }

        public void ToggleSettings()
        {
            if (settingsPanel == null) return;
            bool open = !settingsPanel.activeSelf;
            settingsPanel.SetActive(open);
            player.enabled = !open;
        }

        public void TryMove(Vector2Int dir)
        {
            if (isAnimating) { if (showDebug) Debug.Log($"[GameManager] 动画中，忽略输入"); return; }

            Vector2Int target = player.gridPos + dir;
            if (showDebug) Debug.Log($"[GameManager] 尝试移动：{player.gridPos} -> {target}");

            if (!map.IsWalkable(target)) { if (showDebug) Debug.Log($"[GameManager] 移动失败：目标格不可通行"); return; }

            undo.Record(CaptureState());

            Box pushedBox = null;
            Vector2Int boxOldPos = Vector2Int.zero;

            Box boxAtTarget = GetBoxAt(target);
            if (boxAtTarget != null)
            {
                if (showDebug) Debug.Log($"[GameManager] 目标格有箱子：{boxAtTarget.name}, 剩余推动次数={boxAtTarget.remainingPushes}");
                Vector2Int boxTarget = target + dir;
                if (!map.IsWalkable(boxTarget))
                {
                    if (showDebug) Debug.Log($"[GameManager] 推箱子失败：箱子目标格 {boxTarget} 不可通行");
                    undo.Pop();
                    return;
                }
                Box boxBehind = GetBoxAt(boxTarget);
                if (boxBehind != null)
                {
                    if (showDebug) Debug.Log($"[GameManager] 推箱子失败：箱子目标格 {boxTarget} 有另一个箱子 {boxBehind.name}");
                    undo.Pop();
                    return;
                }

                boxOldPos = boxAtTarget.gridPos;
                pushedBox = boxAtTarget;

                bool sameColor = map.GetColor(boxTarget) == boxAtTarget.boxColor;
                bool isAntiStain = map.IsAntiStain(boxTarget);
                if (!sameColor && !isAntiStain)
                {
                    boxAtTarget.remainingPushes--;
                    boxAtTarget.UpdatePushCountDisplay();
                }
                boxAtTarget.gridPos = boxTarget;

                if (boxAtTarget.remainingPushes <= 0)
                {
                    boxAtTarget.isAlive = false;
                }
            }

            Vector2Int oldPlayerPos = player.gridPos;
            player.gridPos = target;

            isAnimating = true;
            player.StartMoveAnimation(oldPlayerPos, moveDuration);
            if (pushedBox != null)
                pushedBox.StartMoveAnimation(boxOldPos, moveDuration);

            StartCoroutine(WaitForAnimationAndCheck(pushedBox, boxOldPos));
        }

        IEnumerator WaitForAnimationAndCheck(Box pushedBox, Vector2Int boxFromPos)
        {
            yield return new WaitForSeconds(moveDuration + 0.05f);

            if (pushedBox != null)
            {
                map.SetColor(boxFromPos, pushedBox.boxColor);

                if (!map.IsAntiStain(pushedBox.gridPos))
                    map.SetColor(pushedBox.gridPos, pushedBox.boxColor);

                if (!pushedBox.isAlive)
                    pushedBox.gameObject.SetActive(false);
            }

            isAnimating = false;
            CheckWin();
        }

        Box GetBoxAt(Vector2Int pos)
        {
            foreach (var box in boxes)
                if (box.isAlive && box.gridPos == pos)
                    return box;
            return null;
        }

        GameState CaptureState()
        {
            var state = new GameState();
            state.playerPos = player.gridPos;
            int w = map.colorGrid.GetLength(0);
            int h = map.colorGrid.GetLength(1);
            state.colorGrid = new int[w, h];
            System.Array.Copy(map.colorGrid, state.colorGrid, w * h);
            foreach (var box in boxes)
            {
                state.boxStates.Add(new BoxState
                {
                    boxRef = box,
                    color = box.boxColor,
                    gridPos = box.gridPos,
                    remainingPushes = box.remainingPushes,
                    alive = box.isAlive,
                });
            }
            return state;
        }

        void Undo()
        {
            if (!undo.CanUndo || isAnimating) return;
            StopAllCoroutines();
            isAnimating = false;
            RestoreState(undo.Pop());
        }

        void RestoreState(GameState state)
        {
            for (int x = 0; x < state.colorGrid.GetLength(0); x++)
            for (int y = 0; y < state.colorGrid.GetLength(1); y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                BoxColor oldColor = (BoxColor)state.colorGrid[x, y];
                if (map.GetColor(cell) != oldColor)
                {
                    Vector3Int c = new Vector3Int(x, y, 0);
                    if (oldColor == BoxColor.None && defaultFloorTile != null)
                        config.floorTilemap.SetTile(c, defaultFloorTile);
                    else if (oldColor != BoxColor.None)
                        config.floorTilemap.SetTile(c, config.colorPalette.GetFloorTile(oldColor));
                    map.colorGrid[x, y] = (int)oldColor;
                }
            }

            player.gridPos = state.playerPos;
            player.isMoving = false;
            player.transform.position = player.CellToBottomWorld(state.playerPos);

            foreach (var bs in state.boxStates)
            {
                Box box = bs.boxRef;
                box.gridPos = bs.gridPos;
                box.remainingPushes = bs.remainingPushes;
                box.UpdatePushCountDisplay();
                box.isAlive = bs.alive;
                box.gameObject.SetActive(bs.alive);
                if (bs.alive)
                    box.transform.position = box.GetWorldPos();
            }
        }

        void CheckWin()
        {
            if (conditions.CheckWin())
            {
                bool isLastLevel = string.IsNullOrEmpty(nextSceneName);
                if (isLastLevel && allClearPanel != null)
                    allClearPanel.SetActive(true);
                else if (winPanel != null)
                    winPanel.SetActive(true);
                if (showDebug) Debug.Log("You Win!");
            }
        }

        void ReloadLevel()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void NextLevel()
        {
            if (!string.IsNullOrEmpty(nextSceneName))
                LevelManager.instance?.LoadScene(nextSceneName);
            else
                Debug.LogWarning("[GameManager] nextSceneName 未设置！");
        }

        public void BackToMenu()
        {
            LevelManager.instance?.LoadScene("MainMenu");
        }

        public bool IsRowSatisfied(int row)
        {
            return conditions.IsRowSatisfied(row);
        }

        public bool IsColSatisfied(int col)
        {
            return conditions.IsColSatisfied(col);
        }
        public bool IsRowEntrySatisfied(int row, int entryIndex)
        {
            return conditions.IsRowEntrySatisfied(row, entryIndex);
        }

        public bool IsColEntrySatisfied(int col, int entryIndex)
        {
            return conditions.IsColEntrySatisfied(col, entryIndex);
        }
    }
}