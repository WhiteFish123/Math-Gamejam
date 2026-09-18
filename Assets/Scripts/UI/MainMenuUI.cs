using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void OnStartGameClicked()
    {
        Debug.Log("[MainMenuUI] 开始游戏按钮被点击");
        LevelManager.instance?.LoadScene("Level_01");
    }

    public void OnExitClicked()
    {
        Debug.Log("[MainMenuUI] 退出按钮被点击");
        LevelManager.instance?.ExitGame();
    }
}