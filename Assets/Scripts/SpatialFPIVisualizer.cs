using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
// Removed Newtonsoft.Json to avoid dynamic binding issues

[Serializable]
public class SpatialFPIPoint
{
    public Vector3 position;
    public float fpi;
}

[Serializable]
public class SpatialFPIData
{
    public string title = "FPI空间插值可视化";
    public string type = "spatial_interpolation";
    public string processedTime;
    public SpatialBounds bounds;
    public float resolution;
    public SpatialStatistics statistics;
    public List<SpatialFPIPoint> points = new List<SpatialFPIPoint>();
}

[Serializable]
public class SpatialBounds
{
    public Vector3 min;
    public Vector3 max;
}

[Serializable]
public class SpatialStatistics
{
    public int totalPoints;
    public float minFPI;
    public float maxFPI;
    public float avgFPI;
}

// Raw JSON data structures for parsing
[Serializable]
public class RawVector3
{
    public float x;
    public float y;
    public float z;
}

[Serializable]
public class RawBounds
{
    public RawVector3 min;
    public RawVector3 max;
}

[Serializable]
public class RawStatistics
{
    public int totalPoints;
    public float minFPI;
    public float maxFPI;
    public float avgFPI;
}

[Serializable]
public class RawSpatialPoint
{
    public RawVector3 position;
    public float fpi;
}

[Serializable]
public class RawSpatialData
{
    public string title;
    public string type;
    public string processedTime;
    public float resolution;
    public RawBounds bounds;
    public RawStatistics statistics;
    public RawSpatialPoint[] points;
}

/// <summary>
/// 空间FPI插值可视化器
/// </summary>
public class SpatialFPIVisualizer : MonoBehaviour
{
    [Header("数据源")]
    public string spatialDataFileName = "spatial_fpi_visualization.json";
    public bool loadFromStreamingAssets = true;
    public bool autoLoadOnStart = true;

    [Header("可视化设置")]
    public bool showSpatialPoints = true;
    public bool showBoundingBox = true;
    public bool showColorLegend = true;
    public bool enableLOD = true; // Level of Detail

    [Header("渲染设置")]
    public VisualizationMode visualizationMode = VisualizationMode.Points;
    public ColorScheme colorScheme = ColorScheme.Heat;
    public float pointSize = 0.1f;
    public float transparency = 0.8f;
    public int maxVisiblePoints = 5000; // LOD控制

    [Header("材质设置")]
    public Material pointMaterial;
    public Material boundingBoxMaterial;
    public Shader spatialPointShader;

    [Header("图例设置")]
    public GameObject legendPrefab;
    public Canvas legendCanvas;
    public Vector2 legendPosition = new Vector2(50, 50);
    public Vector2 legendSize = new Vector2(200, 300);

    [Header("性能设置")]
    public bool useGPUInstancing = true;
    public bool enableFrustumCulling = true;
    public float lodDistance1 = 50f;
    public float lodDistance2 = 100f;

    // 私有变量
    private SpatialFPIData spatialData;
    private List<GameObject> spatialPointObjects = new List<GameObject>();
    private GameObject boundingBoxObject;
    private GameObject legendObject;
    private Mesh pointMesh;
    private Material[] lodMaterials;
    private Camera mainCamera;
    
    // LOD系统
    private List<SpatialFPIPoint> visiblePoints = new List<SpatialFPIPoint>();
    private float lastLODUpdateTime;
    private const float LOD_UPDATE_INTERVAL = 0.1f; // LOD更新间隔

    // 颜色映射
    private readonly Color[] heatMapColors = new Color[]
    {
        new Color(0f, 0f, 1f),    // 蓝色 (低值)
        new Color(0f, 1f, 1f),    // 青色
        new Color(0f, 1f, 0f),    // 绿色
        new Color(1f, 1f, 0f),    // 黄色
        new Color(1f, 0.5f, 0f),  // 橙色
        new Color(1f, 0f, 0f)     // 红色 (高值)
    };

    public enum VisualizationMode
    {
        Points,      // 点云显示
        Voxels,      // 体素显示
        Isosurface,  // 等值面显示
        VolumeRender // 体积渲染
    }

    public enum ColorScheme
    {
        Heat,        // 热度图 (蓝-红)
        Rainbow,     // 彩虹色
        Grayscale,   // 灰度
        Custom       // 自定义
    }

    void Start()
    {
        InitializeVisualizer();
        
        if (autoLoadOnStart)
        {
            LoadSpatialData();
        }
    }

    void Update()
    {
        if (enableLOD && Time.time - lastLODUpdateTime > LOD_UPDATE_INTERVAL)
        {
            UpdateLOD();
            lastLODUpdateTime = Time.time;
        }
    }

    /// <summary>
    /// 初始化可视化器
    /// </summary>
    void InitializeVisualizer()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }

        // 创建材质
        CreateMaterials();
        
        // 创建基础网格
        CreatePointMesh();
        
        Debug.Log("空间FPI可视化器初始化完成");
    }

    /// <summary>
    /// 加载空间插值数据
    /// </summary>
    public void LoadSpatialData()
    {
        string filePath = GetDataFilePath();
        
        try
        {
            string jsonContent = ReadDataFile(filePath);
            var rawData = JsonUtility.FromJson<RawSpatialData>(jsonContent);
            
            // 解析数据
            spatialData = ParseSpatialData(rawData);
            
            if (spatialData != null && spatialData.points.Count > 0)
            {
                Debug.Log($"成功加载空间插值数据：{spatialData.points.Count} 个插值点");
                CreateSpatialVisualization();
            }
            else
            {
                Debug.LogWarning("空间插值数据为空");
                CreateSampleData();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载空间插值数据失败: {e.Message}");
            CreateSampleData();
        }
    }

    /// <summary>
    /// 获取数据文件路径
    /// </summary>
    string GetDataFilePath()
    {
        if (loadFromStreamingAssets)
        {
            return Path.Combine(Application.streamingAssetsPath, spatialDataFileName);
        }
        else
        {
            return Path.Combine(Application.persistentDataPath, spatialDataFileName);
        }
    }

    /// <summary>
    /// 读取数据文件
    /// </summary>
    string ReadDataFile(string filePath)
    {
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            // 处理WebGL和Android的StreamingAssets
            var www = new WWW(filePath);
            while (!www.isDone) { }
            return www.text;
        }
        else
        {
            return File.ReadAllText(filePath);
        }
    }

    /// <summary>
    /// 解析空间数据
    /// </summary>
    SpatialFPIData ParseSpatialData(RawSpatialData rawData)
    {
        var data = new SpatialFPIData();
        
        try
        {
            data.title = !string.IsNullOrEmpty(rawData.title) ? rawData.title : "FPI空间插值";
            data.type = !string.IsNullOrEmpty(rawData.type) ? rawData.type : "spatial_interpolation";
            data.processedTime = rawData.processedTime;
            data.resolution = rawData.resolution > 0 ? rawData.resolution : 0.5f;
            
            // 解析边界
            if (rawData.bounds != null && rawData.bounds.min != null && rawData.bounds.max != null)
            {
                data.bounds = new SpatialBounds
                {
                    min = new Vector3(rawData.bounds.min.x, rawData.bounds.min.y, rawData.bounds.min.z),
                    max = new Vector3(rawData.bounds.max.x, rawData.bounds.max.y, rawData.bounds.max.z)
                };
            }
            
            // 解析统计信息
            if (rawData.statistics != null)
            {
                data.statistics = new SpatialStatistics
                {
                    totalPoints = rawData.statistics.totalPoints,
                    minFPI = rawData.statistics.minFPI,
                    maxFPI = rawData.statistics.maxFPI,
                    avgFPI = rawData.statistics.avgFPI
                };
            }
            
            // 解析插值点
            if (rawData.points != null)
            {
                foreach (var point in rawData.points)
                {
                    if (point?.position != null)
                    {
                        var spatialPoint = new SpatialFPIPoint
                        {
                            position = new Vector3(
                                point.position.x,
                                point.position.y,
                                point.position.z
                            ),
                            fpi = point.fpi
                        };
                        data.points.Add(spatialPoint);
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"解析空间数据失败: {e.Message}");
            return null;
        }
        
        return data;
    }

    /// <summary>
    /// 创建空间可视化
    /// </summary>
    void CreateSpatialVisualization()
    {
        // 清理现有可视化
        ClearVisualization();
        
        // 创建空间点
        if (showSpatialPoints)
        {
            CreateSpatialPoints();
        }
        
        // 创建边界框
        if (showBoundingBox)
        {
            CreateBoundingBox();
        }
        
        // 创建图例
        if (showColorLegend)
        {
            CreateColorLegend();
        }
        
        // 居中显示
        CenterVisualization();
    }

    /// <summary>
    /// 创建空间插值点
    /// </summary>
    void CreateSpatialPoints()
    {
        if (spatialData?.points == null || spatialData.points.Count == 0) return;
        
        // 应用LOD过滤
        var pointsToRender = ApplyLODFiltering(spatialData.points);
        
        Debug.Log($"渲染 {pointsToRender.Count} / {spatialData.points.Count} 个空间点");
        
        switch (visualizationMode)
        {
            case VisualizationMode.Points:
                CreatePointCloud(pointsToRender);
                break;
            case VisualizationMode.Voxels:
                CreateVoxelVisualization(pointsToRender);
                break;
            case VisualizationMode.Isosurface:
                CreateIsosurfaceVisualization(pointsToRender);
                break;
            case VisualizationMode.VolumeRender:
                CreateVolumeRenderVisualization(pointsToRender);
                break;
        }
    }

    /// <summary>
    /// 创建点云可视化
    /// </summary>
    void CreatePointCloud(List<SpatialFPIPoint> points)
    {
        GameObject pointsContainer = new GameObject("SpatialFPIPoints");
        pointsContainer.transform.SetParent(transform);
        
        foreach (var point in points)
        {
            GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObj.name = $"SpatialPoint_FPI_{point.fpi:F2}";
            pointObj.transform.SetParent(pointsContainer.transform);
            pointObj.transform.position = ConvertToUnityCoords(point.position);
            pointObj.transform.localScale = Vector3.one * pointSize;
            
            // 设置材质和颜色
            Renderer renderer = pointObj.GetComponent<Renderer>();
            renderer.material = pointMaterial ? Instantiate(pointMaterial) : CreateDefaultPointMaterial();
            renderer.material.color = GetColorForFPI(point.fpi);
            
            // 设置透明度
            if (transparency < 1f)
            {
                SetMaterialTransparency(renderer.material, transparency);
            }
            
            // 添加点信息组件
            var pointInfo = pointObj.AddComponent<SpatialFPIPointInfo>();
            pointInfo.Initialize(point);
            
            spatialPointObjects.Add(pointObj);
        }
    }

    /// <summary>
    /// 创建体素可视化
    /// </summary>
    void CreateVoxelVisualization(List<SpatialFPIPoint> points)
    {
        GameObject voxelsContainer = new GameObject("SpatialFPIVoxels");
        voxelsContainer.transform.SetParent(transform);
        
        foreach (var point in points)
        {
            GameObject voxelObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            voxelObj.name = $"Voxel_FPI_{point.fpi:F2}";
            voxelObj.transform.SetParent(voxelsContainer.transform);
            voxelObj.transform.position = ConvertToUnityCoords(point.position);
            
            // 体素大小基于分辨率
            float voxelSize = spatialData.resolution * 0.8f; // 略小于分辨率以避免重叠
            voxelObj.transform.localScale = Vector3.one * voxelSize;
            
            // 设置材质和颜色
            Renderer renderer = voxelObj.GetComponent<Renderer>();
            renderer.material = pointMaterial ? Instantiate(pointMaterial) : CreateDefaultPointMaterial();
            renderer.material.color = GetColorForFPI(point.fpi);
            
            // 根据FPI值调整透明度
            float alpha = Mathf.Lerp(0.1f, transparency, GetNormalizedFPI(point.fpi));
            SetMaterialTransparency(renderer.material, alpha);
            
            spatialPointObjects.Add(voxelObj);
        }
    }

    /// <summary>
    /// 创建等值面可视化（简化版）
    /// </summary>
    void CreateIsosurfaceVisualization(List<SpatialFPIPoint> points)
    {
        // 这里实现简化的等值面，实际项目中可能需要Marching Cubes算法
        Debug.LogWarning("等值面可视化需要更复杂的算法实现，当前使用简化版");
        CreatePointCloud(points);
    }

    /// <summary>
    /// 创建体积渲染可视化（简化版）
    /// </summary>
    void CreateVolumeRenderVisualization(List<SpatialFPIPoint> points)
    {
        // 体积渲染需要3D纹理和专门的着色器
        Debug.LogWarning("体积渲染需要专门的着色器支持，当前使用简化版");
        CreateVoxelVisualization(points);
    }

    /// <summary>
    /// 创建边界框
    /// </summary>
    void CreateBoundingBox()
    {
        if (spatialData?.bounds == null) return;
        
        boundingBoxObject = new GameObject("BoundingBox");
        boundingBoxObject.transform.SetParent(transform);
        
        // 创建线框
        LineRenderer[] edges = new LineRenderer[12]; // 立方体有12条边
        Vector3[] corners = GetBoundingBoxCorners();
        
        // 定义立方体的边
        int[,] edgeIndices = new int[12, 2]
        {
            {0,1}, {1,3}, {3,2}, {2,0}, // 底面
            {4,5}, {5,7}, {7,6}, {6,4}, // 顶面
            {0,4}, {1,5}, {2,6}, {3,7}  // 竖直边
        };
        
        for (int i = 0; i < 12; i++)
        {
            GameObject edgeObj = new GameObject($"Edge_{i}");
            edgeObj.transform.SetParent(boundingBoxObject.transform);
            
            LineRenderer lr = edgeObj.AddComponent<LineRenderer>();
            lr.material = boundingBoxMaterial ? boundingBoxMaterial : CreateDefaultLineMaterial();
            lr.startWidth = 0.02f;
            lr.endWidth = 0.02f;
            lr.positionCount = 2;
            lr.useWorldSpace = false;
            
            Vector3 start = ConvertToUnityCoords(corners[edgeIndices[i, 0]]);
            Vector3 end = ConvertToUnityCoords(corners[edgeIndices[i, 1]]);
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
            
            edges[i] = lr;
        }
    }

    /// <summary>
    /// 获取边界框角点
    /// </summary>
    Vector3[] GetBoundingBoxCorners()
    {
        var bounds = spatialData.bounds;
        return new Vector3[]
        {
            new Vector3(bounds.min.x, bounds.min.y, bounds.min.z), // 0
            new Vector3(bounds.max.x, bounds.min.y, bounds.min.z), // 1
            new Vector3(bounds.min.x, bounds.max.y, bounds.min.z), // 2
            new Vector3(bounds.max.x, bounds.max.y, bounds.min.z), // 3
            new Vector3(bounds.min.x, bounds.min.y, bounds.max.z), // 4
            new Vector3(bounds.max.x, bounds.min.y, bounds.max.z), // 5
            new Vector3(bounds.min.x, bounds.max.y, bounds.max.z), // 6
            new Vector3(bounds.max.x, bounds.max.y, bounds.max.z)  // 7
        };
    }

    /// <summary>
    /// 创建颜色图例
    /// </summary>
    void CreateColorLegend()
    {
        if (spatialData?.statistics == null) return;
        
        // 如果没有指定Canvas，创建一个
        if (legendCanvas == null)
        {
            GameObject canvasObj = new GameObject("LegendCanvas");
            legendCanvas = canvasObj.AddComponent<Canvas>();
            legendCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        // 创建图例对象
        legendObject = new GameObject("FPIColorLegend");
        legendObject.transform.SetParent(legendCanvas.transform);
        
        // 设置RectTransform
        RectTransform rectTransform = legendObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 0);
        rectTransform.anchorMax = new Vector2(0, 0);
        rectTransform.anchoredPosition = legendPosition;
        rectTransform.sizeDelta = legendSize;
        
        // 创建背景
        var bgImage = legendObject.AddComponent<UnityEngine.UI.Image>();
        bgImage.color = new Color(0, 0, 0, 0.7f);
        
        // 创建图例内容
        CreateLegendContent(legendObject);
    }

    /// <summary>
    /// 创建图例内容
    /// </summary>
    void CreateLegendContent(GameObject parent)
    {
        // 创建标题
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(parent.transform);
        var titleText = titleObj.AddComponent<UnityEngine.UI.Text>();
        titleText.text = "FPI值";
        titleText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        titleText.fontSize = 16;
        titleText.color = Color.white;
        titleText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 0.85f);
        titleRect.anchorMax = new Vector2(1, 1f);
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
        
        // 创建颜色条
        CreateColorBar(parent);
        
        // 创建数值标签
        CreateValueLabels(parent);
        
        // 创建统计信息
        CreateStatisticsInfo(parent);
    }

    /// <summary>
    /// 创建颜色条
    /// </summary>
    void CreateColorBar(GameObject parent)
    {
        GameObject colorBarObj = new GameObject("ColorBar");
        colorBarObj.transform.SetParent(parent.transform);
        
        RectTransform colorBarRect = colorBarObj.AddComponent<RectTransform>();
        colorBarRect.anchorMin = new Vector2(0.2f, 0.3f);
        colorBarRect.anchorMax = new Vector2(0.4f, 0.8f);
        colorBarRect.offsetMin = Vector2.zero;
        colorBarRect.offsetMax = Vector2.zero;
        
        // 创建颜色段
        int colorSegments = heatMapColors.Length - 1;
        for (int i = 0; i < colorSegments; i++)
        {
            GameObject segmentObj = new GameObject($"ColorSegment_{i}");
            segmentObj.transform.SetParent(colorBarObj.transform);
            
            var segmentImage = segmentObj.AddComponent<UnityEngine.UI.Image>();
            segmentImage.color = heatMapColors[i];
            
            RectTransform segmentRect = segmentObj.GetComponent<RectTransform>();
            float yMin = (float)i / colorSegments;
            float yMax = (float)(i + 1) / colorSegments;
            segmentRect.anchorMin = new Vector2(0, yMin);
            segmentRect.anchorMax = new Vector2(1, yMax);
            segmentRect.offsetMin = Vector2.zero;
            segmentRect.offsetMax = Vector2.zero;
        }
    }

    /// <summary>
    /// 创建数值标签
    /// </summary>
    void CreateValueLabels(GameObject parent)
    {
        float minFPI = spatialData.statistics.minFPI;
        float maxFPI = spatialData.statistics.maxFPI;
        
        // 创建最大值标签
        CreateValueLabel(parent, "MaxLabel", maxFPI.ToString("F0"), new Vector2(0.5f, 0.8f));
        
        // 创建中值标签
        float midFPI = (minFPI + maxFPI) / 2;
        CreateValueLabel(parent, "MidLabel", midFPI.ToString("F0"), new Vector2(0.5f, 0.55f));
        
        // 创建最小值标签
        CreateValueLabel(parent, "MinLabel", minFPI.ToString("F0"), new Vector2(0.5f, 0.3f));
    }

    /// <summary>
    /// 创建单个数值标签
    /// </summary>
    void CreateValueLabel(GameObject parent, string name, string value, Vector2 anchorPos)
    {
        GameObject labelObj = new GameObject(name);
        labelObj.transform.SetParent(parent.transform);
        
        var labelText = labelObj.AddComponent<UnityEngine.UI.Text>();
        labelText.text = value;
        labelText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        labelText.fontSize = 12;
        labelText.color = Color.white;
        labelText.alignment = TextAnchor.MiddleLeft;
        
        RectTransform labelRect = labelObj.GetComponent<RectTransform>();
        labelRect.anchorMin = anchorPos;
        labelRect.anchorMax = anchorPos;
        labelRect.sizeDelta = new Vector2(80, 20);
        labelRect.anchoredPosition = Vector2.zero;
    }

    /// <summary>
    /// 创建统计信息
    /// </summary>
    void CreateStatisticsInfo(GameObject parent)
    {
        GameObject statsObj = new GameObject("Statistics");
        statsObj.transform.SetParent(parent.transform);
        
        var statsText = statsObj.AddComponent<UnityEngine.UI.Text>();
        var stats = spatialData.statistics;
        statsText.text = $"点数: {stats.totalPoints}\n平均: {stats.avgFPI:F0}\n范围: {stats.maxFPI - stats.minFPI:F0}";
        statsText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        statsText.fontSize = 10;
        statsText.color = Color.white;
        statsText.alignment = TextAnchor.UpperLeft;
        
        RectTransform statsRect = statsObj.GetComponent<RectTransform>();
        statsRect.anchorMin = new Vector2(0.1f, 0.05f);
        statsRect.anchorMax = new Vector2(0.9f, 0.25f);
        statsRect.offsetMin = Vector2.zero;
        statsRect.offsetMax = Vector2.zero;
    }

    /// <summary>
    /// 应用LOD过滤
    /// </summary>
    List<SpatialFPIPoint> ApplyLODFiltering(List<SpatialFPIPoint> allPoints)
    {
        if (!enableLOD || allPoints.Count <= maxVisiblePoints)
        {
            return allPoints;
        }
        
        // 基于相机距离的LOD
        if (mainCamera != null)
        {
            var cameraPos = mainCamera.transform.position;
            
            // 按距离排序并选择最近的点
            var sortedPoints = allPoints
                .OrderBy(p => Vector3.Distance(ConvertToUnityCoords(p.position), cameraPos))
                .Take(maxVisiblePoints)
                .ToList();
            
            return sortedPoints;
        }
        
        // 如果没有相机，使用均匀采样
        int step = Mathf.Max(1, allPoints.Count / maxVisiblePoints);
        var sampledPoints = new List<SpatialFPIPoint>();
        for (int i = 0; i < allPoints.Count; i += step)
        {
            sampledPoints.Add(allPoints[i]);
        }
        
        return sampledPoints;
    }

    /// <summary>
    /// 更新LOD
    /// </summary>
    void UpdateLOD()
    {
        if (!enableLOD || spatialData?.points == null) return;
        
        // 重新计算可见点并更新可视化
        var newVisiblePoints = ApplyLODFiltering(spatialData.points);
        
        if (!ArePointListsEqual(visiblePoints, newVisiblePoints))
        {
            visiblePoints = newVisiblePoints;
            // 只更新空间点，不重新创建整个可视化
            UpdateSpatialPointsDisplay();
        }
    }

    /// <summary>
    /// 更新空间点显示
    /// </summary>
    void UpdateSpatialPointsDisplay()
    {
        // 清理现有点
        foreach (var obj in spatialPointObjects)
        {
            if (obj != null) DestroyImmediate(obj);
        }
        spatialPointObjects.Clear();
        
        // 重新创建点
        CreatePointCloud(visiblePoints);
    }

    /// <summary>
    /// 比较两个点列表是否相等
    /// </summary>
    bool ArePointListsEqual(List<SpatialFPIPoint> list1, List<SpatialFPIPoint> list2)
    {
        if (list1.Count != list2.Count) return false;
        
        for (int i = 0; i < list1.Count; i++)
        {
            if (Vector3.Distance(list1[i].position, list2[i].position) > 0.01f)
                return false;
        }
        
        return true;
    }

    /// <summary>
    /// 坐标转换：数据坐标 -> Unity坐标
    /// </summary>
    Vector3 ConvertToUnityCoords(Vector3 dataPos)
    {
        // 根据需要调整坐标系转换
        return new Vector3(dataPos.x, dataPos.z, dataPos.y);
    }

    /// <summary>
    /// 根据FPI值获取颜色
    /// </summary>
    Color GetColorForFPI(float fpi)
    {
        if (spatialData?.statistics == null) return Color.white;
        
        float normalizedFPI = GetNormalizedFPI(fpi);
        
        switch (colorScheme)
        {
            case ColorScheme.Heat:
                return GetHeatMapColor(normalizedFPI);
            case ColorScheme.Rainbow:
                return GetRainbowColor(normalizedFPI);
            case ColorScheme.Grayscale:
                return Color.Lerp(Color.black, Color.white, normalizedFPI);
            case ColorScheme.Custom:
                return GetCustomColor(normalizedFPI);
            default:
                return GetHeatMapColor(normalizedFPI);
        }
    }

    /// <summary>
    /// 获取标准化的FPI值 [0,1]
    /// </summary>
    float GetNormalizedFPI(float fpi)
    {
        var stats = spatialData.statistics;
        if (stats.maxFPI <= stats.minFPI) return 0f;
        
        return Mathf.Clamp01((fpi - stats.minFPI) / (stats.maxFPI - stats.minFPI));
    }

    /// <summary>
    /// 获取热度图颜色
    /// </summary>
    Color GetHeatMapColor(float normalizedValue)
    {
        if (normalizedValue <= 0f) return heatMapColors[0];
        if (normalizedValue >= 1f) return heatMapColors[heatMapColors.Length - 1];
        
        float scaledValue = normalizedValue * (heatMapColors.Length - 1);
        int index = Mathf.FloorToInt(scaledValue);
        float t = scaledValue - index;
        
        return Color.Lerp(heatMapColors[index], heatMapColors[index + 1], t);
    }

    /// <summary>
    /// 获取彩虹颜色
    /// </summary>
    Color GetRainbowColor(float normalizedValue)
    {
        return Color.HSVToRGB(normalizedValue * 0.8f, 1f, 1f); // 0.8f避免从红色回到红色
    }

    /// <summary>
    /// 获取自定义颜色
    /// </summary>
    Color GetCustomColor(float normalizedValue)
    {
        // 可以在这里实现自定义颜色方案
        return GetHeatMapColor(normalizedValue);
    }

    /// <summary>
    /// 设置材质透明度
    /// </summary>
    void SetMaterialTransparency(Material material, float alpha)
    {
        // 设置渲染模式为透明
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000;
        
        Color color = material.color;
        color.a = alpha;
        material.color = color;
    }

    /// <summary>
    /// 创建材质
    /// </summary>
    void CreateMaterials()
    {
        if (pointMaterial == null)
        {
            pointMaterial = CreateDefaultPointMaterial();
        }
        
        if (boundingBoxMaterial == null)
        {
            boundingBoxMaterial = CreateDefaultLineMaterial();
        }
    }

    /// <summary>
    /// 创建默认点材质
    /// </summary>
    Material CreateDefaultPointMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.SetFloat("_Metallic", 0f);
        mat.SetFloat("_Glossiness", 0.5f);
        return mat;
    }

    /// <summary>
    /// 创建默认线材质
    /// </summary>
    Material CreateDefaultLineMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.white;
        return mat;
    }

    /// <summary>
    /// 创建点网格
    /// </summary>
    void CreatePointMesh()
    {
        // 为GPU实例化创建基础网格
        pointMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
    }

    /// <summary>
    /// 清理可视化
    /// </summary>
    void ClearVisualization()
    {
        // 清理空间点
        foreach (var obj in spatialPointObjects)
        {
            if (obj != null) DestroyImmediate(obj);
        }
        spatialPointObjects.Clear();
        
        // 清理边界框
        if (boundingBoxObject != null)
        {
            DestroyImmediate(boundingBoxObject);
        }
        
        // 清理图例
        if (legendObject != null)
        {
            DestroyImmediate(legendObject);
        }
    }

    /// <summary>
    /// 居中可视化
    /// </summary>
    void CenterVisualization()
    {
        if (spatialData?.bounds == null) return;
        
        Vector3 center = (spatialData.bounds.min + spatialData.bounds.max) / 2f;
        Vector3 unityCenter = ConvertToUnityCoords(center);
        transform.position = -unityCenter;
    }

    /// <summary>
    /// 创建示例数据
    /// </summary>
    void CreateSampleData()
    {
        Debug.Log("创建示例空间插值数据");
        
        spatialData = new SpatialFPIData();
        spatialData.title = "示例FPI空间插值";
        spatialData.resolution = 1f;
        spatialData.bounds = new SpatialBounds
        {
            min = new Vector3(-10, -10, -10),
            max = new Vector3(10, 10, 10)
        };
        
        // 创建示例点
        for (int x = -5; x <= 5; x += 2)
        {
            for (int y = -5; y <= 5; y += 2)
            {
                for (int z = -5; z <= 5; z += 2)
                {
                    float distance = Mathf.Sqrt(x * x + y * y + z * z);
                    float fpi = 1000000f + distance * 100000f;
                    
                    spatialData.points.Add(new SpatialFPIPoint
                    {
                        position = new Vector3(x, y, z),
                        fpi = fpi
                    });
                }
            }
        }
        
        spatialData.statistics = new SpatialStatistics
        {
            totalPoints = spatialData.points.Count,
            minFPI = spatialData.points.Min(p => p.fpi),
            maxFPI = spatialData.points.Max(p => p.fpi),
            avgFPI = spatialData.points.Average(p => p.fpi)
        };
        
        CreateSpatialVisualization();
    }

    /// <summary>
    /// 切换可视化模式
    /// </summary>
    [ContextMenu("切换可视化模式")]
    public void ToggleVisualizationMode()
    {
        visualizationMode = (VisualizationMode)(((int)visualizationMode + 1) % System.Enum.GetValues(typeof(VisualizationMode)).Length);
        if (spatialData != null)
        {
            CreateSpatialVisualization();
        }
    }

    /// <summary>
    /// 切换颜色方案
    /// </summary>
    [ContextMenu("切换颜色方案")]
    public void ToggleColorScheme()
    {
        colorScheme = (ColorScheme)(((int)colorScheme + 1) % System.Enum.GetValues(typeof(ColorScheme)).Length);
        if (spatialData != null)
        {
            CreateSpatialVisualization();
        }
    }

    /// <summary>
    /// 重新加载数据
    /// </summary>
    [ContextMenu("重新加载数据")]
    public void ReloadData()
    {
        LoadSpatialData();
    }

    void OnDestroy()
    {
        ClearVisualization();
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (spatialData?.bounds != null)
        {
            // 绘制边界框
            Gizmos.color = Color.yellow;
            Vector3 center = ConvertToUnityCoords((spatialData.bounds.min + spatialData.bounds.max) / 2f);
            Vector3 size = ConvertToUnityCoords(spatialData.bounds.max - spatialData.bounds.min);
            Gizmos.DrawWireCube(transform.TransformPoint(center), size);
        }
    }
#endif
}

/// <summary>
/// 空间FPI点信息组件
/// </summary>
public class SpatialFPIPointInfo : MonoBehaviour
{
    private SpatialFPIPoint spatialPoint;
    private Renderer pointRenderer;
    private Color originalColor;

    public void Initialize(SpatialFPIPoint point)
    {
        spatialPoint = point;
        pointRenderer = GetComponent<Renderer>();
        if (pointRenderer != null)
        {
            originalColor = pointRenderer.material.color;
        }
    }

    void OnMouseEnter()
    {
        if (pointRenderer != null)
        {
            pointRenderer.material.color = Color.white;
        }
    }

    void OnMouseExit()
    {
        if (pointRenderer != null)
        {
            pointRenderer.material.color = originalColor;
        }
    }

    void OnMouseDown()
    {
        if (spatialPoint != null)
        {
            string info = $"空间插值点信息:\n" +
                         $"位置: ({spatialPoint.position.x:F2}, {spatialPoint.position.y:F2}, {spatialPoint.position.z:F2})\n" +
                         $"FPI值: {spatialPoint.fpi:F2}";
            
            Debug.Log(info);
        }
    }
} 