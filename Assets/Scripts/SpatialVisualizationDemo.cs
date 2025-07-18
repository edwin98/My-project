using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// 空间FPI插值可视化演示脚本
/// 用于快速测试和展示空间插值功能
/// </summary>
public class SpatialVisualizationDemo : MonoBehaviour
{
    [Header("演示设置")]
    public bool autoStartDemo = true;
    public bool createTestData = true;
    public DemoMode demoMode = DemoMode.Integrated;

    [Header("测试数据设置")]
    public int testPointCount = 1000;
    public Vector3 dataRange = new Vector3(20f, 10f, 15f);
    public Vector2 fpiRange = new Vector2(1000000f, 5000000f);

    [Header("UI设置")]
    public bool showDemoUI = true;
    public bool showInstructions = true;

    public enum DemoMode
    {
        SpatialOnly,    // 仅空间插值
        TrajectoryOnly, // 仅轨迹
        Integrated      // 集成显示
    }

    // 私有变量
    private SpatialFPIVisualizer spatialVisualizer;
    private TrajectoryVisualizer trajectoryVisualizer;
    private IntegratedDrillingVisualizer integratedVisualizer;
    private GameObject demoUI;
    private Text statusText;
    private Text instructionText;
    private bool isDemoRunning = false;

    void Start()
    {
        if (autoStartDemo)
        {
            StartCoroutine(StartDemoCoroutine());
        }
    }

    void Update()
    {
        HandleDemoInput();
    }

    /// <summary>
    /// 开始演示协程
    /// </summary>
    IEnumerator StartDemoCoroutine()
    {
        UpdateStatus("初始化演示环境...");
        
        // 1. 创建测试数据
        if (createTestData)
        {
            CreateTestDataFiles();
            yield return new WaitForSeconds(0.5f);
        }

        // 2. 设置场景
        SetupDemoScene();
        yield return new WaitForSeconds(0.5f);

        // 3. 创建UI
        if (showDemoUI)
        {
            CreateDemoUI();
            yield return new WaitForSeconds(0.5f);
        }

        // 4. 根据模式创建可视化器
        CreateVisualizers();
        yield return new WaitForSeconds(1f);

        // 5. 加载和显示数据
        LoadAndDisplayData();
        yield return new WaitForSeconds(1f);

        // 6. 设置相机视图
        SetupCameraView();

        isDemoRunning = true;
        UpdateStatus("演示准备完成！使用键盘快捷键控制可视化。");
        
        if (showInstructions)
        {
            ShowInstructions();
        }
    }

    /// <summary>
    /// 创建测试数据文件
    /// </summary>
    void CreateTestDataFiles()
    {
        UpdateStatus("创建测试数据...");

        // 确保StreamingAssets目录存在
        string streamingPath = Application.streamingAssetsPath;
        if (!Directory.Exists(streamingPath))
        {
            Directory.CreateDirectory(streamingPath);
        }

        // 创建轨迹测试数据
        CreateTestTrajectoryData();

        // 创建空间插值测试数据
        CreateTestSpatialData();
    }

    /// <summary>
    /// 创建测试轨迹数据
    /// </summary>
    void CreateTestTrajectoryData()
    {
        var trajectoryPoints = new List<object>();

        // 生成螺旋形轨迹
        int pointCount = 20;
        for (int i = 0; i < pointCount; i++)
        {
            float t = (float)i / (pointCount - 1);
            float angle = t * Mathf.PI * 4; // 2圈螺旋
            float radius = 5f + t * 10f;
            float depth = t * dataRange.z;

            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            float z = -depth;

            // 随机FPI值
            float fpi = Random.Range(fpiRange.x, fpiRange.y);
            float ucs = fpi * 3f;

            trajectoryPoints.Add(new
            {
                x = x,
                y = y,
                z = z,
                depth = depth,
                inclination = 15f + t * 10f,
                azimuth = angle * Mathf.Rad2Deg,
                fpi = fpi,
                ucs = ucs,
                pointIndex = i,
                markTimestamp = System.DateTime.Now.AddHours(i).ToString("yyyy-MM-dd HH:mm:ss"),
                gravitySum = 0.999f
            });
        }

        var trajectoryData = new
        {
            points = trajectoryPoints,
            title = "演示钻孔轨迹",
            totalLength = dataRange.z,
            maxDepth = dataRange.z,
            bounds = new { x = dataRange.x * 2, y = dataRange.y * 2, z = dataRange.z },
            statistics = new
            {
                validFpiPoints = pointCount,
                avgFpi = (fpiRange.x + fpiRange.y) / 2,
                maxFpi = fpiRange.y,
                minFpi = fpiRange.x,
                avgUcs = (fpiRange.x + fpiRange.y) / 2 * 3f,
                maxUcs = fpiRange.y * 3f,
                minUcs = fpiRange.x * 3f
            }
        };

        string jsonContent = JsonUtility.ToJson(trajectoryData, true);
        string filePath = Path.Combine(Application.streamingAssetsPath, "trajectory_data.json");
        File.WriteAllText(filePath, jsonContent);

        Debug.Log($"测试轨迹数据已创建: {trajectoryPoints.Count} 个点");
    }

    /// <summary>
    /// 创建测试空间插值数据
    /// </summary>
    void CreateTestSpatialData()
    {
        var spatialPoints = new List<object>();

        // 生成规则网格的插值点
        float resolution = 1f;
        Vector3 minBounds = new Vector3(-dataRange.x, -dataRange.y, -dataRange.z);
        Vector3 maxBounds = new Vector3(dataRange.x, dataRange.y, 0);

        int pointCount = 0;
        for (float x = minBounds.x; x <= maxBounds.x; x += resolution)
        {
            for (float y = minBounds.y; y <= maxBounds.y; y += resolution)
            {
                for (float z = minBounds.z; z <= maxBounds.z; z += resolution)
                {
                    // 创建有趣的FPI分布模式
                    Vector3 center = new Vector3(dataRange.x * 0.3f, 0, -dataRange.z * 0.5f);
                    float distanceToCenter = Vector3.Distance(new Vector3(x, y, z), center);
                    
                    // 基于距离和深度的FPI值
                    float baseFPI = Mathf.Lerp(fpiRange.y, fpiRange.x, distanceToCenter / dataRange.magnitude);
                    float depthFactor = 1f + Mathf.Abs(z) / dataRange.z;
                    float noiseFactor = 1f + (Mathf.PerlinNoise(x * 0.1f, y * 0.1f) - 0.5f) * 0.3f;
                    
                    float fpi = baseFPI * depthFactor * noiseFactor;
                    fpi = Mathf.Clamp(fpi, fpiRange.x, fpiRange.y);

                    spatialPoints.Add(new
                    {
                        position = new { x = x, y = y, z = z },
                        fpi = fpi
                    });

                    pointCount++;
                    if (pointCount >= testPointCount) break;
                }
                if (pointCount >= testPointCount) break;
            }
            if (pointCount >= testPointCount) break;
        }

        var spatialData = new
        {
            title = "FPI空间插值演示",
            type = "spatial_interpolation",
            processedTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            bounds = new
            {
                min = new { x = minBounds.x, y = minBounds.y, z = minBounds.z },
                max = new { x = maxBounds.x, y = maxBounds.y, z = maxBounds.z }
            },
            resolution = resolution,
            statistics = new
            {
                totalPoints = spatialPoints.Count,
                minFPI = fpiRange.x,
                maxFPI = fpiRange.y,
                avgFPI = (fpiRange.x + fpiRange.y) / 2
            },
            points = spatialPoints
        };

        string jsonContent = JsonUtility.ToJson(spatialData, true);
        string filePath = Path.Combine(Application.streamingAssetsPath, "spatial_fpi_visualization.json");
        File.WriteAllText(filePath, jsonContent);

        Debug.Log($"测试空间插值数据已创建: {spatialPoints.Count} 个点");
    }

    /// <summary>
    /// 设置演示场景
    /// </summary>
    void SetupDemoScene()
    {
        UpdateStatus("设置演示场景...");

        // 设置相机
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObj = new GameObject("Demo Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }

        mainCamera.transform.position = new Vector3(30f, 25f, 30f);
        mainCamera.transform.LookAt(Vector3.zero);
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.2f);

        // 添加相机控制
        if (mainCamera.GetComponent<CameraController>() == null)
        {
            mainCamera.gameObject.AddComponent<CameraController>();
        }

        // 设置光照
        SetupLighting();
    }

    /// <summary>
    /// 设置光照
    /// </summary>
    void SetupLighting()
    {
        // 环境光
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f);

        // 主光源
        GameObject lightObj = new GameObject("Demo Light");
        Light light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.shadows = LightShadows.Soft;
        lightObj.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
    }

    /// <summary>
    /// 创建可视化器
    /// </summary>
    void CreateVisualizers()
    {
        UpdateStatus("创建可视化器...");

        GameObject visualizerContainer = new GameObject("Demo Visualizers");

        switch (demoMode)
        {
            case DemoMode.SpatialOnly:
                CreateSpatialVisualizer(visualizerContainer);
                break;

            case DemoMode.TrajectoryOnly:
                CreateTrajectoryVisualizer(visualizerContainer);
                break;

            case DemoMode.Integrated:
                CreateIntegratedVisualizer(visualizerContainer);
                break;
        }
    }

    /// <summary>
    /// 创建空间可视化器
    /// </summary>
    void CreateSpatialVisualizer(GameObject parent)
    {
        GameObject spatialObj = new GameObject("Spatial FPI Visualizer");
        spatialObj.transform.SetParent(parent.transform);
        spatialVisualizer = spatialObj.AddComponent<SpatialFPIVisualizer>();
        
        spatialVisualizer.spatialDataFileName = "spatial_fpi_visualization.json";
        spatialVisualizer.autoLoadOnStart = false;
        spatialVisualizer.showSpatialPoints = true;
        spatialVisualizer.showBoundingBox = true;
        spatialVisualizer.showColorLegend = true;
        spatialVisualizer.pointSize = 0.2f;
        spatialVisualizer.transparency = 0.8f;
    }

    /// <summary>
    /// 创建轨迹可视化器
    /// </summary>
    void CreateTrajectoryVisualizer(GameObject parent)
    {
        GameObject trajectoryObj = new GameObject("Trajectory Visualizer");
        trajectoryObj.transform.SetParent(parent.transform);
        trajectoryVisualizer = trajectoryObj.AddComponent<TrajectoryVisualizer>();
        
        trajectoryVisualizer.jsonFileName = "trajectory_data.json";
        trajectoryVisualizer.loadFromStreamingAssets = true;
        trajectoryVisualizer.showTrajectoryLine = true;
        trajectoryVisualizer.showTrajectoryPoints = true;
        trajectoryVisualizer.showDepthLabels = true;
        trajectoryVisualizer.showCoordinateAxis = true;
    }

    /// <summary>
    /// 创建集成可视化器
    /// </summary>
    void CreateIntegratedVisualizer(GameObject parent)
    {
        // 创建轨迹可视化器
        CreateTrajectoryVisualizer(parent);
        
        // 创建空间可视化器
        CreateSpatialVisualizer(parent);

        // 创建集成可视化器
        GameObject integratedObj = new GameObject("Integrated Visualizer");
        integratedObj.transform.SetParent(parent.transform);
        integratedVisualizer = integratedObj.AddComponent<IntegratedDrillingVisualizer>();
        
        integratedVisualizer.trajectoryVisualizer = trajectoryVisualizer;
        integratedVisualizer.spatialFPIVisualizer = spatialVisualizer;
        integratedVisualizer.mainCamera = Camera.main;
        integratedVisualizer.autoLoadDataOnStart = false;
        integratedVisualizer.syncTrajectoryAndSpatial = true;
    }

    /// <summary>
    /// 加载和显示数据
    /// </summary>
    void LoadAndDisplayData()
    {
        UpdateStatus("加载数据...");

        switch (demoMode)
        {
            case DemoMode.SpatialOnly:
                if (spatialVisualizer != null)
                {
                    spatialVisualizer.LoadSpatialData();
                }
                break;

            case DemoMode.TrajectoryOnly:
                if (trajectoryVisualizer != null)
                {
                    trajectoryVisualizer.LoadTrajectoryData();
                }
                break;

            case DemoMode.Integrated:
                if (integratedVisualizer != null)
                {
                    StartCoroutine(integratedVisualizer.LoadDataCoroutine());
                }
                break;
        }
    }

    /// <summary>
    /// 设置相机视图
    /// </summary>
    void SetupCameraView()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera != null && integratedVisualizer != null)
        {
            integratedVisualizer.FitViewToData();
        }
    }

    /// <summary>
    /// 创建演示UI
    /// </summary>
    void CreateDemoUI()
    {
        UpdateStatus("创建演示UI...");

        // 创建Canvas
        GameObject canvasObj = new GameObject("Demo UI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;

        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建事件系统
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        demoUI = canvasObj;

        // 创建状态文本
        CreateStatusUI(canvas);

        // 创建控制按钮
        CreateControlButtons(canvas);
    }

    /// <summary>
    /// 创建状态UI
    /// </summary>
    void CreateStatusUI(Canvas canvas)
    {
        // 状态面板
        GameObject statusPanel = new GameObject("Status Panel");
        statusPanel.transform.SetParent(canvas.transform);

        RectTransform statusRect = statusPanel.AddComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.02f, 0.85f);
        statusRect.anchorMax = new Vector2(0.6f, 0.98f);
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;

        Image statusBg = statusPanel.AddComponent<Image>();
        statusBg.color = new Color(0, 0, 0, 0.7f);

        // 标题
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(statusPanel.transform);
        Text titleText = titleObj.AddComponent<Text>();
        titleText.text = $"空间FPI插值可视化演示 - {demoMode}模式";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 18;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleLeft;

        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = new Vector2(1, 0.5f);
        titleRect.offsetMin = new Vector2(10, 0);
        titleRect.offsetMax = new Vector2(-10, 0);

        // 状态文本
        GameObject statusTextObj = new GameObject("Status Text");
        statusTextObj.transform.SetParent(statusPanel.transform);
        statusText = statusTextObj.AddComponent<Text>();
        statusText.text = "初始化中...";
        statusText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statusText.fontSize = 14;
        statusText.color = Color.yellow;
        statusText.alignment = TextAnchor.MiddleLeft;

        RectTransform statusTextRect = statusTextObj.GetComponent<RectTransform>();
        statusTextRect.anchorMin = new Vector2(0, 0);
        statusTextRect.anchorMax = new Vector2(1, 0.5f);
        statusTextRect.offsetMin = new Vector2(10, 0);
        statusTextRect.offsetMax = new Vector2(-10, 0);
    }

    /// <summary>
    /// 创建控制按钮
    /// </summary>
    void CreateControlButtons(Canvas canvas)
    {
        GameObject buttonPanel = new GameObject("Control Panel");
        buttonPanel.transform.SetParent(canvas.transform);

        RectTransform buttonRect = buttonPanel.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.02f, 0.02f);
        buttonRect.anchorMax = new Vector2(0.4f, 0.2f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        Image buttonBg = buttonPanel.AddComponent<Image>();
        buttonBg.color = new Color(0, 0, 0, 0.5f);

        // 按钮
        CreateButton(buttonPanel, "重置视图 (R)", new Vector2(10, -30), () => ResetView());
        CreateButton(buttonPanel, "切换轨迹 (T)", new Vector2(120, -30), () => ToggleTrajectory());
        CreateButton(buttonPanel, "切换空间 (S)", new Vector2(230, -30), () => ToggleSpatial());
        CreateButton(buttonPanel, "自动旋转 (Space)", new Vector2(10, -60), () => ToggleAutoRotate());
        CreateButton(buttonPanel, "截图 (P)", new Vector2(120, -60), () => TakeScreenshot());
        CreateButton(buttonPanel, "重新加载 (L)", new Vector2(230, -60), () => ReloadData());
    }

    /// <summary>
    /// 创建按钮
    /// </summary>
    Button CreateButton(GameObject parent, string text, Vector2 position, System.Action onClick)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent.transform);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(100, 25);

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => onClick?.Invoke());

        Image bgImage = buttonObj.AddComponent<Image>();
        bgImage.color = new Color(0.3f, 0.3f, 0.5f, 1f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform);
        Text buttonText = textObj.AddComponent<Text>();
        buttonText.text = text;
        buttonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        buttonText.fontSize = 10;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return button;
    }

    /// <summary>
    /// 处理演示输入
    /// </summary>
    void HandleDemoInput()
    {
        if (!isDemoRunning) return;

        if (Input.GetKeyDown(KeyCode.R)) ResetView();
        if (Input.GetKeyDown(KeyCode.T)) ToggleTrajectory();
        if (Input.GetKeyDown(KeyCode.S)) ToggleSpatial();
        if (Input.GetKeyDown(KeyCode.L)) ToggleLegend();
        if (Input.GetKeyDown(KeyCode.Space)) ToggleAutoRotate();
        if (Input.GetKeyDown(KeyCode.P)) TakeScreenshot();
        if (Input.GetKeyDown(KeyCode.F5)) ReloadData();
        if (Input.GetKeyDown(KeyCode.H)) ToggleInstructions();
        if (Input.GetKeyDown(KeyCode.Escape)) ExitDemo();
    }

    #region 控制方法

    void ResetView()
    {
        Camera.main?.GetComponent<CameraController>()?.SendMessage("ResetView", SendMessageOptions.DontRequireReceiver);
        UpdateStatus("视图已重置");
    }

    void ToggleTrajectory()
    {
        if (integratedVisualizer != null)
        {
            integratedVisualizer.ToggleTrajectoryVisibility();
        }
        else if (trajectoryVisualizer != null)
        {
            trajectoryVisualizer.gameObject.SetActive(!trajectoryVisualizer.gameObject.activeSelf);
        }
        UpdateStatus("轨迹显示已切换");
    }

    void ToggleSpatial()
    {
        if (integratedVisualizer != null)
        {
            integratedVisualizer.ToggleSpatialVisibility();
        }
        else if (spatialVisualizer != null)
        {
            spatialVisualizer.gameObject.SetActive(!spatialVisualizer.gameObject.activeSelf);
        }
        UpdateStatus("空间插值显示已切换");
    }

    void ToggleLegend()
    {
        if (integratedVisualizer != null)
        {
            integratedVisualizer.ToggleLegendVisibility();
        }
        else if (spatialVisualizer != null)
        {
            spatialVisualizer.showColorLegend = !spatialVisualizer.showColorLegend;
        }
        UpdateStatus("图例显示已切换");
    }

    void ToggleAutoRotate()
    {
        if (integratedVisualizer != null)
        {
            integratedVisualizer.ToggleAutoRotate();
        }
        UpdateStatus("自动旋转已切换");
    }

    void TakeScreenshot()
    {
        string fileName = $"SpatialDemo_Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        ScreenCapture.CaptureScreenshot(fileName);
        UpdateStatus($"截图已保存: {fileName}");
    }

    void ReloadData()
    {
        if (integratedVisualizer != null)
        {
            integratedVisualizer.ReloadData();
        }
        else
        {
            spatialVisualizer?.ReloadData();
            trajectoryVisualizer?.LoadTrajectoryData();
        }
        UpdateStatus("数据已重新加载");
    }

    void ToggleInstructions()
    {
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(!instructionText.gameObject.activeSelf);
        }
        else
        {
            ShowInstructions();
        }
    }

    void ExitDemo()
    {
        UpdateStatus("退出演示...");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    #endregion

    /// <summary>
    /// 显示操作说明
    /// </summary>
    void ShowInstructions()
    {
        if (instructionText != null) return;

        GameObject instructionPanel = new GameObject("Instruction Panel");
        instructionPanel.transform.SetParent(demoUI.transform);

        RectTransform instructionRect = instructionPanel.AddComponent<RectTransform>();
        instructionRect.anchorMin = new Vector2(0.65f, 0.1f);
        instructionRect.anchorMax = new Vector2(0.98f, 0.8f);
        instructionRect.offsetMin = Vector2.zero;
        instructionRect.offsetMax = Vector2.zero;

        Image instructionBg = instructionPanel.AddComponent<Image>();
        instructionBg.color = new Color(0, 0, 0, 0.8f);

        GameObject instructionTextObj = new GameObject("Instruction Text");
        instructionTextObj.transform.SetParent(instructionPanel.transform);
        instructionText = instructionTextObj.AddComponent<Text>();

        string instructions = @"<b>空间FPI插值可视化演示</b>

<b>操作指南:</b>
• 鼠标左键拖拽: 旋转视图
• 鼠标滚轮: 缩放视图  
• 鼠标中键拖拽: 平移视图

<b>键盘快捷键:</b>
• R - 重置视图
• T - 切换轨迹显示
• S - 切换空间插值显示
• L - 切换图例显示
• 空格 - 自动旋转开关
• P - 截图
• F5 - 重新加载数据
• H - 显示/隐藏帮助
• Esc - 退出演示

<b>可视化说明:</b>
• <color=red>红色</color> - 高FPI值区域
• <color=yellow>黄色</color> - 中等FPI值区域  
• <color=blue>蓝色</color> - 低FPI值区域
• 白线 - 钻井轨迹
• 白框 - 空间边界

<b>演示数据:</b>
当前显示的是程序生成的测试数据，
实际使用时请加载真实的
钻井数据文件。

按H键隐藏此帮助信息。";

        instructionText.text = instructions;
        instructionText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        instructionText.fontSize = 12;
        instructionText.color = Color.white;
        instructionText.alignment = TextAnchor.UpperLeft;

        RectTransform instructionTextRect = instructionTextObj.GetComponent<RectTransform>();
        instructionTextRect.anchorMin = Vector2.zero;
        instructionTextRect.anchorMax = Vector2.one;
        instructionTextRect.offsetMin = new Vector2(10, 10);
        instructionTextRect.offsetMax = new Vector2(-10, -10);
    }

    /// <summary>
    /// 更新状态文本
    /// </summary>
    void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        }
        Debug.Log($"SpatialDemo: {message}");
    }

    /// <summary>
    /// 手动开始演示
    /// </summary>
    [ContextMenu("开始演示")]
    public void StartDemo()
    {
        if (!isDemoRunning)
        {
            StartCoroutine(StartDemoCoroutine());
        }
    }

    /// <summary>
    /// 切换演示模式
    /// </summary>
    [ContextMenu("切换到空间插值模式")]
    public void SwitchToSpatialMode()
    {
        demoMode = DemoMode.SpatialOnly;
        RestartDemo();
    }

    [ContextMenu("切换到轨迹模式")]
    public void SwitchToTrajectoryMode()
    {
        demoMode = DemoMode.TrajectoryOnly;
        RestartDemo();
    }

    [ContextMenu("切换到集成模式")]
    public void SwitchToIntegratedMode()
    {
        demoMode = DemoMode.Integrated;
        RestartDemo();
    }

    /// <summary>
    /// 重启演示
    /// </summary>
    void RestartDemo()
    {
        // 清理现有对象
        if (demoUI != null) DestroyImmediate(demoUI);
        
        GameObject[] visualizers = GameObject.FindGameObjectsWithTag("Untagged");
        foreach (var obj in visualizers)
        {
            if (obj.name.Contains("Visualizer") || obj.name.Contains("Demo"))
            {
                DestroyImmediate(obj);
            }
        }

        isDemoRunning = false;
        StartCoroutine(StartDemoCoroutine());
    }

    void OnDestroy()
    {
        // 清理资源
        if (demoUI != null)
        {
            DestroyImmediate(demoUI);
        }
    }
} 