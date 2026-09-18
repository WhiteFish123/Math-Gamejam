using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            OnChooseLevelClicked();
    }

    public void OnStartGameClicked()
    {
        Debug.Log("[MainMenuUI] 开始游戏按钮被点击");
        LevelManager.instance?.LoadScene("Level_01");
    }
    public void OnChooseLevelClicked()
    {
        LevelManager.instance?.LoadScene("ChooseLevel");
    }

    public void OnExitClicked()
    {
        Debug.Log("[MainMenuUI] 退出按钮被点击");
        LevelManager.instance?.ExitGame();
    }
}