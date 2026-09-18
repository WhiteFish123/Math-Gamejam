using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class YSort : MonoBehaviour
{
    public bool showDebug;

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError($"[YSort] {name} 没有 SpriteRenderer！", this);
        }
        // else
        // {
        //     Debug.Log($"[YSort] {name} 初始化：SortingLayer={spriteRenderer.sortingLayerName}, OrderInLayer={spriteRenderer.sortingOrder}", this);
        // }
    }

    void LateUpdate()
    {
        if (spriteRenderer == null) return;

        int newOrder = Mathf.RoundToInt((-transform.position.y + 100f) * 100f);
        spriteRenderer.sortingOrder = newOrder;

        if (showDebug)
        {
            Debug.Log($"[YSort] {name}: Pos=({transform.position.x:F2},{transform.position.y:F2},{transform.position.z:F2}), Order={newOrder}, Layer={spriteRenderer.sortingLayerName}({spriteRenderer.sortingLayerID})");
        }
    }
}