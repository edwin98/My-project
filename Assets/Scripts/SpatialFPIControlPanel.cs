using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 空间FPI插值可视化控制面板
/// </summary>
public class SpatialFPIControlPanel : MonoBehaviour
{
    [Header("UI组件引用")]
    public SpatialFPIVisualizer spatialVisualizer;
    public TrajectoryVisualizer trajectoryVisualizer;
    
    [Header("主控制")]
    public Toggle enableSpatialVisualizationToggle;
    public Button loadSpatialDataButton;
    public Button reloadDataButton;
    public TMP_InputField dataFileNameInput;
    
    [Header("可视化设置")]
    public Toggle showSpatialPointsToggle;
    public Toggle showBoundingBoxToggle;
    public Toggle showColorLegendToggle;
    public Toggle enableLODToggle;
    
    [Header("渲染设置")]
    public TMP_Dropdown visualizationModeDropdown;
    public TMP_Dropdown colorSchemeDropdown;
    public Slider pointSizeSlider;
    public Slider transparencySlider;
    public TMP_InputField maxVisiblePointsInput;
    
    [Header("LOD设置")]
    public Slider lodDistance1Slider;
    public Slider lodDistance2Slider;
    
    [Header("状态显示")]
    public TextMeshProUGUI statusText;
    public TextMeshProUGUI statisticsText;
    public TextMeshProUGUI performanceText;
    
    [Header("轨迹与插值同步")]
    public Toggle syncWithTrajectoryToggle;
    public Button centerViewButton;
    public Button fitViewButton;
    
    [Header("导出功能")]
    public Button exportVisualizationButton;
    public Button takeScreenshotButton;
    public TMP_InputField exportFileNameInput;
    
    // 私有变量
    private bool isInitialized = false;
    private float lastPerformanceUpdateTime;
    private int lastFrameCount;
    private float lastFrameRate;

    void Start()
    {
        InitializeControlPanel();
        SetupEventHandlers();
        UpdateUI();
    }

    void Update()
    {
        UpdatePerformanceDisplay();
        UpdateStatusDisplay();
    }

    /// <summary>
    /// 初始化控制面板
    /// </summary>
    void InitializeControlPanel()
    {
        // 查找空间可视化器（如果没有指定）
        if (spatialVisualizer == null)
        {
            spatialVisualizer = FindObjectOfType<SpatialFPIVisualizer>();
        }
        
        // 查找轨迹可视化器（如果没有指定）
        if (trajectoryVisualizer == null)
        {
            trajectoryVisualizer = FindObjectOfType<TrajectoryVisualizer>();
        }
        
        // 初始化下拉菜单
        InitializeDropdowns();
        
        // 设置默认值
        SetDefaultValues();
        
        isInitialized = true;
        Debug.Log("空间FPI控制面板初始化完成");
    }

    /// <summary>
    /// 初始化下拉菜单
    /// </summary>
    void InitializeDropdowns()
    {
        // 可视化模式下拉菜单
        if (visualizationModeDropdown != null)
        {
            visualizationModeDropdown.ClearOptions();
            var modeOptions = new List<string>();
            foreach (SpatialFPIVisualizer.VisualizationMode mode in System.Enum.GetValues(typeof(SpatialFPIVisualizer.VisualizationMode)))
            {
                modeOptions.Add(GetVisualizationModeDisplayName(mode));
            }
            visualizationModeDropdown.AddOptions(modeOptions);
        }
        
        // 颜色方案下拉菜单
        if (colorSchemeDropdown != null)
        {
            colorSchemeDropdown.ClearOptions();
            var colorOptions = new List<string>();
            foreach (SpatialFPIVisualizer.ColorScheme scheme in System.Enum.GetValues(typeof(SpatialFPIVisualizer.ColorScheme)))
            {
                colorOptions.Add(GetColorSchemeDisplayName(scheme));
            }
            colorSchemeDropdown.AddOptions(colorOptions);
        }
    }

    /// <summary>
    /// 设置默认值
    /// </summary>
    void SetDefaultValues()
    {
        if (dataFileNameInput != null)
            dataFileNameInput.text = "spatial_fpi_visualization.json";
        
        if (exportFileNameInput != null)
            exportFileNameInput.text = "spatial_visualization_export";
        
        if (pointSizeSlider != null)
        {
            pointSizeSlider.minValue = 0.01f;
            pointSizeSlider.maxValue = 1f;
            pointSizeSlider.value = 0.1f;
        }
        
        if (transparencySlider != null)
        {
            transparencySlider.minValue = 0.1f;
            transparencySlider.maxValue = 1f;
            transparencySlider.value = 0.8f;
        }
        
        if (maxVisiblePointsInput != null)
            maxVisiblePointsInput.text = "5000";
        
        if (lodDistance1Slider != null)
        {
            lodDistance1Slider.minValue = 10f;
            lodDistance1Slider.maxValue = 200f;
            lodDistance1Slider.value = 50f;
        }
        
        if (lodDistance2Slider != null)
        {
            lodDistance2Slider.minValue = 50f;
            lodDistance2Slider.maxValue = 500f;
            lodDistance2Slider.value = 100f;
        }
    }

    /// <summary>
    /// 设置事件处理器
    /// </summary>
    void SetupEventHandlers()
    {
        // 主控制事件
        if (enableSpatialVisualizationToggle != null)
            enableSpatialVisualizationToggle.onValueChanged.AddListener(OnEnableSpatialVisualizationChanged);
        
        if (loadSpatialDataButton != null)
            loadSpatialDataButton.onClick.AddListener(OnLoadSpatialDataClicked);
        
        if (reloadDataButton != null)
            reloadDataButton.onClick.AddListener(OnReloadDataClicked);
        
        // 可视化设置事件
        if (showSpatialPointsToggle != null)
            showSpatialPointsToggle.onValueChanged.AddListener(OnShowSpatialPointsChanged);
        
        if (showBoundingBoxToggle != null)
            showBoundingBoxToggle.onValueChanged.AddListener(OnShowBoundingBoxChanged);
        
        if (showColorLegendToggle != null)
            showColorLegendToggle.onValueChanged.AddListener(OnShowColorLegendChanged);
        
        if (enableLODToggle != null)
            enableLODToggle.onValueChanged.AddListener(OnEnableLODChanged);
        
        // 渲染设置事件
        if (visualizationModeDropdown != null)
            visualizationModeDropdown.onValueChanged.AddListener(OnVisualizationModeChanged);
        
        if (colorSchemeDropdown != null)
            colorSchemeDropdown.onValueChanged.AddListener(OnColorSchemeChanged);
        
        if (pointSizeSlider != null)
            pointSizeSlider.onValueChanged.AddListener(OnPointSizeChanged);
        
        if (transparencySlider != null)
            transparencySlider.onValueChanged.AddListener(OnTransparencyChanged);
        
        if (maxVisiblePointsInput != null)
            maxVisiblePointsInput.onEndEdit.AddListener(OnMaxVisiblePointsChanged);
        
        // LOD设置事件
        if (lodDistance1Slider != null)
            lodDistance1Slider.onValueChanged.AddListener(OnLODDistance1Changed);
        
        if (lodDistance2Slider != null)
            lodDistance2Slider.onValueChanged.AddListener(OnLODDistance2Changed);
        
        // 控制按钮事件
        if (centerViewButton != null)
            centerViewButton.onClick.AddListener(OnCenterViewClicked);
        
        if (fitViewButton != null)
            fitViewButton.onClick.AddListener(OnFitViewClicked);
        
        // 导出功能事件
        if (exportVisualizationButton != null)
            exportVisualizationButton.onClick.AddListener(OnExportVisualizationClicked);
        
        if (takeScreenshotButton != null)
            takeScreenshotButton.onClick.AddListener(OnTakeScreenshotClicked);
    }

    #region 事件处理方法

    void OnEnableSpatialVisualizationChanged(bool enabled)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.gameObject.SetActive(enabled);
            UpdateStatusText(enabled ? "空间插值可视化已启用" : "空间插值可视化已禁用");
        }
    }

    void OnLoadSpatialDataClicked()
    {
        if (spatialVisualizer != null && dataFileNameInput != null)
        {
            spatialVisualizer.spatialDataFileName = dataFileNameInput.text;
            spatialVisualizer.LoadSpatialData();
            UpdateStatusText($"正在加载数据文件: {dataFileNameInput.text}");
        }
    }

    void OnReloadDataClicked()
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.ReloadData();
            UpdateStatusText("正在重新加载数据...");
        }
    }

    void OnShowSpatialPointsChanged(bool show)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.showSpatialPoints = show;
            RefreshVisualization();
        }
    }

    void OnShowBoundingBoxChanged(bool show)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.showBoundingBox = show;
            RefreshVisualization();
        }
    }

    void OnShowColorLegendChanged(bool show)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.showColorLegend = show;
            RefreshVisualization();
        }
    }

    void OnEnableLODChanged(bool enable)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.enableLOD = enable;
            UpdateStatusText(enable ? "LOD已启用" : "LOD已禁用");
        }
    }

    void OnVisualizationModeChanged(int modeIndex)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.visualizationMode = (SpatialFPIVisualizer.VisualizationMode)modeIndex;
            RefreshVisualization();
            UpdateStatusText($"可视化模式已切换为: {GetVisualizationModeDisplayName(spatialVisualizer.visualizationMode)}");
        }
    }

    void OnColorSchemeChanged(int schemeIndex)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.colorScheme = (SpatialFPIVisualizer.ColorScheme)schemeIndex;
            RefreshVisualization();
            UpdateStatusText($"颜色方案已切换为: {GetColorSchemeDisplayName(spatialVisualizer.colorScheme)}");
        }
    }

    void OnPointSizeChanged(float size)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.pointSize = size;
            RefreshVisualization();
        }
    }

    void OnTransparencyChanged(float transparency)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.transparency = transparency;
            RefreshVisualization();
        }
    }

    void OnMaxVisiblePointsChanged(string value)
    {
        if (spatialVisualizer != null && int.TryParse(value, out int maxPoints))
        {
            spatialVisualizer.maxVisiblePoints = maxPoints;
            UpdateStatusText($"最大可见点数设置为: {maxPoints}");
        }
    }

    void OnLODDistance1Changed(float distance)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.lodDistance1 = distance;
        }
    }

    void OnLODDistance2Changed(float distance)
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.lodDistance2 = distance;
        }
    }

    void OnCenterViewClicked()
    {
        // 居中视图到插值数据中心
        if (spatialVisualizer != null && Camera.main != null)
        {
            // 简单的居中逻辑
            Camera.main.transform.LookAt(spatialVisualizer.transform);
            UpdateStatusText("视图已居中");
        }
    }

    void OnFitViewClicked()
    {
        // 调整视图以适应整个插值数据
        if (spatialVisualizer != null && Camera.main != null)
        {
            // 这里可以实现更复杂的视图适应逻辑
            UpdateStatusText("视图已调整以适应数据");
        }
    }

    void OnExportVisualizationClicked()
    {
        // 导出可视化数据或图像
        if (exportFileNameInput != null)
        {
            string fileName = exportFileNameInput.text;
            UpdateStatusText($"正在导出可视化: {fileName}");
            // 这里可以添加实际的导出逻辑
        }
    }

    void OnTakeScreenshotClicked()
    {
        // 截取当前可视化的屏幕截图
        StartCoroutine(TakeScreenshotCoroutine());
    }

    #endregion

    /// <summary>
    /// 刷新可视化
    /// </summary>
    void RefreshVisualization()
    {
        if (spatialVisualizer != null && isInitialized)
        {
            // 这里可以触发可视化的重新创建
            // spatialVisualizer.CreateSpatialVisualization();
        }
    }

    /// <summary>
    /// 更新UI状态
    /// </summary>
    void UpdateUI()
    {
        if (!isInitialized || spatialVisualizer == null) return;
        
        // 同步UI控件与可视化器状态
        if (showSpatialPointsToggle != null)
            showSpatialPointsToggle.isOn = spatialVisualizer.showSpatialPoints;
        
        if (showBoundingBoxToggle != null)
            showBoundingBoxToggle.isOn = spatialVisualizer.showBoundingBox;
        
        if (showColorLegendToggle != null)
            showColorLegendToggle.isOn = spatialVisualizer.showColorLegend;
        
        if (enableLODToggle != null)
            enableLODToggle.isOn = spatialVisualizer.enableLOD;
        
        if (visualizationModeDropdown != null)
            visualizationModeDropdown.value = (int)spatialVisualizer.visualizationMode;
        
        if (colorSchemeDropdown != null)
            colorSchemeDropdown.value = (int)spatialVisualizer.colorScheme;
        
        if (pointSizeSlider != null)
            pointSizeSlider.value = spatialVisualizer.pointSize;
        
        if (transparencySlider != null)
            transparencySlider.value = spatialVisualizer.transparency;
        
        if (maxVisiblePointsInput != null)
            maxVisiblePointsInput.text = spatialVisualizer.maxVisiblePoints.ToString();
        
        if (lodDistance1Slider != null)
            lodDistance1Slider.value = spatialVisualizer.lodDistance1;
        
        if (lodDistance2Slider != null)
            lodDistance2Slider.value = spatialVisualizer.lodDistance2;
    }

    /// <summary>
    /// 更新状态显示
    /// </summary>
    void UpdateStatusDisplay()
    {
        if (spatialVisualizer == null) return;
        
        // 更新统计信息
        UpdateStatisticsDisplay();
    }

    /// <summary>
    /// 更新统计信息显示
    /// </summary>
    void UpdateStatisticsDisplay()
    {
        if (statisticsText == null) return;
        
        string statsInfo = "空间插值统计:\n";
        
        if (spatialVisualizer.gameObject.activeInHierarchy)
        {
            statsInfo += "状态: 活跃\n";
            // 这里可以添加更多统计信息
        }
        else
        {
            statsInfo += "状态: 非活跃\n";
        }
        
        statisticsText.text = statsInfo;
    }

    /// <summary>
    /// 更新性能显示
    /// </summary>
    void UpdatePerformanceDisplay()
    {
        if (performanceText == null) return;
        
        if (Time.time - lastPerformanceUpdateTime > 1.0f)
        {
            int currentFrameCount = Time.frameCount;
            lastFrameRate = (currentFrameCount - lastFrameCount) / (Time.time - lastPerformanceUpdateTime);
            lastFrameCount = currentFrameCount;
            lastPerformanceUpdateTime = Time.time;
            
            string perfInfo = $"性能信息:\n";
            perfInfo += $"帧率: {lastFrameRate:F1} FPS\n";
            perfInfo += $"内存: {(System.GC.GetTotalMemory(false) / 1024 / 1024):F1} MB";
            
            performanceText.text = perfInfo;
        }
    }

    /// <summary>
    /// 更新状态文本
    /// </summary>
    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        }
        Debug.Log($"SpatialFPI: {message}");
    }

    /// <summary>
    /// 获取可视化模式显示名称
    /// </summary>
    string GetVisualizationModeDisplayName(SpatialFPIVisualizer.VisualizationMode mode)
    {
        switch (mode)
        {
            case SpatialFPIVisualizer.VisualizationMode.Points:
                return "点云显示";
            case SpatialFPIVisualizer.VisualizationMode.Voxels:
                return "体素显示";
            case SpatialFPIVisualizer.VisualizationMode.Isosurface:
                return "等值面显示";
            case SpatialFPIVisualizer.VisualizationMode.VolumeRender:
                return "体积渲染";
            default:
                return mode.ToString();
        }
    }

    /// <summary>
    /// 获取颜色方案显示名称
    /// </summary>
    string GetColorSchemeDisplayName(SpatialFPIVisualizer.ColorScheme scheme)
    {
        switch (scheme)
        {
            case SpatialFPIVisualizer.ColorScheme.Heat:
                return "热度图";
            case SpatialFPIVisualizer.ColorScheme.Rainbow:
                return "彩虹色";
            case SpatialFPIVisualizer.ColorScheme.Grayscale:
                return "灰度";
            case SpatialFPIVisualizer.ColorScheme.Custom:
                return "自定义";
            default:
                return scheme.ToString();
        }
    }

    /// <summary>
    /// 截图协程
    /// </summary>
    System.Collections.IEnumerator TakeScreenshotCoroutine()
    {
        yield return new WaitForEndOfFrame();
        
        string fileName = $"SpatialFPI_Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, fileName);
        
        ScreenCapture.CaptureScreenshot(filePath);
        UpdateStatusText($"截图已保存: {fileName}");
    }

    /// <summary>
    /// 预设配置方法
    /// </summary>
    [ContextMenu("应用高质量预设")]
    public void ApplyHighQualityPreset()
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.visualizationMode = SpatialFPIVisualizer.VisualizationMode.Voxels;
            spatialVisualizer.colorScheme = SpatialFPIVisualizer.ColorScheme.Heat;
            spatialVisualizer.pointSize = 0.1f;
            spatialVisualizer.transparency = 0.9f;
            spatialVisualizer.maxVisiblePoints = 10000;
            spatialVisualizer.enableLOD = true;
            
            UpdateUI();
            RefreshVisualization();
            UpdateStatusText("已应用高质量预设");
        }
    }

    [ContextMenu("应用性能优化预设")]
    public void ApplyPerformancePreset()
    {
        if (spatialVisualizer != null)
        {
            spatialVisualizer.visualizationMode = SpatialFPIVisualizer.VisualizationMode.Points;
            spatialVisualizer.colorScheme = SpatialFPIVisualizer.ColorScheme.Heat;
            spatialVisualizer.pointSize = 0.05f;
            spatialVisualizer.transparency = 0.7f;
            spatialVisualizer.maxVisiblePoints = 2000;
            spatialVisualizer.enableLOD = true;
            
            UpdateUI();
            RefreshVisualization();
            UpdateStatusText("已应用性能优化预设");
        }
    }

    /// <summary>
    /// 重置到默认设置
    /// </summary>
    [ContextMenu("重置到默认设置")]
    public void ResetToDefaults()
    {
        SetDefaultValues();
        UpdateUI();
        RefreshVisualization();
        UpdateStatusText("已重置到默认设置");
    }
} 