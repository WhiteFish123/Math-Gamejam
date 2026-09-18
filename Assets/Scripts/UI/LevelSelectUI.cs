using UnityEngine;

public class LevelSelectUI : MonoBehaviour
{
    [Header("关卡列表")]
    [SerializeField] private SceneDataSO[] levels;

    [Header("UI 引用")]
    [SerializeField] private LevelCardUI cardPrefab;
    [SerializeField] private Transform contentParent;

    void Start()
    {
        Debug.Log($"[LevelSelectUI] levels 数量: {levels.Length}");
        foreach (var level in levels)
        {
            if (level == null) continue;
            var card = Instantiate(cardPrefab, contentParent);
            card.Init(level, this);
        }
    }

    public void OnLevelClicked(SceneDataSO levelData)
    {
        Debug.Log($"[LevelSelectUI] sceneName={levelData.sceneName}, LevelManager.instance={(LevelManager.instance != null ? "存在" : "NULL")}");
        LevelManager.instance?.LoadScene(levelData.sceneName);
    }

    public void OnBackClicked()
    {
        LevelManager.instance?.LoadScene("MainMenu");
    }
}