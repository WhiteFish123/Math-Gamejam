using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [Header("Grid & Tilemap")]
    public Grid grid;
    public Tilemap floorTilemap;

    [Header("Layers")]
    public LayerMask wallLayer;
    public LayerMask boxLayer;
    public LayerMask goalLayer;

    [Header("Movement")]
    public float moveDuration = 0.15f;

    [Header("Tile Color")]
    public Color yellowColor = Color.yellow;

    private Vector2Int gridPos;
    private bool isMoving;

    void Start()
    {
        if (grid == null)
        {
            //Debug.LogError("[PlayerController] Grid 未赋值！请在 Inspector 中拖入 Grid 对象。");
            return;
        }
        Vector3Int cell = grid.WorldToCell(transform.position);
        gridPos = new Vector2Int(cell.x, cell.y);
        //Debug.Log($"[PlayerController] 初始化完成，gridPos = {gridPos}");
    }

    void Update()
    {
        if (isMoving) return;

        Vector2Int dir = Vector2Int.zero;
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            dir = Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            dir = Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            dir = Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            dir = Vector2Int.right;

        if (dir != Vector2Int.zero)
        {
            //Debug.Log($"[PlayerController] 检测到输入，方向 = {dir}");
            TryMove(dir);
        }
    }

    void TryMove(Vector2Int dir)
    {
        Vector2Int target = gridPos + dir;
        Vector3 worldPos = grid.GetCellCenterWorld(new Vector3Int(target.x, target.y, 0));
        Vector2 size = grid.cellSize * 0.8f;

        Debug.Log($"[PlayerController] TryMove: target={target}, worldPos={worldPos}");

        Collider2D wallHit = Physics2D.OverlapBox(worldPos, size, 0f, wallLayer);
        if (wallHit != null)
        {
            Debug.Log($"[PlayerController] 撞墙：{wallHit.name}");
            return;
        }

        Collider2D boxCol = Physics2D.OverlapBox(worldPos, size, 0f, boxLayer);
        if (boxCol != null)
        {
            Debug.Log($"[PlayerController] 前方有箱子：{boxCol.name}");

            Box box = boxCol.GetComponent<Box>();
            if (box == null)
            {
                Debug.LogError("[PlayerController] 箱子对象上没有 Box 脚本！");
                return;
            }

            Vector2Int boxTarget = box.gridPos + dir;
            Vector3 boxWorldPos = grid.GetCellCenterWorld(new Vector3Int(boxTarget.x, boxTarget.y, 0));

            Collider2D boxWallHit = Physics2D.OverlapBox(boxWorldPos, size, 0f, wallLayer);
            if (boxWallHit != null)
            {
                Debug.Log($"[PlayerController] 箱子前方有墙：{boxWallHit.name}，推不动");
                return;
            }

            Collider2D boxBoxHit = Physics2D.OverlapBox(boxWorldPos, size, 0f, boxLayer);
            if (boxBoxHit != null)
            {
                Debug.Log($"[PlayerController] 箱子前方有另一个箱子：{boxBoxHit.name}，推不动");
                return;
            }

            Vector3Int cellFrom = new Vector3Int(box.gridPos.x, box.gridPos.y, 0);
            if (IsGoal(box.gridPos))
                floorTilemap.SetColor(cellFrom, Color.white);
            else
                floorTilemap.SetColor(cellFrom, yellowColor);

            Debug.Log($"[PlayerController] 推动箱子 {box.name}，从 {box.gridPos} 到 {boxTarget}");
            StartCoroutine(MoveBox(box, boxTarget));
            box.gridPos = boxTarget;
        }
        else
        {
            Debug.Log("[PlayerController] 前方为空地，直接移动");
        }

        //Debug.Log($"[PlayerController] 玩家移动：{gridPos} -> {target}");
        StartCoroutine(MoveTo(target));
    }

    Vector3 CellToBottomWorld(Vector2Int cellPos)
    {
        Vector3 center = grid.GetCellCenterWorld(new Vector3Int(cellPos.x, cellPos.y, 0));
        return center + new Vector3(0, -grid.cellSize.y * 0.5f, 0);
    }

    bool IsGoal(Vector2Int pos)
    {
        Vector3 worldPos = grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
        Vector2 size = grid.cellSize * 0.8f;
        return Physics2D.OverlapBox(worldPos, size, 0f, goalLayer) != null;
    }

    IEnumerator MoveTo(Vector2Int target)
    {
        isMoving = true;
        Vector3 start = transform.position;
        Vector3 end = CellToBottomWorld(target);
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            yield return null;
        }
        transform.position = end;
        gridPos = target;
        isMoving = false;
        //Debug.Log($"[PlayerController] MoveTo 完成，当前位置：{gridPos}");
    }

    IEnumerator MoveBox(Box box, Vector2Int target)
    {
        Vector3 start = box.transform.position;
        Vector3 end = CellToBottomWorld(target);
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            box.transform.position = Vector3.Lerp(start, end, elapsed / moveDuration);
            yield return null;
        }
        box.transform.position = end;
        Debug.Log($"[PlayerController] MoveBox 完成，箱子位置：{target}");
    }
}