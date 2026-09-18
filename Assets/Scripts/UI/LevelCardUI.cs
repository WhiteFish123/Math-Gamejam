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

        button.onClick.AddListener(OnClicked);
    }

    void OnClicked()
    {
        owner.OnLevelClicked(levelData);
    }
}