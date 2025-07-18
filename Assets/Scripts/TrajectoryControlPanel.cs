using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 轨迹可视化控制面板
/// 提供基本的轨迹显示控制功能
/// </summary>
public class TrajectoryControlPanel : MonoBehaviour
{
    [Header("基础控制UI")]
    public Button loadDataButton;
    public Toggle trajectoryLineToggle;
    public Toggle trajectoryPointsToggle;
    public Toggle coordinateAxisToggle;
    public Toggle depthLabelsToggle;
    public Button resetViewButton;
    public TMP_InputField fileNameInput;
    
    [Header("信息显示")]
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI statusText;
    
    [Header("轨迹可视化器")]
    public TrajectoryVisualizer trajectoryVisualizer;

    void Start()
    {
        SetupUI();
        RegisterEvents();
        UpdateUI();
    }

    /// <summary>
    /// 设置UI
    /// </summary>
    void SetupUI()
    {
        if (trajectoryVisualizer == null)
        {
            trajectoryVisualizer = FindObjectOfType<TrajectoryVisualizer>();
        }
        
        // 设置默认值
        if (fileNameInput != null)
        {
            fileNameInput.text = "trajectory_data.json";
        }
        
        UpdateStatusText("轨迹可视化控制面板就绪");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvents()
    {
        // 基础控制按钮
        if (loadDataButton != null)
            loadDataButton.onClick.AddListener(LoadTrajectoryData);
        
        if (resetViewButton != null)
            resetViewButton.onClick.AddListener(ResetView);
        
        // 显示切换
        if (trajectoryLineToggle != null)
            trajectoryLineToggle.onValueChanged.AddListener(OnTrajectoryLineToggle);
        
        if (trajectoryPointsToggle != null)
            trajectoryPointsToggle.onValueChanged.AddListener(OnTrajectoryPointsToggle);
        
        if (coordinateAxisToggle != null)
            coordinateAxisToggle.onValueChanged.AddListener(OnCoordinateAxisToggle);
        
        if (depthLabelsToggle != null)
            depthLabelsToggle.onValueChanged.AddListener(OnDepthLabelsToggle);
    }

    /// <summary>
    /// 更新UI状态
    /// </summary>
    void UpdateUI()
    {
        if (trajectoryVisualizer == null) return;
        
        // 更新切换按钮状态
        if (trajectoryLineToggle != null)
            trajectoryLineToggle.isOn = trajectoryVisualizer.showTrajectoryLine;
        
        if (trajectoryPointsToggle != null)
            trajectoryPointsToggle.isOn = trajectoryVisualizer.showTrajectoryPoints;
        
        if (coordinateAxisToggle != null)
            coordinateAxisToggle.isOn = trajectoryVisualizer.showCoordinateAxis;
        
        if (depthLabelsToggle != null)
            depthLabelsToggle.isOn = trajectoryVisualizer.showDepthLabels;
        
        // 更新信息显示
        UpdateInfoText();
    }

    /// <summary>
    /// 加载轨迹数据
    /// </summary>
    void LoadTrajectoryData()
    {
        UpdateStatusText("正在加载轨迹数据...");
        
        if (trajectoryVisualizer != null)
        {
            if (fileNameInput != null && !string.IsNullOrEmpty(fileNameInput.text))
            {
                trajectoryVisualizer.jsonFileName = fileNameInput.text;
            }
            
            // 重新加载数据
            trajectoryVisualizer.LoadTrajectoryData();
            trajectoryVisualizer.SetupVisualization();
            
            UpdateInfoText();
            UpdateStatusText("轨迹数据加载完成");
        }
        else
        {
            UpdateStatusText("错误：未找到轨迹可视化器组件");
        }
    }

    /// <summary>
    /// 重置视图
    /// </summary>
    void ResetView()
    {
        if (trajectoryVisualizer != null)
        {
            // 重新设置可视化以重置视图
            trajectoryVisualizer.SetupVisualization();
            UpdateStatusText("视图已重置");
        }
    }

    /// <summary>
    /// 轨迹线显示切换
    /// </summary>
    void OnTrajectoryLineToggle(bool value)
    {
        if (trajectoryVisualizer != null)
        {
            trajectoryVisualizer.showTrajectoryLine = value;
            trajectoryVisualizer.ToggleTrajectoryLine();
        }
    }

    /// <summary>
    /// 轨迹点显示切换
    /// </summary>
    void OnTrajectoryPointsToggle(bool value)
    {
        if (trajectoryVisualizer != null)
        {
            trajectoryVisualizer.showTrajectoryPoints = value;
            trajectoryVisualizer.ToggleTrajectoryPoints();
        }
    }

    /// <summary>
    /// 坐标轴显示切换
    /// </summary>
    void OnCoordinateAxisToggle(bool value)
    {
        if (trajectoryVisualizer != null)
        {
            trajectoryVisualizer.showCoordinateAxis = value;
            trajectoryVisualizer.ToggleCoordinateAxis();
        }
    }

    /// <summary>
    /// 深度标签显示切换
    /// </summary>
    void OnDepthLabelsToggle(bool value)
    {
        if (trajectoryVisualizer != null)
        {
            trajectoryVisualizer.showDepthLabels = value;
            trajectoryVisualizer.ToggleDepthLabels();
        }
    }

    /// <summary>
    /// 更新信息文本
    /// </summary>
    void UpdateInfoText()
    {
        if (infoText != null && trajectoryVisualizer != null)
        {
            infoText.text = trajectoryVisualizer.GetTrajectoryInfo();
        }
    }

    /// <summary>
    /// 更新状态文本
    /// </summary>
    void UpdateStatusText(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"状态: {message}";
        }
        Debug.Log($"轨迹控制面板: {message}");
    }

    void OnDestroy()
    {
        // 清理事件监听
        if (loadDataButton != null)
            loadDataButton.onClick.RemoveListener(LoadTrajectoryData);
        
        if (resetViewButton != null)
            resetViewButton.onClick.RemoveListener(ResetView);
        
        if (trajectoryLineToggle != null)
            trajectoryLineToggle.onValueChanged.RemoveListener(OnTrajectoryLineToggle);
        
        if (trajectoryPointsToggle != null)
            trajectoryPointsToggle.onValueChanged.RemoveListener(OnTrajectoryPointsToggle);
        
        if (coordinateAxisToggle != null)
            coordinateAxisToggle.onValueChanged.RemoveListener(OnCoordinateAxisToggle);
        
        if (depthLabelsToggle != null)
            depthLabelsToggle.onValueChanged.RemoveListener(OnDepthLabelsToggle);
    }
} 