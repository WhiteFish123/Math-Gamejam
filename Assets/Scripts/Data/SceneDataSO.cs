using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "Math Game Jam/Level/Scene Data")]
public class SceneDataSO : ScriptableObject
{
    [Tooltip("场景文件名（与 Build Settings 中一致）")]
    public string sceneName;

    [Tooltip("显示用名称（可选，供 UI 使用）")]
    public string displayName;

    [Tooltip("进入此场景后自动播放的 BGM 组名")]
    public string bgmGroupName;

    [Tooltip("加载界面背景图（可选）")]
    public Sprite loadingBackground;

    /// <summary>
    /// 通过场景名获取 Build Settings 中的索引。
    /// 如果场景未添加到 Build Settings，返回 -1。
    /// </summary>
    public int BuildIndex
    {
        get
        {
            if (string.IsNullOrEmpty(sceneName)) return -1;
            return UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(
                GetScenePath()
            );
        }
    }

    private string GetScenePath()
    {
        return "Assets/Scenes/" + sceneName + ".unity";
    }
}
