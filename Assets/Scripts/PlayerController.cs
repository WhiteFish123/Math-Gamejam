using System.Collections;
using UnityEngine;

namespace GameCore
{
    public class PlayerController : MonoBehaviour
    {
        [Header("References")]
        public Grid grid;

        [Header("Movement")]
        public float moveDuration = 0.15f;

        [HideInInspector] public Vector2Int gridPos;
        [HideInInspector] public bool isMoving;

        void Start()
        {
            Vector3Int cell = grid.WorldToCell(transform.position);
            gridPos = new Vector2Int(cell.x, cell.y);
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
                GameManager.instance.TryMove(dir);
        }

        public void StartMoveAnimation(Vector2Int fromCell, float duration)
        {
            StartCoroutine(MoveRoutine(fromCell, duration));
        }

        IEnumerator MoveRoutine(Vector2Int fromCell, float duration)
        {
            isMoving = true;
            Vector3 start = CellToBottomWorld(fromCell);
            Vector3 end = CellToBottomWorld(gridPos);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, elapsed / duration);
                yield return null;
            }
            transform.position = end;
            isMoving = false;
        }

        public Vector3 CellToBottomWorld(Vector2Int cell)
        {
            Vector3 c = grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
            return c + new Vector3(0, -grid.cellSize.y * 0.5f, 0);
        }
    }
}