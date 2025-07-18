using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 空间插值可视化场景自动设置
/// </summary>
public class SpatialVisualizationSceneSetup : MonoBehaviour
{
    [Header("场景设置")]
    public bool setupOnStart = true;
    public bool createLighting = true;
    public bool createUI = true;
    public bool createExampleData = false;

    [Header("材质和预制体")]
    public Material trajectoryLineMaterial;
    public Material trajectoryPointMaterial;
    public Material spatialPointMaterial;
    public Material boundingBoxMaterial;

    [Header("UI设置")]
    public Font uiFont;
    public int uiFontSize = 14;
    public Color uiBackgroundColor = new Color(0, 0, 0, 0.7f);
    public Color uiTextColor = Color.white;

    void Start()
    {
        if (setupOnStart)
        {
            StartCoroutine(SetupSceneCoroutine());
        }
    }

    /// <summary>
    /// 设置场景协程
    /// </summary>
    IEnumerator SetupSceneCoroutine()
    {
        Debug.Log("开始设置空间插值可视化场景...");

        // 1. 设置相机
        SetupCamera();
        yield return null;

        // 2. 设置光照
        if (createLighting)
        {
            SetupLighting();
            yield return null;
        }

        // 3. 创建可视化组件
        CreateVisualizationComponents();
        yield return null;

        // 4. 创建UI
        if (createUI)
        {
            CreateUISystem();
            yield return null;
        }

        // 5. 创建示例数据
        if (createExampleData)
        {
            CreateExampleData();
            yield return null;
        }

        // 6. 设置后处理
        SetupPostProcessing();
        yield return null;

        Debug.Log("空间插值可视化场景设置完成！");
        
        // 显示设置完成信息
        ShowSetupCompleteInfo();
    }

    /// <summary>
    /// 设置相机
    /// </summary>
    void SetupCamera()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            // 创建主相机
            GameObject cameraObj = new GameObject("Main Camera");
            mainCamera = cameraObj.AddComponent<Camera>();
            cameraObj.tag = "MainCamera";
        }

        // 配置相机
        mainCamera.clearFlags = CameraClearFlags.Skybox;
        mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.2f, 1f);
        mainCamera.fieldOfView = 60f;
        mainCamera.nearClipPlane = 0.1f;
        mainCamera.farClipPlane = 1000f;

        // 设置初始位置
        mainCamera.transform.position = new Vector3(25f, 20f, 25f);
        mainCamera.transform.LookAt(Vector3.zero);

        // 添加相机控制脚本
        if (mainCamera.GetComponent<CameraController>() == null)
        {
            mainCamera.gameObject.AddComponent<CameraController>();
        }

        Debug.Log("相机设置完成");
    }

    /// <summary>
    /// 设置光照
    /// </summary>
    void SetupLighting()
    {
        // 设置环境光
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.5f, 0.7f, 1f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

        // 创建主光源
        GameObject mainLightObj = GameObject.Find("Directional Light");
        if (mainLightObj == null)
        {
            mainLightObj = new GameObject("Directional Light");
            Light mainLight = mainLightObj.AddComponent<Light>();
            mainLight.type = LightType.Directional;
            mainLight.color = Color.white;
            mainLight.intensity = 1.2f;
            mainLight.shadows = LightShadows.Soft;
            mainLightObj.transform.rotation = Quaternion.Euler(45f, 45f, 0f);
        }

        // 创建填充光
        GameObject fillLightObj = new GameObject("Fill Light");
        Light fillLight = fillLightObj.AddComponent<Light>();
        fillLight.type = LightType.Directional;
        fillLight.color = new Color(0.8f, 0.9f, 1f, 1f);
        fillLight.intensity = 0.3f;
        fillLight.shadows = LightShadows.None;
        fillLightObj.transform.rotation = Quaternion.Euler(-45f, -45f, 0f);

        Debug.Log("光照设置完成");
    }

    /// <summary>
    /// 创建可视化组件
    /// </summary>
    void CreateVisualizationComponents()
    {
        // 创建主可视化器容器
        GameObject visualizerContainer = new GameObject("Drilling Data Visualizers");

        // 创建集成可视化器
        IntegratedDrillingVisualizer integratedVisualizer = visualizerContainer.AddComponent<IntegratedDrillingVisualizer>();

        // 创建轨迹可视化器
        GameObject trajectoryObj = new GameObject("Trajectory Visualizer");
        trajectoryObj.transform.SetParent(visualizerContainer.transform);
        TrajectoryVisualizer trajectoryVisualizer = trajectoryObj.AddComponent<TrajectoryVisualizer>();

        // 配置轨迹可视化器
        if (trajectoryLineMaterial != null)
            trajectoryVisualizer.trajectoryLineMaterial = trajectoryLineMaterial;
        if (trajectoryPointMaterial != null)
            trajectoryVisualizer.trajectoryPointMaterial = trajectoryPointMaterial;

        // 创建空间FPI可视化器
        GameObject spatialObj = new GameObject("Spatial FPI Visualizer");
        spatialObj.transform.SetParent(visualizerContainer.transform);
        SpatialFPIVisualizer spatialVisualizer = spatialObj.AddComponent<SpatialFPIVisualizer>();

        // 配置空间FPI可视化器
        if (spatialPointMaterial != null)
            spatialVisualizer.pointMaterial = spatialPointMaterial;
        if (boundingBoxMaterial != null)
            spatialVisualizer.boundingBoxMaterial = boundingBoxMaterial;

        // 设置集成可视化器的引用
        integratedVisualizer.trajectoryVisualizer = trajectoryVisualizer;
        integratedVisualizer.spatialFPIVisualizer = spatialVisualizer;
        integratedVisualizer.mainCamera = Camera.main;

        Debug.Log("可视化组件创建完成");
    }

    /// <summary>
    /// 创建UI系统
    /// </summary>
    void CreateUISystem()
    {
        // 创建Canvas
        GameObject canvasObj = new GameObject("UI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        canvasObj.AddComponent<GraphicRaycaster>();

        // 创建事件系统
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        // 创建主控制面板
        CreateMainControlPanel(canvas);

        // 创建信息面板
        CreateInfoPanel(canvas);

        // 创建工具栏
        CreateToolbar(canvas);

        Debug.Log("UI系统创建完成");
    }

    /// <summary>
    /// 创建主控制面板
    /// </summary>
    void CreateMainControlPanel(Canvas canvas)
    {
        GameObject panelObj = new GameObject("Main Control Panel");
        panelObj.transform.SetParent(canvas.transform);

        // 设置面板位置和大小
        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.02f, 0.7f);
        panelRect.anchorMax = new Vector2(0.32f, 0.98f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // 添加背景
        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = uiBackgroundColor;

        // 创建标题
        CreateUIText(panelObj, "钻井数据可视化控制", new Vector2(0, -20), new Vector2(0, 30), uiFontSize + 4, TextAnchor.MiddleCenter);

        // 创建控制组件
        SpatialFPIControlPanel controlPanel = panelObj.AddComponent<SpatialFPIControlPanel>();

        // 创建控制按钮
        CreateControlButtons(panelObj);
    }

    /// <summary>
    /// 创建控制按钮
    /// </summary>
    void CreateControlButtons(GameObject parent)
    {
        float yPos = -70f;
        float buttonHeight = 30f;
        float spacing = 35f;

        // 轨迹显示切换
        CreateUIToggle(parent, "显示轨迹", new Vector2(10, yPos), true);
        yPos -= spacing;

        // 空间插值显示切换
        CreateUIToggle(parent, "显示空间插值", new Vector2(10, yPos), true);
        yPos -= spacing;

        // 显示边界框
        CreateUIToggle(parent, "显示边界框", new Vector2(10, yPos), true);
        yPos -= spacing;

        // 显示图例
        CreateUIToggle(parent, "显示图例", new Vector2(10, yPos), true);
        yPos -= spacing;

        // 重置视图按钮
        CreateUIButton(parent, "重置视图", new Vector2(10, yPos), new Vector2(100, buttonHeight));
        CreateUIButton(parent, "适应视图", new Vector2(120, yPos), new Vector2(100, buttonHeight));
        yPos -= spacing;

        // 自动旋转按钮
        CreateUIButton(parent, "自动旋转", new Vector2(10, yPos), new Vector2(100, buttonHeight));
        CreateUIButton(parent, "截图", new Vector2(120, yPos), new Vector2(100, buttonHeight));
    }

    /// <summary>
    /// 创建信息面板
    /// </summary>
    void CreateInfoPanel(Canvas canvas)
    {
        GameObject panelObj = new GameObject("Info Panel");
        panelObj.transform.SetParent(canvas.transform);

        RectTransform panelRect = panelObj.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.68f, 0.02f);
        panelRect.anchorMax = new Vector2(0.98f, 0.5f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image bgImage = panelObj.AddComponent<Image>();
        bgImage.color = new Color(uiBackgroundColor.r, uiBackgroundColor.g, uiBackgroundColor.b, 0.5f);

        // 创建信息内容
        CreateUIText(panelObj, "数据信息", new Vector2(0, -20), new Vector2(0, 30), uiFontSize + 2, TextAnchor.MiddleCenter);
        
        CreateUIText(panelObj, "轨迹点数: 等待加载\n空间插值点数: 等待加载\nFPI范围: 等待计算\n处理时间: --", 
                    new Vector2(10, -60), new Vector2(-20, -100), uiFontSize - 2, TextAnchor.UpperLeft);
    }

    /// <summary>
    /// 创建工具栏
    /// </summary>
    void CreateToolbar(Canvas canvas)
    {
        GameObject toolbarObj = new GameObject("Toolbar");
        toolbarObj.transform.SetParent(canvas.transform);

        RectTransform toolbarRect = toolbarObj.AddComponent<RectTransform>();
        toolbarRect.anchorMin = new Vector2(0.35f, 0.02f);
        toolbarRect.anchorMax = new Vector2(0.65f, 0.08f);
        toolbarRect.offsetMin = Vector2.zero;
        toolbarRect.offsetMax = Vector2.zero;

        Image bgImage = toolbarObj.AddComponent<Image>();
        bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        // 创建工具栏按钮
        float buttonWidth = 80f;
        float spacing = 90f;
        float startX = -180f;

        CreateUIButton(toolbarObj, "加载数据", new Vector2(startX, 0), new Vector2(buttonWidth, 30));
        CreateUIButton(toolbarObj, "导出图像", new Vector2(startX + spacing, 0), new Vector2(buttonWidth, 30));
        CreateUIButton(toolbarObj, "设置", new Vector2(startX + spacing * 2, 0), new Vector2(buttonWidth, 30));
        CreateUIButton(toolbarObj, "帮助", new Vector2(startX + spacing * 3, 0), new Vector2(buttonWidth, 30));
    }

    /// <summary>
    /// 创建UI文本
    /// </summary>
    Text CreateUIText(GameObject parent, string content, Vector2 position, Vector2 sizeDelta, int fontSize, TextAnchor alignment)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(parent.transform);

        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        if (sizeDelta.x == 0 && sizeDelta.y == 0)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
        else
        {
            rect.sizeDelta = sizeDelta;
        }

        Text text = textObj.AddComponent<Text>();
        text.text = content;
        text.font = uiFont != null ? uiFont : Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = fontSize;
        text.color = uiTextColor;
        text.alignment = alignment;

        return text;
    }

    /// <summary>
    /// 创建UI按钮
    /// </summary>
    Button CreateUIButton(GameObject parent, string text, Vector2 position, Vector2 sizeDelta)
    {
        GameObject buttonObj = new GameObject($"Button_{text}");
        buttonObj.transform.SetParent(parent.transform);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = sizeDelta;

        Button button = buttonObj.AddComponent<Button>();
        
        Image bgImage = buttonObj.AddComponent<Image>();
        bgImage.color = new Color(0.4f, 0.4f, 0.4f, 1f);

        // 创建按钮文本
        CreateUIText(buttonObj, text, Vector2.zero, Vector2.zero, uiFontSize - 2, TextAnchor.MiddleCenter);

        return button;
    }

    /// <summary>
    /// 创建UI切换按钮
    /// </summary>
    Toggle CreateUIToggle(GameObject parent, string labelText, Vector2 position, bool initialValue)
    {
        GameObject toggleObj = new GameObject($"Toggle_{labelText}");
        toggleObj.transform.SetParent(parent.transform);

        RectTransform rect = toggleObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(200, 25);

        Toggle toggle = toggleObj.AddComponent<Toggle>();
        toggle.isOn = initialValue;

        // 创建背景
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform);
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.white;
        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.sizeDelta = new Vector2(20, 20);
        bgRect.anchoredPosition = new Vector2(0, 0);

        // 创建复选标记
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(bgObj.transform);
        Image checkImage = checkObj.AddComponent<Image>();
        checkImage.color = new Color(0.2f, 0.8f, 0.2f, 1f);
        RectTransform checkRect = checkObj.GetComponent<RectTransform>();
        checkRect.sizeDelta = new Vector2(16, 16);
        checkRect.anchoredPosition = Vector2.zero;

        // 创建标签文本
        CreateUIText(toggleObj, labelText, new Vector2(30, 0), new Vector2(160, 25), uiFontSize - 2, TextAnchor.MiddleLeft);

        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;

        return toggle;
    }

    /// <summary>
    /// 创建示例数据
    /// </summary>
    void CreateExampleData()
    {
        Debug.Log("创建示例数据...");

        // 这里可以创建一些示例的轨迹和FPI数据文件
        CreateExampleTrajectoryData();
        CreateExampleSpatialData();

        Debug.Log("示例数据创建完成");
    }

    /// <summary>
    /// 创建示例轨迹数据
    /// </summary>
    void CreateExampleTrajectoryData()
    {
        var exampleData = new
        {
            points = new[]
            {
                new { x = 0f, y = 0f, z = 0f, depth = 0f, inclination = 0f, azimuth = 0f, fpi = 1000000f, ucs = 3000000f },
                new { x = 5f, y = -2f, z = 1f, depth = 5f, inclination = 15f, azimuth = 45f, fpi = 1500000f, ucs = 4500000f },
                new { x = 10f, y = -5f, z = 3f, depth = 10f, inclination = 25f, azimuth = 60f, fpi = 2000000f, ucs = 6000000f },
                new { x = 15f, y = -8f, z = 5f, depth = 15f, inclination = 30f, azimuth = 75f, fpi = 2500000f, ucs = 7500000f }
            },
            title = "示例钻孔轨迹",
            totalLength = 15f,
            maxDepth = 8f
        };

        string jsonContent = JsonUtility.ToJson(exampleData, true);
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "trajectory_data.json");
        
        if (!System.IO.Directory.Exists(Application.streamingAssetsPath))
        {
            System.IO.Directory.CreateDirectory(Application.streamingAssetsPath);
        }
        
        System.IO.File.WriteAllText(filePath, jsonContent);
    }

    /// <summary>
    /// 创建示例空间数据
    /// </summary>
    void CreateExampleSpatialData()
    {
        var pointsList = new System.Collections.Generic.List<object>();
        
        // 生成网格状的插值点
        for (float x = -10; x <= 20; x += 2)
        {
            for (float y = -10; y <= 2; y += 2)
            {
                for (float z = -2; z <= 8; z += 2)
                {
                    float distance = Mathf.Sqrt(x * x + y * y + z * z);
                    float fpi = 1000000f + distance * 50000f + UnityEngine.Random.Range(-100000f, 100000f);
                    
                    pointsList.Add(new
                    {
                        position = new { x = x, y = y, z = z },
                        fpi = fpi
                    });
                }
            }
        }

        var exampleSpatialData = new
        {
            title = "FPI空间插值可视化",
            type = "spatial_interpolation",
            processedTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            bounds = new
            {
                min = new { x = -10f, y = -10f, z = -2f },
                max = new { x = 20f, y = 2f, z = 8f }
            },
            resolution = 2f,
            statistics = new
            {
                totalPoints = pointsList.Count,
                minFPI = 1000000f,
                maxFPI = 3000000f,
                avgFPI = 2000000f
            },
            points = pointsList.ToArray()
        };

        string jsonContent = JsonUtility.ToJson(exampleSpatialData, true);
        string filePath = System.IO.Path.Combine(Application.streamingAssetsPath, "spatial_fpi_visualization.json");
        System.IO.File.WriteAllText(filePath, jsonContent);
    }

    /// <summary>
    /// 设置后处理
    /// </summary>
    void SetupPostProcessing()
    {
        // 这里可以添加后处理效果设置
        // 例如抗锯齿、颜色校正等
        
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            // 启用HDR
            mainCamera.allowHDR = true;
            
            // 设置抗锯齿
            mainCamera.allowMSAA = true;
        }

        Debug.Log("后处理设置完成");
    }

    /// <summary>
    /// 显示设置完成信息
    /// </summary>
    void ShowSetupCompleteInfo()
    {
        string info = @"空间插值可视化场景设置完成！

功能说明：
- 轨迹可视化：显示钻井轨迹线和关键点
- 空间插值：展示FPI值的三维分布
- 颜色图例：显示FPI值与颜色的对应关系
- 交互控制：鼠标旋转、缩放、平移视图

操作指南：
- 鼠标左键拖拽：旋转视图
- 鼠标滚轮：缩放视图
- 鼠标中键拖拽：平移视图
- R键：重置视图
- T键：切换轨迹显示
- S键：切换空间插值显示
- L键：切换图例显示
- 空格键：开关自动旋转

请加载实际数据文件或使用示例数据开始可视化。";

        Debug.Log(info);
        
        // 也可以在UI中显示这个信息
    }

    /// <summary>
    /// 手动设置场景
    /// </summary>
    [ContextMenu("设置场景")]
    public void SetupScene()
    {
        StartCoroutine(SetupSceneCoroutine());
    }

    /// <summary>
    /// 重置场景
    /// </summary>
    [ContextMenu("重置场景")]
    public void ResetScene()
    {
        // 清理现有对象
        GameObject[] existingObjects = {
            GameObject.Find("Drilling Data Visualizers"),
            GameObject.Find("UI Canvas"),
            GameObject.Find("EventSystem"),
            GameObject.Find("Fill Light")
        };

        foreach (var obj in existingObjects)
        {
            if (obj != null)
                DestroyImmediate(obj);
        }

        Debug.Log("场景已重置，可以重新设置");
    }
}

/// <summary>
/// 相机控制器
/// </summary>
public class CameraController : MonoBehaviour
{
    public float rotationSpeed = 100f;
    public float zoomSpeed = 10f;
    public float panSpeed = 5f;

    private Vector3 lastMousePosition;
    private bool isDragging = false;

    void Update()
    {
        HandleMouseInput();
        HandleKeyboardInput();
    }

    void HandleMouseInput()
    {
        // 旋转控制
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
            transform.RotateAround(Vector3.zero, Vector3.up, mouseDelta.x * rotationSpeed * Time.deltaTime);
            transform.RotateAround(Vector3.zero, transform.right, -mouseDelta.y * rotationSpeed * Time.deltaTime);
            lastMousePosition = Input.mousePosition;
        }

        // 缩放控制
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            transform.position += transform.forward * scroll * zoomSpeed;
        }

        // 平移控制
        if (Input.GetMouseButton(2))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
            Vector3 moveDirection = -transform.right * mouseDelta.x * panSpeed * Time.deltaTime;
            moveDirection += -transform.up * mouseDelta.y * panSpeed * Time.deltaTime;
            transform.position += moveDirection;
            lastMousePosition = Input.mousePosition;
        }
    }

    void HandleKeyboardInput()
    {
        // 这里可以添加键盘快捷键
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 重置相机位置
            transform.position = new Vector3(25f, 20f, 25f);
            transform.LookAt(Vector3.zero);
        }
    }
} 