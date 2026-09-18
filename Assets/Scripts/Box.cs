using System.Collections;
using UnityEngine;
using TMPro;

namespace GameCore
{
    public class Box : MonoBehaviour
    {
        public BoxColor boxColor;
        public int remainingPushes;
        public TextMeshPro pushCountText;

        [HideInInspector] public Vector2Int gridPos;
        [HideInInspector] public bool isAlive = true;

        private Grid grid;

        public void Init(Grid g, Vector2Int pos)
        {
            grid = g;
            gridPos = pos;
            isAlive = true;
            UpdatePushCountDisplay();
        }

        public void UpdatePushCountDisplay()
        {
            if (pushCountText != null)
                pushCountText.text = remainingPushes.ToString();
        }

        public void StartMoveAnimation(Vector2Int fromCell, float duration)
        {
            StartCoroutine(MoveRoutine(fromCell, duration));
        }

        IEnumerator MoveRoutine(Vector2Int fromCell, float duration)
        {
            Vector3 start = GetWorldPosAt(fromCell);
            Vector3 end = GetWorldPosAt(gridPos);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(start, end, elapsed / duration);
                yield return null;
            }
            transform.position = end;
        }

        public Vector3 GetWorldPos()
        {
            return GetWorldPosAt(gridPos);
        }

        Vector3 GetWorldPosAt(Vector2Int cell)
        {
            Vector3 c = grid.GetCellCenterWorld(new Vector3Int(cell.x, cell.y, 0));
            return c + new Vector3(0, -grid.cellSize.y * 0.5f, 0);
        }
    }
}