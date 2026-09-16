using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UI : MonoBehaviour
{
    public static UI instance;

    [Header("Panel Registry")]
    [SerializeField] private List<GameObject> exclusivePanels = new List<GameObject>();
    [SerializeField] private List<GameObject> overlayPanels = new List<GameObject>();

    [Header("Special Layers")]
    [SerializeField] private List<RectTransform> alwaysOnTopLayers = new List<RectTransform>();

    private PlayerInputSet input;
    public bool alternativeInput { get; private set; }

    #region UI Panel References
    // 按项目需求在此区域声明子面板引用
    // 示例：
    // public UI_InGame inGameUI { get; private set; }
    // public UI_Options optionsUI { get; private set; }
    #endregion

    #region Tooltip References
    // 按项目需求在此区域声明 tooltip 引用
    // 示例：
    // public UI_ItemToolTip itemToolTip { get; private set; }
    #endregion

    //====================生命周期====================

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;

        CachePanelReferences();
    }

    /// <summary>
    /// 在此方法中用 GetComponentInChildren<T>(true) 缓存所有子面板引用。
    /// includeInactive=true 确保初始关闭的面板也能被拿到。
    /// </summary>
    private void CachePanelReferences()
    {
        // 示例：
        // inGameUI   = GetComponentInChildren<UI_InGame>(true);
        // optionsUI  = GetComponentInChildren<UI_Options>(true);
        //
        // itemToolTip = GetComponentInChildren<UI_ItemToolTip>(true);
    }

    // ==================== Input Binding ====================

    /// <summary>
    /// 由外部（如 GameManager）在初始化时调用一次，传入 InputSystem 的 PlayerInputSet。
    /// 所有 UI 相关输入绑定集中于此。
    /// </summary>
    public void SetupControlsUI(PlayerInputSet inputSet)
    {
        input = inputSet;

        // 示例绑定（等 InputSystem 配置好后取消注释）：
        //
        // input.UI.OptionsUI.performed += ctx => ToggleOptionsUI();
        // input.UI.AlternativeInput.performed += ctx => alternativeInput = true;
        // input.UI.AlternativeInput.canceled += ctx => alternativeInput = false;
    }

    // ==================== Panel Switching ====================

    /// <summary>
    /// 互斥式切换：关闭 exclusivePanels 中所有面板，只激活指定的那一个。
    /// overlayPanels 不受影响（叠加型面板独立管理）。
    /// </summary>
    public void SwitchTo(GameObject targetPanel)
    {
        if (targetPanel == null) return;

        foreach (var panel in exclusivePanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        targetPanel.SetActive(true);
    }

    /// <summary>
    /// 回到 InGame 状态：关闭所有独占面板，恢复玩家控制。
    /// </summary>
    public void SwitchToInGame()
    {
        HideAllTooltips();
        StopPlayerControls(false);

        foreach (var panel in exclusivePanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        foreach (var panel in overlayPanels)
        {
            if (panel != null)
                panel.SetActive(false);
        }

        // 如果 inGameUI 单独管理，在这里激活它
        // inGameUI?.gameObject.SetActive(true);
    }

    /// <summary>
    /// 叠加型面板的通用 Toggle 方法。
    /// </summary>
    public void ToggleOverlayPanel(GameObject panel)
    {
        if (panel == null) return;

        bool isActive = !panel.activeSelf;
        panel.SetActive(isActive);

        MoveAlwaysOnTopToLast();
        HideAllTooltips();
        StopPlayerControlsIfNeeded();
    }

    // ==================== Player Control ====================

    /// <summary>
    /// 强制设置玩家输入开关。
    /// </summary>
    public void StopPlayerControls(bool stopControls)
    {
        if (input == null) return;

        if (stopControls)
        {
            input.UI.Disable();
        }
        else
        {
            // 再次确认没有面板需要禁用玩家输入
            if (!IsAnyUIOpen())
                input.UI.Enable();
        }
    }

    /// <summary>
    /// 自动判断：只要 exclusivePanels 或 overlayPanels 中有任何面板开着，就禁用玩家输入。
    /// </summary>
    public void StopPlayerControlsIfNeeded()
    {
        if (input == null) return;

        if (IsAnyUIOpen())
            input.UI.Disable();
        else
            input.UI.Enable();
    }

    /// <summary>
    /// 检查是否有任何 UI 面板处于激活状态。
    /// </summary>
    public bool IsAnyUIOpen()
    {
        foreach (var panel in exclusivePanels)
        {
            if (panel != null && panel.activeSelf)
                return true;
        }
        foreach (var panel in overlayPanels)
        {
            if (panel != null && panel.activeSelf)
                return true;
        }
        return false;
    }

    // ==================== Tooltip Management ====================

    /// <summary>
    /// 关闭所有 tooltip。子类 tooltip 在此方法中追加调用。
    /// </summary>
    public void HideAllTooltips()
    {
        // 示例：
        // itemToolTip?.ShowToolTip(false, null);
        // skillToolTip?.ShowToolTip(false, null);
    }

    /// <summary>
    /// 确保 tooltip 和 always-on-top 层渲染顺序在最前面。
    /// </summary>
    public void MoveAlwaysOnTopToLast()
    {
        foreach (var layer in alwaysOnTopLayers)
        {
            if (layer != null)
                layer.SetAsLastSibling();
        }
    }

    // ==================== Public API (外部调用入口) ====================

    // 在此区域添加供其他系统调用的公开方法。
    // 命名规范：OpenXxxUI() / CloseXxxUI() / ToggleXxxUI()
    //
    // 示例：
    //
    // public void OpenOptionsUI()
    // {
    //     HideAllTooltips();
    //     StopPlayerControls(true);
    //     SwitchTo(optionsUI.gameObject);
    //     Time.timeScale = 0;
    // }
    //
    // public void CloseOptionsUI()
    // {
    //     Time.timeScale = 1;
    //     SwitchToInGame();
    // }
}