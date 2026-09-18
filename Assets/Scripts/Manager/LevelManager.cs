using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public interface ISceneTransition//场景过渡效果接口，实现此接口即可自定义过渡效果（淡入淡出、擦除、像素化等）
{
    //覆盖屏幕（进入过渡），例如淡出到黑屏
    IEnumerator PlayEnter();

    //揭开屏幕（退出过渡），例如从黑屏淡入
    IEnumerator PlayExit();
}
public class LevelManager : MonoBehaviour//场景加载管理器
{
    public static LevelManager instance;
    [Header("Scene Registry")]//场景注册表
    [SerializeField] private List<SceneDataSO> sceneDataList = new List<SceneDataSO>();

    [Header("Transition")]
    [SerializeField] private float minLoadScreenTime = 0.5f;//最短加载屏幕时间

    //事件：供其他系统（UI、AudioManager 等）订阅
    public event Action<SceneDataSO> OnSceneLoadStarted;
    public event Action<SceneDataSO> OnSceneLoadCompleted;
    public event Action<float> OnSceneLoadProgress;

    private Dictionary<string, SceneDataSO> sceneLookup;
    private SceneDataSO currentSceneData;
    private AsyncOperation currentLoadOperation;
    private bool isLoading;
    private ISceneTransition transition;

    public bool IsLoading => isLoading;
    public SceneDataSO CurrentSceneData => currentSceneData;
    //生命周期

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        BuildSceneLookup();
        transition = GetComponent<ISceneTransition>();
    }

    private void BuildSceneLookup()
    {
        sceneLookup = new Dictionary<string, SceneDataSO>();
        foreach (var data in sceneDataList)
        {
            if (data != null && !string.IsNullOrEmpty(data.sceneName))
            {
                if (!sceneLookup.ContainsKey(data.sceneName))
                    sceneLookup.Add(data.sceneName, data);
            }
        }
    }

 
    public void LoadSceneImmediate(SceneDataSO sceneData)//通过场景名加载场景（无过渡效果）。
    {
        if (sceneData == null || isLoading) return;

        int index = sceneData.BuildIndex;
        if (index < 0)
        {
            Debug.LogError($"[LevelManager] 场景 '{sceneData.sceneName}' 未添加到 Build Settings");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneData, index, false));
    }


    public void LoadSceneWithTranslation(SceneDataSO sceneData)// 通过场景名加载场景（带过渡效果）。
    {
        if (sceneData == null || isLoading) return;

        int index = sceneData.BuildIndex;
        if (index < 0)
        {
            Debug.LogError($"[LevelManager] 场景 '{sceneData.sceneName}' 未添加到 Build Settings");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneData, index, true));
    }

    public void LoadScene(string sceneName)//通过场景名（字符串）加载，从注册表中查找对应的 SceneDataSO。
    {
        if (!sceneLookup.TryGetValue(sceneName, out var sceneData))
        {
            Debug.LogError($"[LevelManager] 未注册的场景名: '{sceneName}'");
            return;
        }
        LoadSceneWithTranslation(sceneData);
    }

    public void RestartCurrentScene()//重新加载当前场景。
    {
        if (currentSceneData != null)
            LoadSceneWithTranslation(currentSceneData);
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public SceneDataSO GetSceneData(string sceneName)// 从注册表中查找场景数据。
    {
        sceneLookup.TryGetValue(sceneName, out var data);
        return data;
    }

    //核心加载场景流程

    private IEnumerator LoadSceneRoutine(SceneDataSO sceneData, int buildIndex, bool useTransition)
    {
        isLoading = true;

        //通知外部：开始加载
        OnSceneLoadStarted?.Invoke(sceneData);

        //过渡效果 — 进入（覆盖屏幕）
        if (useTransition && transition != null)
            yield return StartCoroutine(transition.PlayEnter());

        //异步加载场景
        currentLoadOperation = SceneManager.LoadSceneAsync(buildIndex);
        currentLoadOperation.allowSceneActivation = true;

        //跟踪加载进度
        float loadStartTime = Time.unscaledTime;
        while (!currentLoadOperation.isDone)
        {
            float progress = Mathf.Clamp01(currentLoadOperation.progress / 0.9f);
            OnSceneLoadProgress?.Invoke(progress);
            yield return null;
        }

        //确保最低加载屏显示时间
        float elapsed = Time.unscaledTime - loadStartTime;
        if (elapsed < minLoadScreenTime)
            yield return new WaitForSecondsRealtime(minLoadScreenTime - elapsed);

        OnSceneLoadProgress?.Invoke(1f);

        //更新当前场景记录
        currentSceneData = sceneData;

        //自动切换 BGM
        if (!string.IsNullOrEmpty(sceneData.bgmGroupName) && AudioManager.instance != null)
            AudioManager.instance.StartBGM(sceneData.bgmGroupName);

        //过渡效果 — 退出（揭开屏幕）
        if (useTransition && transition != null)
            yield return StartCoroutine(transition.PlayExit());

        //通知外部：加载完成
        OnSceneLoadCompleted?.Invoke(sceneData);

        isLoading = false;
        currentLoadOperation = null;
    }
}