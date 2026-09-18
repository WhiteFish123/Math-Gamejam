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
        foreach (var level in levels)
        {
            if (level == null) continue;
            var card = Instantiate(cardPrefab, contentParent);
            card.Init(level, this);
        }
    }

    public void OnLevelClicked(SceneDataSO levelData)
    {
        LevelManager.instance?.LoadScene(levelData.sceneName);
    }

    public void OnBackClicked()
    {
        LevelManager.instance?.LoadScene("MainMenu");
    }
}