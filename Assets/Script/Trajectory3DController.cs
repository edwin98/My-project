using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DrillingData;
using TrajectoryPoint = DrillingData.TrajectoryPoint;

namespace DrillingData
{
    /// <summary>
    /// 三维轨迹可视化UI控制器
    /// </summary>
    public class Trajectory3DController : MonoBehaviour
    {
        [Header("组件引用")]
        public Trajectory3DVisualizer visualizer;
        public UnityDrillingDataProcessor dataProcessor;
        public Camera targetCamera;
        
        [Header("UI控制")]
        public Button resetViewButton;
        public Button autoRotateButton;
        public Button toggleLineButton;
        public Button togglePointsButton;
        public Button toggleLabelsButton;
        public Button toggleAxisButton;
        
        [Header("颜色方案控制")]
        public Dropdown colorSchemeDropdown;
        public Slider lineWidthSlider;
        public Slider pointSizeSlider;
        
        [Header("相机控制")]
        public Slider rotationSpeedSlider;
        public Slider zoomSpeedSlider;
        public Slider autoRotateSpeedSlider;
        
        [Header("状态显示")]
        public Text statusText;
        public Text pointCountText;
        public Text depthRangeText;
        
        // 私有变量
        private bool isAutoRotating = false;
        private bool showTrajectoryLine = true;
        private bool showTrajectoryPoints = true;
        private bool showDepthLabels = true;
        private bool showCoordinateAxis = true;
        
        void Start()
        {
            InitializeUI();
            SetupEventHandlers();
            UpdateUI();
        }
        
        void Update()
        {
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// 初始化UI
        /// </summary>
        private void InitializeUI()
        {
            // 初始化颜色方案下拉框
            if (colorSchemeDropdown != null)
            {
                colorSchemeDropdown.ClearOptions();
                colorSchemeDropdown.AddOptions(new List<string> { "深度", "倾角", "方位角", "自定义" });
                colorSchemeDropdown.value = 0;
            }
            
            // 初始化滑块
            if (lineWidthSlider != null)
            {
                lineWidthSlider.minValue = 0.01f;
                lineWidthSlider.maxValue = 1f;
                lineWidthSlider.value = 0.1f;
            }
            
            if (pointSizeSlider != null)
            {
                pointSizeSlider.minValue = 0.1f;
                pointSizeSlider.maxValue = 2f;
                pointSizeSlider.value = 0.5f;
            }
            
            if (rotationSpeedSlider != null)
            {
                rotationSpeedSlider.minValue = 10f;
                rotationSpeedSlider.maxValue = 500f;
                rotationSpeedSlider.value = 100f;
            }
            
            if (zoomSpeedSlider != null)
            {
                zoomSpeedSlider.minValue = 1f;
                zoomSpeedSlider.maxValue = 50f;
                zoomSpeedSlider.value = 10f;
            }
            
            if (autoRotateSpeedSlider != null)
            {
                autoRotateSpeedSlider.minValue = 10f;
                autoRotateSpeedSlider.maxValue = 100f;
                autoRotateSpeedSlider.value = 30f;
            }
        }
        
        /// <summary>
        /// 设置事件处理器
        /// </summary>
        private void SetupEventHandlers()
        {
            // 按钮事件
            if (resetViewButton != null)
                resetViewButton.onClick.AddListener(OnResetViewClicked);
            
            if (autoRotateButton != null)
                autoRotateButton.onClick.AddListener(OnAutoRotateClicked);
            
            if (toggleLineButton != null)
                toggleLineButton.onClick.AddListener(OnToggleLineClicked);
            
            if (togglePointsButton != null)
                togglePointsButton.onClick.AddListener(OnTogglePointsClicked);
            
            if (toggleLabelsButton != null)
                toggleLabelsButton.onClick.AddListener(OnToggleLabelsClicked);
            
            if (toggleAxisButton != null)
                toggleAxisButton.onClick.AddListener(OnToggleAxisClicked);
            
            // 下拉框事件
            if (colorSchemeDropdown != null)
                colorSchemeDropdown.onValueChanged.AddListener(OnColorSchemeChanged);
            
            // 滑块事件
            if (lineWidthSlider != null)
                lineWidthSlider.onValueChanged.AddListener(OnLineWidthChanged);
            
            if (pointSizeSlider != null)
                pointSizeSlider.onValueChanged.AddListener(OnPointSizeChanged);
            
            if (rotationSpeedSlider != null)
                rotationSpeedSlider.onValueChanged.AddListener(OnRotationSpeedChanged);
            
            if (zoomSpeedSlider != null)
                zoomSpeedSlider.onValueChanged.AddListener(OnZoomSpeedChanged);
            
            if (autoRotateSpeedSlider != null)
                autoRotateSpeedSlider.onValueChanged.AddListener(OnAutoRotateSpeedChanged);
        }
        
        /// <summary>
        /// 更新UI状态
        /// </summary>
        private void UpdateUI()
        {
            // 更新按钮文本
            if (autoRotateButton != null)
            {
                autoRotateButton.GetComponentInChildren<Text>().text = isAutoRotating ? "停止旋转" : "开始旋转";
            }
            
            if (toggleLineButton != null)
            {
                toggleLineButton.GetComponentInChildren<Text>().text = showTrajectoryLine ? "隐藏轨迹线" : "显示轨迹线";
            }
            
            if (togglePointsButton != null)
            {
                togglePointsButton.GetComponentInChildren<Text>().text = showTrajectoryPoints ? "隐藏轨迹点" : "显示轨迹点";
            }
            
            if (toggleLabelsButton != null)
            {
                toggleLabelsButton.GetComponentInChildren<Text>().text = showDepthLabels ? "隐藏深度标签" : "显示深度标签";
            }
            
            if (toggleAxisButton != null)
            {
                toggleAxisButton.GetComponentInChildren<Text>().text = showCoordinateAxis ? "隐藏坐标轴" : "显示坐标轴";
            }
        }
        
        /// <summary>
        /// 更新状态显示
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (dataProcessor == null) return;
            
            var trajectoryPoints = dataProcessor.GetTrajectoryPoints();
            
            if (pointCountText != null)
            {
                pointCountText.text = $"轨迹点数量: {trajectoryPoints.Count}";
            }
            
            if (depthRangeText != null && trajectoryPoints.Count > 0)
            {
                float minDepth = trajectoryPoints[0].verticalDepth;
                float maxDepth = trajectoryPoints[0].verticalDepth;
                
                foreach (var point in trajectoryPoints)
                {
                    if (point.verticalDepth < minDepth) minDepth = point.verticalDepth;
                    if (point.verticalDepth > maxDepth) maxDepth = point.verticalDepth;
                }
                
                depthRangeText.text = $"深度范围: {minDepth:F1}m - {maxDepth:F1}m";
            }
            
            if (statusText != null)
            {
                string status = "就绪";
                if (visualizer != null)
                {
                    status = $"可视化状态: {(visualizer.enabled ? "已启用" : "已禁用")}";
                }
                statusText.text = status;
            }
        }
        
        // 事件处理方法
        private void OnResetViewClicked()
        {
            if (visualizer != null)
            {
                visualizer.ResetView();
            }
        }
        
        private void OnAutoRotateClicked()
        {
            isAutoRotating = !isAutoRotating;
            if (visualizer != null)
            {
                visualizer.autoRotate = isAutoRotating;
            }
            UpdateUI();
        }
        
        private void OnToggleLineClicked()
        {
            showTrajectoryLine = !showTrajectoryLine;
            if (visualizer != null)
            {
                visualizer.showTrajectoryLine = showTrajectoryLine;
                visualizer.UpdateFromProcessor();
            }
            UpdateUI();
        }
        
        private void OnTogglePointsClicked()
        {
            showTrajectoryPoints = !showTrajectoryPoints;
            if (visualizer != null)
            {
                visualizer.showTrajectoryPoints = showTrajectoryPoints;
                visualizer.UpdateFromProcessor();
            }
            UpdateUI();
        }
        
        private void OnToggleLabelsClicked()
        {
            showDepthLabels = !showDepthLabels;
            if (visualizer != null)
            {
                visualizer.showDepthLabels = showDepthLabels;
                visualizer.UpdateFromProcessor();
            }
            UpdateUI();
        }
        
        private void OnToggleAxisClicked()
        {
            showCoordinateAxis = !showCoordinateAxis;
            if (visualizer != null)
            {
                visualizer.showCoordinateAxis = showCoordinateAxis;
                visualizer.UpdateFromProcessor();
            }
            UpdateUI();
        }
        
        private void OnColorSchemeChanged(int index)
        {
            if (visualizer != null)
            {
                visualizer.colorScheme = (Trajectory3DVisualizer.ColorScheme)index;
                visualizer.UpdateFromProcessor();
            }
        }
        
        private void OnLineWidthChanged(float value)
        {
            if (visualizer != null)
            {
                visualizer.lineWidth = value;
                visualizer.UpdateFromProcessor();
            }
        }
        
        private void OnPointSizeChanged(float value)
        {
            if (visualizer != null)
            {
                visualizer.pointSize = value;
                visualizer.UpdateFromProcessor();
            }
        }
        
        private void OnRotationSpeedChanged(float value)
        {
            if (visualizer != null)
            {
                visualizer.rotationSpeed = value;
            }
        }
        
        private void OnZoomSpeedChanged(float value)
        {
            if (visualizer != null)
            {
                visualizer.zoomSpeed = value;
            }
        }
        
        private void OnAutoRotateSpeedChanged(float value)
        {
            if (visualizer != null)
            {
                visualizer.autoRotateSpeed = value;
            }
        }
        
        /// <summary>
        /// 手动更新可视化
        /// </summary>
        [ContextMenu("更新可视化")]
        public void UpdateVisualization()
        {
            if (visualizer != null && dataProcessor != null)
            {
                visualizer.UpdateFromProcessor();
            }
        }
        
        /// <summary>
        /// 设置数据处理器
        /// </summary>
        public void SetDataProcessor(UnityDrillingDataProcessor processor)
        {
            dataProcessor = processor;
            if (visualizer != null)
            {
                visualizer.dataProcessor = processor;
            }
        }
        
        /// <summary>
        /// 设置可视化器
        /// </summary>
        public void SetVisualizer(Trajectory3DVisualizer viz)
        {
            visualizer = viz;
            if (viz != null && dataProcessor != null)
            {
                viz.dataProcessor = dataProcessor;
            }
        }
        
        /// <summary>
        /// 设置目标相机
        /// </summary>
        public void SetTargetCamera(Camera camera)
        {
            targetCamera = camera;
            if (visualizer != null)
            {
                visualizer.targetCamera = camera;
            }
        }
    }
} 