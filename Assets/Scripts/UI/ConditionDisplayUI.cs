using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace GameCore
{
    [System.Serializable]
    public struct ConditionColorEntry
    {
        public BoxColor boxColor;
        public Color textColor;
    }

    public class ConditionDisplayUI : MonoBehaviour
    {
        public Canvas canvas;
        public GameObject textPrefab;
        public Camera cam;
        public Grid grid;
        public LevelConfig levelConfig;

        [Header("Condition Colors")]
        public ConditionColorEntry[] conditionColors;

        [Header("Satisfied Outline")]
        public Color satisfiedOutlineColor = new Color32(0xE3, 0xFF, 0x00, 0xFF);
        public float satisfiedOutlineThickness = 0.06f;

        [Header("Debug")]
        public bool showDebug = true;

        private class LabelInfo
        {
            public GameObject go;
            public bool isRow;
            public int lineIndex;
            public int entryIndex;
        }
        private List<LabelInfo> labelInfos = new List<LabelInfo>();

        private ConditionManager conditionManager;
        private MapManager mapRef;

        void Start()
        {
            StartCoroutine(DelayedBuild());
        }

        System.Collections.IEnumerator DelayedBuild()
        {
            yield return null;
            BuildDisplay();
        }

        void BuildDisplay()
        {
            ClearDisplay();
            var map = GameManager.instance != null ? GameManager.instance.map : null;
            if (map == null)
            {
                if (showDebug) Debug.LogWarning("[ConditionDisplayUI] GameManager.instance.map 为 null，无法生成条件标签");
                return;
            }

            GenerateRowLabels(map);
            GenerateColLabels(map);
        }

        void ClearDisplay()
        {
            foreach (var info in labelInfos)
                if (info.go != null) Destroy(info.go);
            labelInfos.Clear();
        }

        void GenerateRowLabels(MapManager map)
        {
            var conds = levelConfig.rowConditions;
            if (showDebug) Debug.Log($"[ConditionDisplayUI] 行条件数量：{((conds != null) ? conds.Length.ToString() : "null")}, gridHeight={levelConfig.gridHeight}");
            if (conds == null) return;
            int h = levelConfig.gridHeight;

            for (int y = 0; y < h; y++)
            {
                if (y >= conds.Length || conds[y] == null) continue;
                var group = conds[y];
                if (group.entries.Count == 0) continue;

                int firstX = -1;
                for (int x = 0; x < map.Width; x++)
                {
                    var t = map.GetTileType(x, y);
                    if (t == TileType.Floor || t == TileType.AntiStain)
                    {
                        firstX = x;
                        break;
                    }
                }
                if (firstX < 0) continue;

                Vector3 basePos = grid.GetCellCenterWorld(new Vector3Int(firstX, y, 0));
                for (int i = 0; i < group.entries.Count; i++)
                {
                    var entry = group.entries[i];
                    Vector3 pos = basePos + new Vector3(-grid.cellSize.x * (i + 2), 0, 0);
                    Color c = GetDisplayColor(entry.color);
                    if (showDebug) Debug.Log($"[ConditionDisplayUI] 行 {y}[{i}]：color={entry.color}, count={entry.count}, worldPos={pos}");
                    CreateLabel(entry.count.ToString(), pos, c, true, y, i);
                }
            }
        }

        void GenerateColLabels(MapManager map)
        {
            var conds = levelConfig.colConditions;
            if (showDebug) Debug.Log($"[ConditionDisplayUI] 列条件数量：{((conds != null) ? conds.Length.ToString() : "null")}, gridWidth={levelConfig.gridWidth}");
            if (conds == null) return;
            int w = levelConfig.gridWidth;

            for (int x = 0; x < w; x++)
            {
                if (x >= conds.Length || conds[x] == null) continue;
                var group = conds[x];
                if (group.entries.Count == 0) continue;

                int firstY = -1;
                for (int y = map.Height - 1; y >= 0; y--)
                {
                    var t = map.GetTileType(x, y);
                    if (t == TileType.Floor || t == TileType.AntiStain)
                    {
                        firstY = y;
                        break;
                    }
                }
                if (firstY < 0) continue;

                Vector3 basePos = grid.GetCellCenterWorld(new Vector3Int(x, firstY, 0));
                for (int i = 0; i < group.entries.Count; i++)
                {
                    var entry = group.entries[i];
                    Vector3 pos = basePos + new Vector3(0, grid.cellSize.y * (i + 3), 0);
                    Color c = GetDisplayColor(entry.color);
                    if (showDebug) Debug.Log($"[ConditionDisplayUI] 列 {x}[{i}]：firstY={firstY}, color={entry.color}, count={entry.count}, worldPos={pos}");
                    CreateLabel(entry.count.ToString(), pos, c, false, x, i);
                }
            }
        }

        void CreateLabel(string text, Vector3 worldPos, Color color, bool isRow, int lineIndex, int entryIndex)
        {
            if (textPrefab == null || canvas == null || cam == null) return;
            GameObject go = Instantiate(textPrefab, canvas.transform);
            go.transform.SetAsFirstSibling();
            var tmp = go.GetComponent<TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
                tmp.color = color;
            }

            Vector2 screenPos = cam.WorldToScreenPoint(worldPos);
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.position = screenPos;

            labelInfos.Add(new LabelInfo { go = go, isRow = isRow, lineIndex = lineIndex, entryIndex = entryIndex });
        }

        void Update()
        {
            var gm = GameManager.instance;
            if (gm == null) return;

            foreach (var info in labelInfos)
            {
                bool satisfied = info.isRow
                    ? gm.IsRowEntrySatisfied(info.lineIndex, info.entryIndex)
                    : gm.IsColEntrySatisfied(info.lineIndex, info.entryIndex);

                var tmp = info.go.GetComponent<TextMeshProUGUI>();
                if (tmp != null)
                {
                    if (satisfied)
                    {
                        tmp.fontMaterial.EnableKeyword("OUTLINE_ON");
                        tmp.outlineWidth = satisfiedOutlineThickness;
                        tmp.outlineColor = satisfiedOutlineColor;
                    }
                    else
                    {
                        tmp.fontMaterial.DisableKeyword("OUTLINE_ON");
                        tmp.outlineWidth = 0f;
                    }
                }
            }
        }

        Color GetDisplayColor(BoxColor boxColor)
        {
            if (conditionColors == null || conditionColors.Length == 0)
                return Color.white;
            foreach (var entry in conditionColors)
                if (entry.boxColor == boxColor)
                    return entry.textColor;
            return Color.white;
        }
    }
}