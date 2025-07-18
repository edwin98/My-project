using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// 集成的钻井数据可视化器
/// 同时显示轨迹数据和空间FPI插值结果
/// </summary>
public class IntegratedDrillingVisualizer : MonoBehaviour
{
    [Header("组件引用")]
    public TrajectoryVisualizer trajectoryVisualizer;
    public SpatialFPIVisualizer spatialFPIVisualizer;
    public Camera mainCamera;
    public Canvas uiCanvas;

    [Header("数据同步")]
    public bool autoLoadDataOnStart = true;
    public bool syncTrajectoryAndSpatial = true;
    public string trajectoryDataFile = "trajectory_data.json";
    public string spatialDataFile = "spatial_fpi_visualization.json";

    [Header("显示控制")]
    public bool showTrajectory = true;
    public bool showSpatialInterpolation = true;
    public bool showLegend = true;
    public bool showStatistics = true;

    [Header("交互控制")]
    public float cameraRotationSpeed = 100f;
    public float cameraZoomSpeed = 10f;
    public float cameraPanSpeed = 5f;
    public bool enableAutoRotate = false;
    public float autoRotateSpeed = 30f;

    [Header("UI面板")]
    public GameObject controlPanel;
    public GameObject legendPanel;
    public GameObject statisticsPanel;

    // 私有变量
    private bool isInitialized = false;
    private Vector3 lastMousePosition;
    private bool isDragging = false;
    private bool isDataLoaded = false;
    private Coroutine autoRotateCoroutine;

    // UI组件
    private Toggle trajectoryToggle;
    private Toggle spatialToggle;
    private Toggle legendToggle;
    private Button resetViewButton;
    private Button autoRotateButton;
    private Text statusText;
    private Text statisticsText;

    void Start()
    {
        Initialize();
        
        if (autoLoadDataOnStart)
        {
            StartCoroutine(LoadDataCoroutine());
        }
    }

    void Update()
    {
        HandleCameraControls();
        UpdateUI();
    }

    /// <summary>
    /// 初始化可视化器
    /// </summary>
    void Initialize()
    {
        Debug.Log("初始化集成钻井数据可视化器...");

        // 初始化相机
        InitializeCamera();

        // 初始化可视化组件
        InitializeVisualizers();

        // 创建UI
        CreateUI();

        // 设置事件监听
        SetupEventHandlers();

        isInitialized = true;
        Debug.Log("集成可视化器初始化完成");
    }

    /// <summary>
    /// 初始化相机
    /// </summary>
    void InitializeCamera()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }
        }

        if (mainCamera != null)
        {
            // 设置相机初始位置和角度
            mainCamera.transform.position = new Vector3(20f, 15f, 20f);
            mainCamera.transform.LookAt(Vector3.zero);
            
            // 确保相机有适当的设置
            mainCamera.clearFlags = CameraClearFlags.Skybox;
            mainCamera.fieldOfView = 60f;
            mainCamera.nearClipPlane = 0.1f;
            mainCamera.farClipPlane = 1000f;
        }
    }

    /// <summary>
    /// 初始化可视化组件
    /// </summary>
    void InitializeVisualizers()
    {
        // 查找或创建轨迹可视化器
        if (trajectoryVisualizer == null)
        {
            trajectoryVisualizer = FindObjectOfType<TrajectoryVisualizer>();
            if (trajectoryVisualizer == null)
            {
                GameObject trajectoryObj = new GameObject("TrajectoryVisualizer");
                trajectoryObj.transform.SetParent(transform);
                trajectoryVisualizer = trajectoryObj.AddComponent<TrajectoryVisualizer>();
            }
        }

        // 查找或创建空间FPI可视化器
        if (spatialFPIVisualizer == null)
        {
            spatialFPIVisualizer = FindObjectOfType<SpatialFPIVisualizer>();
            if (spatialFPIVisualizer == null)
            {
                GameObject spatialObj = new GameObject("SpatialFPIVisualizer");
                spatialObj.transform.SetParent(transform);
                spatialFPIVisualizer = spatialObj.AddComponent<SpatialFPIVisualizer>();
            }
        }

        // 配置可视化器
        ConfigureVisualizers();
    }

    /// <summary>
    /// 配置可视化器
    /// </summary>
    void ConfigureVisualizers()
    {
        // 配置轨迹可视化器
        if (trajectoryVisualizer != null)
        {
            trajectoryVisualizer.jsonFileName = trajectoryDataFile;
            trajectoryVisualizer.loadFromStreamingAssets = true;
            trajectoryVisualizer.showTrajectoryLine = showTrajectory;
            trajectoryVisualizer.showTrajectoryPoints = showTrajectory;
            trajectoryVisualizer.showDepthLabels = showTrajectory;
            trajectoryVisualizer.showCoordinateAxis = true;
        }

        // 配置空间FPI可视化器
        if (spatialFPIVisualizer != null)
        {
            spatialFPIVisualizer.spatialDataFileName = spatialDataFile;
            spatialFPIVisualizer.loadFromStreamingAssets = true;
            spatialFPIVisualizer.autoLoadOnStart = false; // 我们会手动控制加载
            spatialFPIVisualizer.showSpatialPoints = showSpatialInterpolation;
            spatialFPIVisualizer.showBoundingBox = showSpatialInterpolation;
            spatialFPIVisualizer.showColorLegend = showLegend;
        }
    }

    /// <summary>
    /// 创建UI
    /// </summary>
    void CreateUI()
    {
        // 查找或创建Canvas
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas == null)
            {
                GameObject canvasObj = new GameObject("UI Canvas");
                uiCanvas = canvasObj.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }

        // 创建主控制面板
        CreateControlPanel();

        // 创建图例面板
        CreateLegendPanel();

        // 创建统计面板
        CreateStatisticsPanel();
    }

    /// <summary>
    /// 创建控制面板
    /// </summary>
    void CreateControlPanel()
    {
        if (controlPanel == null)
        {
            controlPanel = new GameObject("ControlPanel");
            controlPanel.transform.SetParent(uiCanvas.transform);

            // 设置面板属性
            RectTransform panelRect = controlPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.7f);
            panelRect.anchorMax = new Vector2(0.3f, 1f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            // 添加背景
            Image bgImage = controlPanel.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.7f);

            // 创建控制按钮
            CreateControlButtons();
        }
    }

    /// <summary>
    /// 创建控制按钮
    /// </summary>
    void CreateControlButtons()
    {
        // 轨迹显示切换
        trajectoryToggle = CreateToggle(controlPanel, "轨迹显示", new Vector2(10, -30), showTrajectory);
        trajectoryToggle.onValueChanged.AddListener(OnTrajectoryToggleChanged);

        // 空间插值显示切换
        spatialToggle = CreateToggle(controlPanel, "空间插值", new Vector2(10, -60), showSpatialInterpolation);
        spatialToggle.onValueChanged.AddListener(OnSpatialToggleChanged);

        // 图例显示切换
        legendToggle = CreateToggle(controlPanel, "显示图例", new Vector2(10, -90), showLegend);
        legendToggle.onValueChanged.AddListener(OnLegendToggleChanged);

        // 重置视图按钮
        resetViewButton = CreateButton(controlPanel, "重置视图", new Vector2(10, -120));
        resetViewButton.onClick.AddListener(ResetView);

        // 自动旋转按钮
        autoRotateButton = CreateButton(controlPanel, "自动旋转", new Vector2(120, -120));
        autoRotateButton.onClick.AddListener(ToggleAutoRotate);

        // 状态文本
        statusText = CreateText(controlPanel, "状态: 准备就绪", new Vector2(10, -150));
    }

    /// <summary>
    /// 创建图例面板
    /// </summary>
    void CreateLegendPanel()
    {
        if (legendPanel == null)
        {
            legendPanel = new GameObject("LegendPanel");
            legendPanel.transform.SetParent(uiCanvas.transform);

            RectTransform panelRect = legendPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.7f, 0.3f);
            panelRect.anchorMax = new Vector2(1f, 0.7f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image bgImage = legendPanel.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.5f);

            // 图例内容将由空间FPI可视化器创建
            legendPanel.SetActive(showLegend);
        }
    }

    /// <summary>
    /// 创建统计面板
    /// </summary>
    void CreateStatisticsPanel()
    {
        if (statisticsPanel == null)
        {
            statisticsPanel = new GameObject("StatisticsPanel");
            statisticsPanel.transform.SetParent(uiCanvas.transform);

            RectTransform panelRect = statisticsPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0);
            panelRect.anchorMax = new Vector2(0.3f, 0.3f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            Image bgImage = statisticsPanel.AddComponent<Image>();
            bgImage.color = new Color(0, 0, 0, 0.6f);

            // 创建统计文本
            statisticsText = CreateText(statisticsPanel, "数据统计:\n等待加载...", new Vector2(10, -20));
            statisticsText.fontSize = 12;

            statisticsPanel.SetActive(showStatistics);
        }
    }

    /// <summary>
    /// 设置事件处理器
    /// </summary>
    void SetupEventHandlers()
    {
        // 这里可以添加更多事件监听
    }

    /// <summary>
    /// 加载数据协程
    /// </summary>
    public IEnumerator LoadDataCoroutine()
    {
        UpdateStatus("正在加载数据...");

        // 加载轨迹数据
        if (trajectoryVisualizer != null && showTrajectory)
        {
            trajectoryVisualizer.LoadTrajectoryData();
            yield return new WaitForSeconds(0.5f); // 等待加载完成
        }

        // 加载空间插值数据
        if (spatialFPIVisualizer != null && showSpatialInterpolation)
        {
            spatialFPIVisualizer.LoadSpatialData();
            yield return new WaitForSeconds(0.5f); // 等待加载完成
        }

        // 同步可视化
        if (syncTrajectoryAndSpatial)
        {
            SynchronizeVisualizations();
        }

        // 调整相机视图
        yield return new WaitForSeconds(0.5f);
        FitViewToData();

        isDataLoaded = true;
        UpdateStatus("数据加载完成");
        UpdateStatistics();
    }

    /// <summary>
    /// 同步可视化
    /// </summary>
    void SynchronizeVisualizations()
    {
        if (trajectoryVisualizer != null && spatialFPIVisualizer != null)
        {
            // 确保两个可视化器在同一个坐标系中
            Vector3 trajectoryCenter = trajectoryVisualizer.transform.position;
            spatialFPIVisualizer.transform.position = trajectoryCenter;

            Debug.Log("轨迹和空间插值可视化已同步");
        }
    }

    /// <summary>
    /// 处理相机控制
    /// </summary>
    void HandleCameraControls()
    {
        if (mainCamera == null) return;

        // 鼠标旋转
        if (Input.GetMouseButtonDown(0))
        {
            lastMousePosition = Input.mousePosition;
            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (isDragging)
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            
            // 水平旋转
            mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, 
                mouseDelta.x * cameraRotationSpeed * Time.deltaTime);
            
            // 垂直旋转
            mainCamera.transform.RotateAround(Vector3.zero, mainCamera.transform.right, 
                -mouseDelta.y * cameraRotationSpeed * Time.deltaTime);
            
            lastMousePosition = Input.mousePosition;
        }

        // 鼠标滚轮缩放
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            Vector3 forward = mainCamera.transform.forward;
            mainCamera.transform.position += forward * scroll * cameraZoomSpeed;
        }

        // 中键平移
        if (Input.GetMouseButton(2))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            Vector3 moveDirection = -mainCamera.transform.right * mouseDelta.x * cameraPanSpeed * Time.deltaTime;
            moveDirection += -mainCamera.transform.up * mouseDelta.y * cameraPanSpeed * Time.deltaTime;
            
            mainCamera.transform.position += moveDirection;
            lastMousePosition = Input.mousePosition;
        }

        // 键盘快捷键
        HandleKeyboardShortcuts();
    }

    /// <summary>
    /// 处理键盘快捷键
    /// </summary>
    void HandleKeyboardShortcuts()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetView();
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            ToggleTrajectoryVisibility();
        }
        
        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleSpatialVisibility();
        }
        
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleLegendVisibility();
        }
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ToggleAutoRotate();
        }
    }

    /// <summary>
    /// 更新UI
    /// </summary>
    void UpdateUI()
    {
        if (!isInitialized) return;

        // 更新切换按钮状态
        if (trajectoryToggle != null)
            trajectoryToggle.isOn = showTrajectory;
        
        if (spatialToggle != null)
            spatialToggle.isOn = showSpatialInterpolation;
        
        if (legendToggle != null)
            legendToggle.isOn = showLegend;

        // 更新面板可见性
        if (legendPanel != null)
            legendPanel.SetActive(showLegend);
        
        if (statisticsPanel != null)
            statisticsPanel.SetActive(showStatistics);
    }

    #region 事件处理方法

    void OnTrajectoryToggleChanged(bool value)
    {
        showTrajectory = value;
        
        if (trajectoryVisualizer != null)
        {
            trajectoryVisualizer.showTrajectoryLine = value;
            trajectoryVisualizer.showTrajectoryPoints = value;
            trajectoryVisualizer.showDepthLabels = value;
            trajectoryVisualizer.gameObject.SetActive(value);
        }
        
        UpdateStatus($"轨迹显示: {(value ? "开启" : "关闭")}");
    }

    void OnSpatialToggleChanged(bool value)
    {
        showSpatialInterpolation = value;
        
        if (spatialFPIVisualizer != null)
        {
            spatialFPIVisualizer.showSpatialPoints = value;
            spatialFPIVisualizer.showBoundingBox = value;
            spatialFPIVisualizer.gameObject.SetActive(value);
        }
        
        UpdateStatus($"空间插值显示: {(value ? "开启" : "关闭")}");
    }

    void OnLegendToggleChanged(bool value)
    {
        showLegend = value;
        
        if (spatialFPIVisualizer != null)
        {
            spatialFPIVisualizer.showColorLegend = value;
        }
        
        UpdateStatus($"图例显示: {(value ? "开启" : "关闭")}");
    }

    #endregion

    #region 公共控制方法

    /// <summary>
    /// 重置视图
    /// </summary>
    public void ResetView()
    {
        if (mainCamera != null)
        {
            mainCamera.transform.position = new Vector3(20f, 15f, 20f);
            mainCamera.transform.LookAt(Vector3.zero);
            UpdateStatus("视图已重置");
        }
    }

    /// <summary>
    /// 适应数据视图
    /// </summary>
    public void FitViewToData()
    {
        if (mainCamera == null) return;

        // 计算数据边界
        UnityEngine.Bounds combinedBounds = CalculateCombinedBounds();
        
        if (combinedBounds.size.magnitude > 0)
        {
            // 调整相机位置以适应数据
            float distance = combinedBounds.size.magnitude * 1.5f;
            Vector3 direction = (mainCamera.transform.position - combinedBounds.center).normalized;
            mainCamera.transform.position = combinedBounds.center + direction * distance;
            mainCamera.transform.LookAt(combinedBounds.center);
            
            UpdateStatus("视图已适应数据范围");
        }
    }

    /// <summary>
    /// 切换自动旋转
    /// </summary>
    public void ToggleAutoRotate()
    {
        enableAutoRotate = !enableAutoRotate;
        
        if (enableAutoRotate)
        {
            if (autoRotateCoroutine == null)
            {
                autoRotateCoroutine = StartCoroutine(AutoRotateCoroutine());
            }
            UpdateStatus("自动旋转已启用");
        }
        else
        {
            if (autoRotateCoroutine != null)
            {
                StopCoroutine(autoRotateCoroutine);
                autoRotateCoroutine = null;
            }
            UpdateStatus("自动旋转已禁用");
        }
    }

    /// <summary>
    /// 切换轨迹可见性
    /// </summary>
    public void ToggleTrajectoryVisibility()
    {
        if (trajectoryToggle != null)
        {
            trajectoryToggle.isOn = !trajectoryToggle.isOn;
        }
    }

    /// <summary>
    /// 切换空间插值可见性
    /// </summary>
    public void ToggleSpatialVisibility()
    {
        if (spatialToggle != null)
        {
            spatialToggle.isOn = !spatialToggle.isOn;
        }
    }

    /// <summary>
    /// 切换图例可见性
    /// </summary>
    public void ToggleLegendVisibility()
    {
        if (legendToggle != null)
        {
            legendToggle.isOn = !legendToggle.isOn;
        }
    }

    /// <summary>
    /// 重新加载数据
    /// </summary>
    public void ReloadData()
    {
        isDataLoaded = false;
        StartCoroutine(LoadDataCoroutine());
    }

    #endregion

    #region 辅助方法

    /// <summary>
    /// 计算组合边界
    /// </summary>
    UnityEngine.Bounds CalculateCombinedBounds()
    {
        UnityEngine.Bounds bounds = new UnityEngine.Bounds();
        bool boundsInitialized = false;

        // 包含轨迹数据边界
        if (trajectoryVisualizer != null && showTrajectory)
        {
            // 这里需要从轨迹可视化器获取边界信息
            // 暂时使用默认值
            if (!boundsInitialized)
            {
                bounds = new UnityEngine.Bounds(Vector3.zero, Vector3.one * 50f);
                boundsInitialized = true;
            }
        }

        // 包含空间插值数据边界
        if (spatialFPIVisualizer != null && showSpatialInterpolation)
        {
            // 这里需要从空间FPI可视化器获取边界信息
            // 暂时使用默认值扩展
            if (boundsInitialized)
            {
                bounds.Encapsulate(new UnityEngine.Bounds(Vector3.zero, Vector3.one * 60f));
            }
        }

        return bounds;
    }

    /// <summary>
    /// 自动旋转协程
    /// </summary>
    IEnumerator AutoRotateCoroutine()
    {
        while (enableAutoRotate)
        {
            if (mainCamera != null)
            {
                mainCamera.transform.RotateAround(Vector3.zero, Vector3.up, 
                    autoRotateSpeed * Time.deltaTime);
            }
            yield return null;
        }
    }

    /// <summary>
    /// 更新状态
    /// </summary>
    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"状态: {message}";
        }
        Debug.Log($"IntegratedVisualizer: {message}");
    }

    /// <summary>
    /// 更新统计信息
    /// </summary>
    void UpdateStatistics()
    {
        if (statisticsText == null || !isDataLoaded) return;

        string stats = "数据统计:\n";
        
        // 轨迹统计
        if (trajectoryVisualizer != null && showTrajectory)
        {
            stats += "轨迹点数: 加载中...\n";
            // 这里可以添加实际的轨迹统计信息
        }

        // 空间插值统计
        if (spatialFPIVisualizer != null && showSpatialInterpolation)
        {
            stats += "插值点数: 加载中...\n";
            // 这里可以添加实际的插值统计信息
        }

        statisticsText.text = stats;
    }

    /// <summary>
    /// 创建切换按钮
    /// </summary>
    Toggle CreateToggle(GameObject parent, string text, Vector2 position, bool initialValue)
    {
        GameObject toggleObj = new GameObject($"Toggle_{text}");
        toggleObj.transform.SetParent(parent.transform);

        RectTransform rect = toggleObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(150, 20);

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = initialValue;

        // 添加背景
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.white;
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(20, 20);
        bgRect.anchoredPosition = Vector2.zero;

        // 添加标记
        GameObject checkmarkObj = new GameObject("Checkmark");
        checkmarkObj.transform.SetParent(bgObj.transform);
        Image checkImage = checkmarkObj.AddComponent<Image>();
        checkImage.color = Color.green;
        RectTransform checkRect = checkmarkObj.GetComponent<RectTransform>();
        checkRect.sizeDelta = new Vector2(16, 16);
        checkRect.anchoredPosition = Vector2.zero;

        // 添加文本
        GameObject textObj = new GameObject("Label");
        textObj.transform.SetParent(toggleObj.transform);
        Text labelText = textObj.AddComponent<Text>();
        labelText.text = text;
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 12;
        labelText.color = Color.white;
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = new Vector2(120, 20);
        textRect.anchoredPosition = new Vector2(25, 0);

        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;

        return toggle;
    }

    /// <summary>
    /// 创建按钮
    /// </summary>
    Button CreateButton(GameObject parent, string text, Vector2 position)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent.transform);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(100, 25);

        Button button = buttonObj.AddComponent<Button>();
        
        Image bgImage = buttonObj.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 12;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.sizeDelta = rect.sizeDelta;
        textRect.anchoredPosition = Vector2.zero;

        button.targetGraphic = bgImage;

        return button;
    }

    /// <summary>
    /// 创建文本
    /// </summary>
    Text CreateText(GameObject parent, string content, Vector2 position)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(280, 100);

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 14;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;

        return text;
    }

    #endregion

    void OnDestroy()
    {
        if (autoRotateCoroutine != null)
        {
            StopCoroutine(autoRotateCoroutine);
        }
    }
} 