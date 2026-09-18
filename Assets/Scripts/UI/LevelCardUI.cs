using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelCardUI : MonoBehaviour
{
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI levelNameText;
    [SerializeField] private Button button;

    private SceneDataSO levelData;
    private LevelSelectUI owner;

    public void Init(SceneDataSO data, LevelSelectUI selectUI)
    {
        levelData = data;
        owner = selectUI;

        if (data.previewImage != null)
            previewImage.sprite = data.previewImage;

        levelNameText.text = data.displayName;
        if (button == null) Debug.LogError($"[LevelCardUI] button 未赋值！");
        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        Debug.Log($"[LevelCardUI] 点击了卡片: {levelData?.displayName}");
        Debug.Log($"[LevelCardUI] owner={(owner != null ? "存在" : "NULL")}, levelData={(levelData != null ? levelData.sceneName : "NULL")}");
        owner.OnLevelClicked(levelData);
    }
}