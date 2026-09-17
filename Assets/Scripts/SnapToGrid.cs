using UnityEngine;

[ExecuteInEditMode]
public class SnapToGrid : MonoBehaviour
{
    public Grid grid;

    void Update()
    {
        if (Application.isPlaying) return;
        Vector3Int cell = grid.WorldToCell(transform.position);
        Vector3 pos = grid.GetCellCenterWorld(cell);
        pos += new Vector3(0, -grid.cellSize.y * 0.5f, 0);
        transform.position = pos;
    }
}