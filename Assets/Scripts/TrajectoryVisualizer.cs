using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

/// <summary>
/// 钻孔轨迹点数据
/// </summary>
[Serializable]
public class TrajectoryPoint
{
    public float x;           // X坐标
    public float y;           // Y坐标
    public float z;           // Z坐标（深度）
    public float depth;       // 钻孔深度
    public float inclination; // 倾角
    public float azimuth;     // 方位角
    public float fpi;         // FPI贯入指数
    public float ucs;         // 煤岩强度
}

/// <summary>
/// 钻孔轨迹数据
/// </summary>
[Serializable]
public class TrajectoryData
{
    public List<TrajectoryPoint> points = new List<TrajectoryPoint>();
    public string title = "钻孔轨迹";
    public float totalLength;
    public float maxDepth;
    public Vector3 bounds;
}

/// <summary>
/// 钻孔轨迹可视化器
/// 专注于轨迹和周边情况的三维可视化
/// </summary>
public class TrajectoryVisualizer : MonoBehaviour
{
    [Header("数据源")]
    public string jsonFileName = "trajectory_data.json";
    public bool loadFromStreamingAssets = true;
    
    [Header("轨迹可视化设置")]
    public Material trajectoryLineMaterial;
    public Material trajectoryPointMaterial;
    public Color startColor = Color.green;
    public Color endColor = Color.red;
    public float lineWidth = 0.1f;
    public float pointSize = 0.2f;
    
    [Header("显示选项")]
    public bool showTrajectoryLine = true;
    public bool showTrajectoryPoints = true;
    public bool showDepthLabels = true;
    public bool showCoordinateAxis = true;
    
    [Header("坐标轴设置")]
    public float axisLength = 10f;
    public Material axisMaterial;
    
    // 私有变量
    private TrajectoryData trajectoryData;
    private LineRenderer lineRenderer;
    private GameObject pointsContainer;
    private GameObject axisContainer;
    private List<GameObject> pointObjects = new List<GameObject>();
    private List<GameObject> labelObjects = new List<GameObject>();

    void Start()
    {
        LoadTrajectoryData();
        SetupVisualization();
    }

    /// <summary>
    /// 加载轨迹数据
    /// </summary>
    public void LoadTrajectoryData()
    {
        string filePath;
        
        if (loadFromStreamingAssets)
        {
            filePath = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        }
        else
        {
            filePath = Path.Combine(Application.persistentDataPath, jsonFileName);
        }
        
        try
        {
            string jsonContent;
            
            // 处理不同平台的文件读取
            if (filePath.Contains("://") || filePath.Contains(":///"))
            {
                var www = new WWW(filePath);
                while (!www.isDone) { }
                jsonContent = www.text;
            }
            else
            {
                jsonContent = File.ReadAllText(filePath);
            }
            
            trajectoryData = JsonUtility.FromJson<TrajectoryData>(jsonContent);
            
            if (trajectoryData == null || trajectoryData.points == null || trajectoryData.points.Count == 0)
            {
                Debug.LogError("轨迹数据为空或格式错误，创建示例数据");
                CreateSampleData();
            }
            else
            {
                Debug.Log($"成功加载轨迹数据：{trajectoryData.points.Count} 个点");
                CalculateTrajectoryBounds();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"加载轨迹数据失败: {e.Message}，创建示例数据");
            CreateSampleData();
        }
    }

    /// <summary>
    /// 创建示例轨迹数据
    /// </summary>
    void CreateSampleData()
    {
        trajectoryData = new TrajectoryData();
        trajectoryData.title = "示例钻孔轨迹";
        
        // 创建一个弯曲的钻孔轨迹
        for (int i = 0; i <= 30; i++)
        {
            float t = i / 30f;
            float depth = t * 40f; // 40米深度
            
            // 轨迹逐渐偏向东南方向
            float angle = t * Mathf.PI * 0.5f; // 90度转向
            float deviation = t * 8f; // 最大偏移8米
            
            var point = new TrajectoryPoint
            {
                x = Mathf.Sin(angle) * deviation,
                y = Mathf.Cos(angle) * deviation,
                z = -depth,
                depth = depth,
                inclination = 10f + t * 20f, // 倾角从10度增加到30度
                azimuth = t * 45f, // 方位角从0度到45度
                fpi = 1000000f + UnityEngine.Random.Range(-200000f, 500000f),
                ucs = 3000000f + UnityEngine.Random.Range(-1000000f, 2000000f)
            };
            
            trajectoryData.points.Add(point);
        }
        
        CalculateTrajectoryBounds();
        Debug.Log("示例轨迹数据创建完成");
    }

    /// <summary>
    /// 计算轨迹边界和基本统计
    /// </summary>
    void CalculateTrajectoryBounds()
    {
        if (trajectoryData.points.Count == 0) return;
        
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        
        foreach (var point in trajectoryData.points)
        {
            Vector3 pos = new Vector3(point.x, point.y, point.z);
            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos);
        }
        
        trajectoryData.bounds = max - min;
        trajectoryData.maxDepth = trajectoryData.points[trajectoryData.points.Count - 1].depth;
        
        // 计算轨迹总长度
        float totalLength = 0f;
        for (int i = 1; i < trajectoryData.points.Count; i++)
        {
            Vector3 prev = new Vector3(trajectoryData.points[i-1].x, trajectoryData.points[i-1].y, trajectoryData.points[i-1].z);
            Vector3 curr = new Vector3(trajectoryData.points[i].x, trajectoryData.points[i].y, trajectoryData.points[i].z);
            totalLength += Vector3.Distance(prev, curr);
        }
        trajectoryData.totalLength = totalLength;
        
        Debug.Log($"轨迹统计 - 总长度: {totalLength:F2}m, 最大深度: {trajectoryData.maxDepth:F2}m, 边界: {trajectoryData.bounds}");
    }

    /// <summary>
    /// 设置轨迹可视化
    /// </summary>
    public void SetupVisualization()
    {
        if (trajectoryData == null || trajectoryData.points.Count == 0) return;
        
        // 清理现有对象
        ClearVisualization();
        
        // 创建容器
        pointsContainer = new GameObject("TrajectoryPoints");
        pointsContainer.transform.SetParent(transform);
        
        // 设置轨迹线
        if (showTrajectoryLine)
        {
            SetupTrajectoryLine();
        }
        
        // 设置轨迹点
        if (showTrajectoryPoints)
        {
            SetupTrajectoryPoints();
        }
        
        // 设置坐标轴
        if (showCoordinateAxis)
        {
            SetupCoordinateAxis();
        }
        
        // 设置深度标签
        if (showDepthLabels)
        {
            SetupDepthLabels();
        }
        
        // 居中显示
        CenterTrajectory();
        
        Debug.Log("轨迹可视化设置完成");
    }

    /// <summary>
    /// 设置轨迹线
    /// </summary>
    void SetupTrajectoryLine()
    {
        GameObject lineObject = new GameObject("TrajectoryLine");
        lineObject.transform.SetParent(transform);
        
        lineRenderer = lineObject.AddComponent<LineRenderer>();
        lineRenderer.material = trajectoryLineMaterial ? trajectoryLineMaterial : CreateDefaultLineMaterial();
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = trajectoryData.points.Count;
        lineRenderer.useWorldSpace = false;
        
        // 设置深度渐变色
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(startColor, 0.0f), new GradientColorKey(endColor, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) }
        );
        lineRenderer.colorGradient = gradient;
        
        // 设置轨迹点位置
        Vector3[] positions = new Vector3[trajectoryData.points.Count];
        for (int i = 0; i < trajectoryData.points.Count; i++)
        {
            var point = trajectoryData.points[i];
            positions[i] = new Vector3(point.x, point.z, point.y); // Unity坐标系转换
        }
        lineRenderer.SetPositions(positions);
    }

    /// <summary>
    /// 设置轨迹点
    /// </summary>
    void SetupTrajectoryPoints()
    {
        for (int i = 0; i < trajectoryData.points.Count; i++)
        {
            var point = trajectoryData.points[i];
            
            GameObject pointObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pointObj.name = $"TrajectoryPoint_{i}_{point.depth:F1}m";
            pointObj.transform.SetParent(pointsContainer.transform);
            pointObj.transform.localPosition = new Vector3(point.x, point.z, point.y);
            pointObj.transform.localScale = Vector3.one * pointSize;
            
            // 设置材质和颜色
            Renderer renderer = pointObj.GetComponent<Renderer>();
            if (trajectoryPointMaterial)
            {
                renderer.material = Instantiate(trajectoryPointMaterial);
            }
            
            // 根据深度设置颜色
            float t = i / (float)(trajectoryData.points.Count - 1);
            Color pointColor = Color.Lerp(startColor, endColor, t);
            renderer.material.color = pointColor;
            
            // 添加点击交互
            var pointInfo = pointObj.AddComponent<TrajectoryPointInfo>();
            pointInfo.point = point;
            pointInfo.index = i;
            
            pointObjects.Add(pointObj);
        }
    }

    /// <summary>
    /// 设置坐标轴
    /// </summary>
    void SetupCoordinateAxis()
    {
        axisContainer = new GameObject("CoordinateAxis");
        axisContainer.transform.SetParent(transform);
        
        // X轴 (东方向 - 红色)
        CreateAxis(Vector3.right * axisLength, Color.red, "X-Axis (East)");
        
        // Y轴 (深度方向 - 绿色)
        CreateAxis(Vector3.up * axisLength, Color.green, "Y-Axis (Depth)");
        
        // Z轴 (北方向 - 蓝色)
        CreateAxis(Vector3.forward * axisLength, Color.blue, "Z-Axis (North)");
    }

    /// <summary>
    /// 创建单个坐标轴
    /// </summary>
    void CreateAxis(Vector3 direction, Color color, string name)
    {
        GameObject axis = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        axis.name = name;
        axis.transform.SetParent(axisContainer.transform);
        
        // 设置位置和旋转
        axis.transform.localPosition = direction * 0.5f;
        axis.transform.localScale = new Vector3(0.05f, direction.magnitude * 0.5f, 0.05f);
        axis.transform.LookAt(axis.transform.position + direction);
        axis.transform.Rotate(90, 0, 0);
        
        // 设置颜色
        Renderer renderer = axis.GetComponent<Renderer>();
        Material mat = axisMaterial ? Instantiate(axisMaterial) : new Material(Shader.Find("Standard"));
        mat.color = color;
        renderer.material = mat;
        
        // 添加箭头
        GameObject arrowHead = GameObject.CreatePrimitive(PrimitiveType.Cone);
        arrowHead.name = name + "_Arrow";
        arrowHead.transform.SetParent(axis.transform);
        arrowHead.transform.localPosition = new Vector3(0, 1, 0);
        arrowHead.transform.localScale = new Vector3(2, 1, 2);
        arrowHead.GetComponent<Renderer>().material = mat;
    }

    /// <summary>
    /// 设置深度标签
    /// </summary>
    void SetupDepthLabels()
    {
        // 每隔几个点显示一个深度标签
        int labelInterval = Mathf.Max(1, trajectoryData.points.Count / 8); // 最多8个标签
        
        for (int i = 0; i < trajectoryData.points.Count; i += labelInterval)
        {
            var point = trajectoryData.points[i];
            
            GameObject labelObj = new GameObject($"DepthLabel_{point.depth:F1}m");
            labelObj.transform.SetParent(pointsContainer.transform);
            labelObj.transform.localPosition = new Vector3(point.x, point.z, point.y) + Vector3.up * 0.8f;
            
            // 添加文本网格
            TextMesh textMesh = labelObj.AddComponent<TextMesh>();
            textMesh.text = $"{point.depth:F0}m";
            textMesh.fontSize = 15;
            textMesh.color = Color.white;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            
            labelObjects.Add(labelObj);
        }
    }

    /// <summary>
    /// 居中轨迹显示
    /// </summary>
    void CenterTrajectory()
    {
        if (trajectoryData.points.Count == 0) return;
        
        // 计算轨迹中心点
        Vector3 center = Vector3.zero;
        foreach (var point in trajectoryData.points)
        {
            center += new Vector3(point.x, point.z, point.y);
        }
        center /= trajectoryData.points.Count;
        
        // 移动到原点
        transform.position = -center;
    }

    /// <summary>
    /// 清理可视化对象
    /// </summary>
    void ClearVisualization()
    {
        if (lineRenderer != null)
        {
            DestroyImmediate(lineRenderer.gameObject);
        }
        
        if (pointsContainer != null)
        {
            DestroyImmediate(pointsContainer);
        }
        
        if (axisContainer != null)
        {
            DestroyImmediate(axisContainer);
        }
        
        pointObjects.Clear();
        labelObjects.Clear();
    }

    /// <summary>
    /// 创建默认线条材质
    /// </summary>
    Material CreateDefaultLineMaterial()
    {
        Material mat = new Material(Shader.Find("Sprites/Default"));
        mat.color = Color.yellow;
        return mat;
    }

    /// <summary>
    /// 切换轨迹线显示
    /// </summary>
    public void ToggleTrajectoryLine()
    {
        showTrajectoryLine = !showTrajectoryLine;
        if (lineRenderer != null)
        {
            lineRenderer.gameObject.SetActive(showTrajectoryLine);
        }
    }

    /// <summary>
    /// 切换轨迹点显示
    /// </summary>
    public void ToggleTrajectoryPoints()
    {
        showTrajectoryPoints = !showTrajectoryPoints;
        if (pointsContainer != null)
        {
            foreach (var pointObj in pointObjects)
            {
                if (pointObj != null) pointObj.SetActive(showTrajectoryPoints);
            }
        }
    }

    /// <summary>
    /// 切换坐标轴显示
    /// </summary>
    public void ToggleCoordinateAxis()
    {
        showCoordinateAxis = !showCoordinateAxis;
        if (axisContainer != null)
        {
            axisContainer.SetActive(showCoordinateAxis);
        }
    }

    /// <summary>
    /// 切换深度标签显示
    /// </summary>
    public void ToggleDepthLabels()
    {
        showDepthLabels = !showDepthLabels;
        foreach (var label in labelObjects)
        {
            if (label != null) label.SetActive(showDepthLabels);
        }
    }

    /// <summary>
    /// 获取轨迹基本信息
    /// </summary>
    public string GetTrajectoryInfo()
    {
        if (trajectoryData == null) return "无轨迹数据";
        
        return $"轨迹信息:\n" +
               $"总点数: {trajectoryData.points.Count}\n" +
               $"总长度: {trajectoryData.totalLength:F2}m\n" +
               $"最大深度: {trajectoryData.maxDepth:F2}m\n" +
               $"轨迹范围: {trajectoryData.bounds.x:F1} × {trajectoryData.bounds.y:F1} × {trajectoryData.bounds.z:F1}m";
    }

#if UNITY_EDITOR
    /// <summary>
    /// 编辑器中的Gizmos绘制
    /// </summary>
    void OnDrawGizmos()
    {
        if (trajectoryData == null || trajectoryData.points.Count == 0) return;
        
        // 绘制轨迹线
        Gizmos.color = Color.yellow;
        for (int i = 0; i < trajectoryData.points.Count - 1; i++)
        {
            var p1 = trajectoryData.points[i];
            var p2 = trajectoryData.points[i + 1];
            
            Vector3 pos1 = transform.TransformPoint(new Vector3(p1.x, p1.z, p1.y));
            Vector3 pos2 = transform.TransformPoint(new Vector3(p2.x, p2.z, p2.y));
            
            Gizmos.DrawLine(pos1, pos2);
        }
        
        // 绘制起始点和终点
        if (trajectoryData.points.Count > 0)
        {
            var startPoint = trajectoryData.points[0];
            Gizmos.color = Color.green;
            Vector3 startPos = transform.TransformPoint(new Vector3(startPoint.x, startPoint.z, startPoint.y));
            Gizmos.DrawSphere(startPos, 0.5f);
            
            var endPoint = trajectoryData.points[trajectoryData.points.Count - 1];
            Gizmos.color = Color.red;
            Vector3 endPos = transform.TransformPoint(new Vector3(endPoint.x, endPoint.z, endPoint.y));
            Gizmos.DrawSphere(endPos, 0.5f);
        }
    }
#endif
}

/// <summary>
/// 轨迹点信息组件 - 处理点击交互
/// </summary>
public class TrajectoryPointInfo : MonoBehaviour
{
    public TrajectoryPoint point;
    public int index;
    
    void OnMouseDown()
    {
        string info = $"钻孔轨迹点 {index + 1}:\n" +
                     $"深度: {point.depth:F1}m\n" +
                     $"倾角: {point.inclination:F1}°\n" +
                     $"方位角: {point.azimuth:F1}°\n" +
                     $"坐标: ({point.x:F2}, {point.y:F2}, {point.z:F2})";
        
        if (!float.IsNaN(point.fpi) && point.fpi > 0)
        {
            info += $"\nFPI贯入指数: {point.fpi:F0}";
        }
        
        if (!float.IsNaN(point.ucs) && point.ucs > 0)
        {
            info += $"\nUCS煤岩强度: {point.ucs:F0} MPa";
        }
        
        Debug.Log(info);
    }
} 