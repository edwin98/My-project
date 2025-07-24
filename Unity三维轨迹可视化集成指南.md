echo $env:HTTP_PROXY# Unity三维轨迹可视化集成指南

## 📖 概述

本指南详细说明如何在Unity中展示钻井轨迹的三维可视化，包括数据处理、可视化组件设置、交互控制和效果展示。通过本指南，你将学会如何创建专业的钻井轨迹三维可视化系统。

## 🎯 功能特性

### 核心可视化功能
- ✨ **三维轨迹线**: 平滑的3D轨迹路径渲染
- 🎨 **轨迹点显示**: 可交互的轨迹节点
- 📏 **坐标轴系统**: XYZ坐标参考轴
- 🏷️ **深度标签**: 自动生成的深度标记
- 🌈 **多种颜色方案**: 深度、倾角、方位角着色

### 交互控制功能
- 🖱️ **鼠标控制**: 拖拽旋转、滚轮缩放、中键平移
- ⌨️ **键盘快捷键**: 快速切换显示选项
- 🎮 **UI控制面板**: 完整的参数调节界面
- 🔄 **自动旋转**: 可开启的自动视角旋转
- 👆 **点击交互**: 点击查看轨迹点详细信息

### 数据集成功能
- 📊 **Excel数据导入**: 直接读取Excel钻井数据
- 🔄 **实时数据更新**: 文件变化自动更新显示
- 📈 **数据处理集成**: 无缝集成数据处理流程
- 💾 **多格式支持**: 支持CSV、JSON等格式

## 🏗️ 在Unity中展示轨迹的完整流程

### 第一步：项目准备

1. **创建新的Unity项目**
   ```
   Unity版本: 2022.3 LTS或更高
   渲染管线: Built-in Render Pipeline
   模板: 3D Template
   ```

2. **准备数据文件**
   ```
   📁 Assets/Data/
   └── 📄 drilling_data.xlsx  # 钻井数据Excel文件
   ```

3. **创建场景结构**
   ```
   📁 Hierarchy
   ├── 🎥 Main Camera
   ├── 💡 Directional Light
   ├── 🎨 Canvas (UI)
   ├── 🔧 DrillingDataProcessor
   ├── 👁️ TrajectoryVisualizer
   └── 🎛️ TrajectoryController
   ```

### 第二步：自动场景设置（推荐方式）

1. **使用自动设置器**
   ```csharp
   // 在空场景中创建GameObject
   GameObject autoSetup = new GameObject("AutoSetup");
   
   // 添加自动设置组件
   Trajectory3DSceneSetup setup = autoSetup.AddComponent<Trajectory3DSceneSetup>();
   
   // 配置设置选项
   setup.createTestData = true;      // 创建测试数据
   setup.setupLighting = true;       // 设置光照
   setup.createUI = true;            // 创建UI界面
   setup.setupCamera = true;         // 设置相机
   ```

2. **运行场景**
   - 点击Play按钮
   - 系统会自动创建所有必要组件
   - 几秒钟后即可看到轨迹可视化效果

### 第三步：手动配置（进阶方式）

#### 3.1 配置数据处理器

```csharp
// 创建数据处理器GameObject
GameObject processorObj = new GameObject("DrillingDataProcessor");
UnityDrillingDataProcessor processor = processorObj.AddComponent<UnityDrillingDataProcessor>();

// 配置文件路径
processor.inputExcelPath = "Assets/Data/drilling_data.xlsx";
processor.outputPath = "Assets/Output/";
processor.jsonOutputPath = "Assets/Json/";

// 配置处理参数
processor.depthStep = 1.5f;           // 深度步长
processor.depthInterval = 0.2f;       // 深度间隔
processor.magneticDeclination = 0f;   // 磁偏角
processor.minRotationSpeed = 10f;     // 最小转速
processor.minDrillPressure = 200f;    // 最小钻压
processor.gravityMin = 0.98f;         // 重力值最小值
processor.gravityMax = 1.02f;         // 重力值最大值
```

#### 3.2 配置三维可视化器

```csharp
// 创建可视化器GameObject
GameObject visualizerObj = new GameObject("Trajectory3DVisualizer");
Trajectory3DVisualizer visualizer = visualizerObj.AddComponent<Trajectory3DVisualizer>();

// 设置数据源
visualizer.dataProcessor = processor;
visualizer.targetCamera = Camera.main;

// 配置显示选项
visualizer.showTrajectoryLine = true;      // 显示轨迹线
visualizer.showTrajectoryPoints = true;    // 显示轨迹点
visualizer.showCoordinateAxis = true;      // 显示坐标轴
visualizer.showDepthLabels = true;         // 显示深度标签

// 配置外观参数
visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Depth;  // 颜色方案
visualizer.lineWidth = 0.1f;              // 线宽
visualizer.pointSize = 0.5f;               // 点大小

// 配置交互参数
visualizer.enableInteractiveControls = true;  // 启用交互控制
visualizer.rotationSpeed = 100f;              // 旋转速度
visualizer.zoomSpeed = 10f;                    // 缩放速度
visualizer.autoRotate = false;                 // 自动旋转
visualizer.autoRotateSpeed = 30f;              // 自动旋转速度
```

#### 3.3 配置UI控制器

```csharp
// 创建UI控制器GameObject
GameObject controllerObj = new GameObject("Trajectory3DController");
Trajectory3DController controller = controllerObj.AddComponent<Trajectory3DController>();

// 设置组件引用
controller.dataProcessor = processor;
controller.visualizer = visualizer;
controller.targetCamera = Camera.main;
```

#### 3.4 创建UI界面

```csharp
// 创建Canvas
GameObject canvasObj = new GameObject("Canvas");
Canvas canvas = canvasObj.AddComponent<Canvas>();
canvas.renderMode = RenderMode.ScreenSpaceOverlay;
canvasObj.AddComponent<CanvasScaler>();
canvasObj.AddComponent<GraphicRaycaster>();

// 创建EventSystem（如果场景中没有）
if (FindObjectOfType<EventSystem>() == null)
{
    GameObject eventSystemObj = new GameObject("EventSystem");
    eventSystemObj.AddComponent<EventSystem>();
    eventSystemObj.AddComponent<StandaloneInputModule>();
}

// 创建控制面板
GameObject panelObj = new GameObject("ControlPanel");
panelObj.transform.SetParent(canvas.transform, false);
Image panel = panelObj.AddComponent<Image>();
panel.color = new Color(0, 0, 0, 0.7f);

// 设置面板布局
RectTransform panelRect = panelObj.GetComponent<RectTransform>();
panelRect.anchorMin = new Vector2(1, 1);
panelRect.anchorMax = new Vector2(1, 1);
panelRect.pivot = new Vector2(1, 1);
panelRect.anchoredPosition = new Vector2(-10, -10);
panelRect.sizeDelta = new Vector2(300, 600);
```

### 第四步：轨迹展示效果配置

#### 4.1 轨迹线效果设置

```csharp
// 创建轨迹线材质
Material lineMaterial = new Material(Shader.Find("Sprites/Default"));
lineMaterial.color = Color.white;
visualizer.lineMaterial = lineMaterial;

// LineRenderer配置
LineRenderer lineRenderer = visualizer.GetComponent<LineRenderer>();
if (lineRenderer != null)
{
    lineRenderer.startWidth = 0.1f;
    lineRenderer.endWidth = 0.1f;
    lineRenderer.useWorldSpace = true;
    lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    lineRenderer.receiveShadows = false;
}
```

#### 4.2 轨迹点效果设置

```csharp
// 创建轨迹点材质
Material pointMaterial = new Material(Shader.Find("Standard"));
pointMaterial.color = Color.white;
pointMaterial.SetFloat("_Metallic", 0f);
pointMaterial.SetFloat("_Glossiness", 0.5f);
visualizer.pointMaterial = pointMaterial;
```

#### 4.3 坐标轴效果设置

```csharp
// 创建坐标轴材质
Material axisMaterial = new Material(Shader.Find("Unlit/Color"));
visualizer.axisMaterial = axisMaterial;
```

#### 4.4 颜色方案配置

```csharp
// 深度着色方案
visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Depth;

// 倾角着色方案
visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Inclination;

// 方位角着色方案
visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Azimuth;

// 自定义着色方案
visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Custom;
```

### 第五步：相机和光照设置

#### 5.1 相机配置

```csharp
Camera mainCamera = Camera.main;

// 基本设置
mainCamera.clearFlags = CameraClearFlags.SolidColor;
mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.1f, 1f);
mainCamera.fieldOfView = 60f;

// 位置设置
mainCamera.transform.position = new Vector3(10f, 10f, 10f);
mainCamera.transform.LookAt(Vector3.zero);

// 渲染设置
mainCamera.nearClipPlane = 0.1f;
mainCamera.farClipPlane = 1000f;
```

#### 5.2 光照设置

```csharp
// 主光源
Light mainLight = FindObjectOfType<Light>();
if (mainLight != null)
{
    mainLight.type = LightType.Directional;
    mainLight.color = Color.white;
    mainLight.intensity = 1f;
    mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
}

// 环境光
RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
RenderSettings.ambientSkyColor = new Color(0.2f, 0.3f, 0.4f);
RenderSettings.ambientEquatorColor = new Color(0.1f, 0.1f, 0.1f);
RenderSettings.ambientGroundColor = new Color(0.05f, 0.05f, 0.05f);
```

### 第六步：数据处理和显示

#### 6.1 处理数据

```csharp
// 手动触发数据处理
processor.ProcessDrillingDataAndGenerateCharts();

// 获取处理结果
List<TrajectoryPoint> trajectoryPoints = processor.GetTrajectoryPoints();
List<DrillingDataItem> drillingData = processor.GetDrillingData();

Debug.Log($"轨迹点数量: {trajectoryPoints.Count}");
Debug.Log($"钻井数据点数量: {drillingData.Count}");
```

#### 6.2 更新可视化

```csharp
// 从数据处理器更新
visualizer.UpdateFromProcessor();

// 手动设置数据
visualizer.SetTrajectoryData(trajectoryPoints);

// 重置视角
visualizer.ResetView();
```

#### 6.3 监控文件变化（可选）

```csharp
// 启用自动监控
processor.enableAutoTrigger = true;
processor.monitorFileChanges = true;
processor.fileChangeDelay = 2f;  // 文件变化延迟
processor.minProcessInterval = 30f;  // 最小处理间隔

// 启动文件监控
processor.StartFileWatcher();
```

## 🎮 交互操作指南

### 鼠标交互

| 操作 | 功能 | 说明 |
|------|------|------|
| **左键拖拽** | 旋转视角 | 围绕轨迹中心旋转相机 |
| **滚轮** | 缩放视图 | 向前滚动放大，向后滚动缩小 |
| **中键拖拽** | 平移视图 | 平移相机位置 |
| **左键点击轨迹点** | 显示信息 | 弹出轨迹点详细信息 |

### 键盘快捷键

| 按键 | 功能 | 说明 |
|------|------|------|
| **R** | 重置视角 | 恢复默认相机位置和角度 |
| **Space** | 切换自动旋转 | 开启/关闭自动旋转 |
| **L** | 切换轨迹线 | 显示/隐藏轨迹线 |
| **P** | 切换轨迹点 | 显示/隐藏轨迹点 |
| **A** | 切换坐标轴 | 显示/隐藏坐标轴 |
| **D** | 切换深度标签 | 显示/隐藏深度标签 |

### UI控制面板

#### 控制按钮
```csharp
// 重置视角按钮
if (resetViewButton != null)
{
    resetViewButton.onClick.AddListener(() => visualizer.ResetView());
}

// 自动旋转切换按钮
if (autoRotateButton != null)
{
    autoRotateButton.onClick.AddListener(() => {
        visualizer.autoRotate = !visualizer.autoRotate;
        UpdateUI();
    });
}

// 轨迹线显示切换
if (toggleLineButton != null)
{
    toggleLineButton.onClick.AddListener(() => {
        visualizer.showTrajectoryLine = !visualizer.showTrajectoryLine;
        visualizer.UpdateFromProcessor();
    });
}
```

#### 参数滑块
```csharp
// 线宽调节
if (lineWidthSlider != null)
{
    lineWidthSlider.onValueChanged.AddListener((value) => {
        visualizer.lineWidth = value;
        visualizer.UpdateFromProcessor();
    });
}

// 点大小调节
if (pointSizeSlider != null)
{
    pointSizeSlider.onValueChanged.AddListener((value) => {
        visualizer.pointSize = value;
        visualizer.UpdateFromProcessor();
    });
}

// 旋转速度调节
if (rotationSpeedSlider != null)
{
    rotationSpeedSlider.onValueChanged.AddListener((value) => {
        visualizer.rotationSpeed = value;
    });
}
```

## 📊 坐标系统详解

### Unity坐标系映射

```
钻井数据坐标 → Unity世界坐标
┌─────────────────┬─────────────────┐
│ EastDisplacement │ → X轴 (右手坐标系) │
│ VerticalDepth    │ → Y轴 (向上为正)  │
│ NorthDisplacement│ → Z轴 (向前为正)  │
└─────────────────┴─────────────────┘
```

### 坐标转换代码

```csharp
public Vector3 ConvertToUnityCoordinate(TrajectoryPoint point)
{
    return new Vector3(
        point.eastDisplacement,     // X轴: 东向位移
        -point.verticalDepth,       // Y轴: 垂直深度 (负值，因为钻井向下)
        point.northDisplacement     // Z轴: 北向位移
    );
}
```

### 坐标轴颜色标准

```csharp
// X轴 - 红色 (East)
Color xAxisColor = Color.red;

// Y轴 - 绿色 (Up/Down) 
Color yAxisColor = Color.green;

// Z轴 - 蓝色 (North)
Color zAxisColor = Color.blue;
```

## 🎨 视觉效果优化

### 颜色方案实现

#### 深度着色
```csharp
Color GetDepthColor(TrajectoryPoint point, float minDepth, float maxDepth)
{
    float normalizedDepth = (point.verticalDepth - minDepth) / (maxDepth - minDepth);
    return Color.Lerp(Color.green, Color.red, normalizedDepth);
}
```

#### 倾角着色
```csharp
Color GetInclinationColor(TrajectoryPoint point)
{
    float normalizedInclination = point.inclination / 90f;
    return Color.Lerp(Color.blue, Color.yellow, normalizedInclination);
}
```

#### 方位角着色
```csharp
Color GetAzimuthColor(TrajectoryPoint point)
{
    float normalizedAzimuth = point.azimuth / 360f;
    return Color.HSVToRGB(normalizedAzimuth, 1f, 1f);
}
```

### 材质和着色器

#### 轨迹线材质
```csharp
Material CreateTrajectoryLineMaterial()
{
    Material mat = new Material(Shader.Find("Sprites/Default"));
    mat.color = Color.white;
    mat.SetInt("_ZWrite", 1);
    mat.SetInt("_ZTest", 4);
    return mat;
}
```

#### 轨迹点材质
```csharp
Material CreateTrajectoryPointMaterial()
{
    Material mat = new Material(Shader.Find("Standard"));
    mat.SetFloat("_Metallic", 0f);
    mat.SetFloat("_Glossiness", 0.5f);
    mat.EnableKeyword("_EMISSION");
    return mat;
}
```

### 光效和阴影

```csharp
// 启用实时阴影
QualitySettings.shadows = ShadowQuality.All;
QualitySettings.shadowResolution = ShadowResolution.High;

// 配置主光源阴影
Light mainLight = FindObjectOfType<Light>();
if (mainLight != null)
{
    mainLight.shadows = LightShadows.Soft;
    mainLight.shadowStrength = 0.5f;
}
```

## ⚡ 性能优化策略

### 大数据集优化

#### LOD系统
```csharp
public class TrajectoryLODManager : MonoBehaviour
{
    public float[] lodDistances = {50f, 100f, 200f};
    public int[] lodPointCounts = {1000, 500, 100};
    
    void Update()
    {
        float distance = Vector3.Distance(Camera.main.transform.position, transform.position);
        int lodLevel = GetLODLevel(distance);
        UpdatePointDisplay(lodLevel);
    }
    
    int GetLODLevel(float distance)
    {
        for (int i = 0; i < lodDistances.Length; i++)
        {
            if (distance < lodDistances[i])
                return i;
        }
        return lodDistances.Length;
    }
}
```

#### 批处理优化
```csharp
public void OptimizePointRendering()
{
    // 合并相近的轨迹点
    List<TrajectoryPoint> optimizedPoints = new List<TrajectoryPoint>();
    float mergeDistance = 0.5f;
    
    for (int i = 0; i < trajectoryPoints.Count; i++)
    {
        bool shouldMerge = false;
        foreach (var existing in optimizedPoints)
        {
            float distance = Vector3.Distance(
                ConvertToUnityCoordinate(trajectoryPoints[i]),
                ConvertToUnityCoordinate(existing)
            );
            if (distance < mergeDistance)
            {
                shouldMerge = true;
                break;
            }
        }
        
        if (!shouldMerge)
        {
            optimizedPoints.Add(trajectoryPoints[i]);
        }
    }
    
    trajectoryPoints = optimizedPoints;
}
```

### 内存管理

#### 对象池
```csharp
public class TrajectoryPointPool : MonoBehaviour
{
    private Queue<GameObject> pointPool = new Queue<GameObject>();
    public GameObject pointPrefab;
    
    public GameObject GetPoint()
    {
        if (pointPool.Count > 0)
        {
            return pointPool.Dequeue();
        }
        return Instantiate(pointPrefab);
    }
    
    public void ReturnPoint(GameObject point)
    {
        point.SetActive(false);
        pointPool.Enqueue(point);
    }
}
```

#### 资源清理
```csharp
public void ClearVisualization()
{
    // 清理轨迹对象
    foreach (GameObject obj in trajectoryObjects)
    {
        if (obj != null)
        {
            DestroyImmediate(obj);
        }
    }
    trajectoryObjects.Clear();
    
    // 清理点对象
    foreach (GameObject obj in pointObjects)
    {
        if (obj != null)
        {
            pointPool.ReturnPoint(obj);
        }
    }
    pointObjects.Clear();
    
    // 清理标签对象
    foreach (GameObject obj in labelObjects)
    {
        if (obj != null)
        {
            DestroyImmediate(obj);
        }
    }
    labelObjects.Clear();
}
```

## 🐛 故障排除

### 常见问题及解决方案

#### 问题1: 轨迹不显示
```csharp
// 检查数据处理状态
if (processor.GetTrajectoryPoints().Count == 0)
{
    Debug.LogError("没有轨迹点数据，请检查Excel文件是否正确处理");
    return;
}

// 检查可视化器配置
if (!visualizer.showTrajectoryLine && !visualizer.showTrajectoryPoints)
{
    Debug.LogWarning("轨迹线和轨迹点都已隐藏，请开启至少一个显示选项");
}

// 检查相机位置
float distance = Vector3.Distance(Camera.main.transform.position, Vector3.zero);
if (distance > 1000f)
{
    Debug.LogWarning("相机距离过远，可能看不到轨迹");
    visualizer.ResetView();
}
```

#### 问题2: 性能问题
```csharp
// 性能检测
void CheckPerformance()
{
    if (trajectoryPoints.Count > 1000)
    {
        Debug.LogWarning("轨迹点数量过多，建议优化显示");
        
        // 自动优化
        visualizer.showTrajectoryPoints = false;  // 关闭点显示
        visualizer.showDepthLabels = false;       // 关闭标签
        visualizer.lineWidth = 0.05f;             // 减小线宽
    }
    
    // 检查帧率
    if (Time.deltaTime > 0.033f)  // 低于30FPS
    {
        Debug.LogWarning("帧率较低，建议降低可视化复杂度");
    }
}
```

#### 问题3: 交互不响应
```csharp
// 检查EventSystem
if (FindObjectOfType<EventSystem>() == null)
{
    Debug.LogError("场景中缺少EventSystem，UI交互将无法正常工作");
    
    // 自动创建
    GameObject eventSystem = new GameObject("EventSystem");
    eventSystem.AddComponent<EventSystem>();
    eventSystem.AddComponent<StandaloneInputModule>();
}

// 检查相机组件
if (Camera.main == null)
{
    Debug.LogError("场景中没有MainCamera标签的相机");
}
```

#### 问题4: 数据格式错误
```csharp
// 验证Excel数据格式
bool ValidateExcelData(string filePath)
{
    if (!File.Exists(filePath))
    {
        Debug.LogError($"Excel文件不存在: {filePath}");
        return false;
    }
    
    // 检查文件扩展名
    if (!filePath.EndsWith(".xlsx") && !filePath.EndsWith(".xls"))
    {
        Debug.LogError("文件格式不正确，请使用Excel格式(.xlsx或.xls)");
        return false;
    }
    
    return true;
}
```

## 📱 扩展功能

### VR支持
```csharp
#if UNITY_XR
public class VRTrajectoryController : MonoBehaviour
{
    public XRRig xrRig;
    
    void Start()
    {
        // 启用VR模式
        XRGeneralSettings.Instance.Manager.InitializeLoader();
        XRGeneralSettings.Instance.Manager.StartSubsystems();
        
        // 配置VR交互
        SetupVRInteraction();
    }
    
    void SetupVRInteraction()
    {
        // VR手柄交互逻辑
        // ...
    }
}
#endif
```

### 多轨迹支持
```csharp
public class MultiTrajectoryManager : MonoBehaviour
{
    public List<UnityDrillingDataProcessor> processors = new List<UnityDrillingDataProcessor>();
    public List<Trajectory3DVisualizer> visualizers = new List<Trajectory3DVisualizer>();
    
    public void LoadMultipleTrajectories(string[] filePaths)
    {
        for (int i = 0; i < filePaths.Length; i++)
        {
            // 创建处理器
            GameObject processorObj = new GameObject($"Processor_{i}");
            UnityDrillingDataProcessor processor = processorObj.AddComponent<UnityDrillingDataProcessor>();
            processor.inputExcelPath = filePaths[i];
            processors.Add(processor);
            
            // 创建可视化器
            GameObject visualizerObj = new GameObject($"Visualizer_{i}");
            Trajectory3DVisualizer visualizer = visualizerObj.AddComponent<Trajectory3DVisualizer>();
            visualizer.dataProcessor = processor;
            visualizers.Add(visualizer);
            
            // 设置不同颜色
            visualizer.colorScheme = (Trajectory3DVisualizer.ColorScheme)(i % 4);
        }
    }
}
```

### 数据导出功能
```csharp
public class TrajectoryExporter : MonoBehaviour
{
    public void ExportToCSV(List<TrajectoryPoint> points, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("Index,EastDisplacement,NorthDisplacement,VerticalDepth,Inclination,Azimuth");
            
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i];
                writer.WriteLine($"{i},{point.eastDisplacement},{point.northDisplacement},{point.verticalDepth},{point.inclination},{point.azimuth}");
            }
        }
        
        Debug.Log($"轨迹数据已导出到: {filePath}");
    }
    
    public void ExportScreenshot(string filePath)
    {
        StartCoroutine(CaptureScreenshot(filePath));
    }
    
    IEnumerator CaptureScreenshot(string filePath)
    {
        yield return new WaitForEndOfFrame();
        
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();
        byte[] data = screenshot.EncodeToPNG();
        File.WriteAllBytes(filePath, data);
        
        Destroy(screenshot);
        Debug.Log($"截图已保存到: {filePath}");
    }
}
```

## 📋 检查清单

### 部署前检查
- [ ] Unity版本兼容性 (2022.3 LTS+)
- [ ] 所有必要组件已添加
- [ ] Excel数据文件路径正确
- [ ] UI界面正常显示
- [ ] 鼠标和键盘交互正常
- [ ] 轨迹可视化效果正确
- [ ] 性能表现可接受

### 测试检查
- [ ] 数据处理功能正常
- [ ] 轨迹显示准确
- [ ] 交互控制响应
- [ ] UI参数调节有效
- [ ] 错误处理机制正常
- [ ] 文件监控功能正常

### 优化检查
- [ ] 大数据集性能测试
- [ ] 内存使用情况检查
- [ ] 渲染性能优化
- [ ] 用户体验优化

---

## 💡 总结

通过本指南，你已经学会了如何在Unity中创建专业的钻井轨迹三维可视化系统。系统提供了完整的数据处理、可视化渲染、交互控制功能，能够满足专业的钻井数据分析和展示需求。

关键要点：
1. **自动化设置**: 使用`Trajectory3DSceneSetup`快速搭建场景
2. **数据集成**: 通过`UnityDrillingDataProcessor`无缝处理Excel数据
3. **可视化效果**: `Trajectory3DVisualizer`提供丰富的显示选项
4. **交互控制**: 完整的鼠标、键盘和UI交互系统
5. **性能优化**: 针对大数据集的优化策略
6. **扩展性**: 支持VR、多轨迹等高级功能

**最后更新**: 2024年12月 | **版本**: v2.0 