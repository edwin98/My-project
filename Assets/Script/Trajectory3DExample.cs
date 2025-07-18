using UnityEngine;
using System.Collections.Generic;
using DrillingData;

namespace DrillingData
{
    /// <summary>
    /// 三维轨迹可视化使用示例
    /// </summary>
    public class Trajectory3DExample : MonoBehaviour
    {
        [Header("示例设置")]
        public bool createExampleOnStart = true;
        public ExampleType exampleType = ExampleType.Spiral;
        
        [Header("示例参数")]
        public int pointCount = 50;
        public float spiralRadius = 5f;
        public float spiralHeight = 20f;
        public float spiralTurns = 3f;
        
        [Header("组件引用")]
        public Trajectory3DVisualizer visualizer;
        
        public enum ExampleType
        {
            Spiral,     // 螺旋轨迹
            Straight,   // 直线轨迹
            Curve,      // 曲线轨迹
            Random      // 随机轨迹
        }
        
        void Start()
        {
            if (createExampleOnStart)
            {
                CreateExample();
            }
        }
        
        /// <summary>
        /// 创建示例
        /// </summary>
        [ContextMenu("创建示例")]
        public void CreateExample()
        {
            Debug.Log($"创建 {exampleType} 轨迹示例...");
            
            // 查找或创建可视化器
            if (visualizer == null)
            {
                visualizer = FindObjectOfType<Trajectory3DVisualizer>();
                if (visualizer == null)
                {
                    GameObject vizObject = new GameObject("ExampleTrajectory3DVisualizer");
                    visualizer = vizObject.AddComponent<Trajectory3DVisualizer>();
                    visualizer.targetCamera = Camera.main;
                }
            }
            
            // 创建示例数据
            List<TrajectoryPoint> exampleData = CreateExampleData();
            
            // 设置可视化
            visualizer.SetTrajectoryData(exampleData);
            
            Debug.Log("示例创建完成！");
        }
        
        /// <summary>
        /// 创建示例数据
        /// </summary>
        private List<TrajectoryPoint> CreateExampleData()
        {
            List<TrajectoryPoint> points = new List<TrajectoryPoint>();
            
            switch (exampleType)
            {
                case ExampleType.Spiral:
                    points = CreateSpiralTrajectory();
                    break;
                case ExampleType.Straight:
                    points = CreateStraightTrajectory();
                    break;
                case ExampleType.Curve:
                    points = CreateCurveTrajectory();
                    break;
                case ExampleType.Random:
                    points = CreateRandomTrajectory();
                    break;
            }
            
            return points;
        }
        
        /// <summary>
        /// 创建螺旋轨迹
        /// </summary>
        private List<TrajectoryPoint> CreateSpiralTrajectory()
        {
            List<TrajectoryPoint> points = new List<TrajectoryPoint>();
            
            for (int i = 0; i < pointCount; i++)
            {
                TrajectoryPoint point = new TrajectoryPoint();
                
                float t = (float)i / (pointCount - 1);
                float angle = t * spiralTurns * 2f * Mathf.PI;
                float radius = spiralRadius * (1f - t * 0.3f); // 半径逐渐减小
                float height = t * spiralHeight;
                
                point.eastDisplacement = radius * Mathf.Cos(angle);
                point.northDisplacement = radius * Mathf.Sin(angle);
                point.verticalDepth = height;
                
                // 计算倾角和方位角
                point.inclination = 15f + t * 30f; // 倾角从15度增加到45度
                point.azimuth = t * 360f; // 方位角从0度增加到360度
                
                // 其他参数
                point.rodLength = height;
                point.gravitySum = 1.0f;
                point.trueAzimuth = point.azimuth;
                point.avgInclination = point.inclination;
                point.avgMagneticAzimuth = point.azimuth;
                point.xCoordinate = i * 0.5f;
                point.lateralDeviation = radius;
                point.hValue = i * 0.1f;
                point.markTimestamp = System.DateTime.Now.AddMinutes(i);
                
                points.Add(point);
            }
            
            return points;
        }
        
        /// <summary>
        /// 创建直线轨迹
        /// </summary>
        private List<TrajectoryPoint> CreateStraightTrajectory()
        {
            List<TrajectoryPoint> points = new List<TrajectoryPoint>();
            
            for (int i = 0; i < pointCount; i++)
            {
                TrajectoryPoint point = new TrajectoryPoint();
                
                float t = (float)i / (pointCount - 1);
                float depth = t * spiralHeight;
                
                // 直线向下，稍微偏斜
                point.eastDisplacement = t * 2f; // 向东偏移
                point.northDisplacement = t * 1f; // 向北偏移
                point.verticalDepth = depth;
                
                // 固定的倾角和方位角
                point.inclination = 20f;
                point.azimuth = 45f;
                
                // 其他参数
                point.rodLength = depth;
                point.gravitySum = 1.0f;
                point.trueAzimuth = point.azimuth;
                point.avgInclination = point.inclination;
                point.avgMagneticAzimuth = point.azimuth;
                point.xCoordinate = i * 0.5f;
                point.lateralDeviation = Mathf.Sqrt(point.eastDisplacement * point.eastDisplacement + point.northDisplacement * point.northDisplacement);
                point.hValue = i * 0.1f;
                point.markTimestamp = System.DateTime.Now.AddMinutes(i);
                
                points.Add(point);
            }
            
            return points;
        }
        
        /// <summary>
        /// 创建曲线轨迹
        /// </summary>
        private List<TrajectoryPoint> CreateCurveTrajectory()
        {
            List<TrajectoryPoint> points = new List<TrajectoryPoint>();
            
            for (int i = 0; i < pointCount; i++)
            {
                TrajectoryPoint point = new TrajectoryPoint();
                
                float t = (float)i / (pointCount - 1);
                float depth = t * spiralHeight;
                
                // S形曲线
                float curveX = Mathf.Sin(t * 4f * Mathf.PI) * spiralRadius * 0.5f;
                float curveZ = Mathf.Cos(t * 2f * Mathf.PI) * spiralRadius * 0.3f;
                
                point.eastDisplacement = curveX;
                point.northDisplacement = curveZ;
                point.verticalDepth = depth;
                
                // 变化的倾角和方位角
                point.inclination = 10f + Mathf.Sin(t * Mathf.PI) * 20f;
                point.azimuth = 90f + Mathf.Sin(t * 2f * Mathf.PI) * 45f;
                
                // 其他参数
                point.rodLength = depth;
                point.gravitySum = 1.0f;
                point.trueAzimuth = point.azimuth;
                point.avgInclination = point.inclination;
                point.avgMagneticAzimuth = point.azimuth;
                point.xCoordinate = i * 0.5f;
                point.lateralDeviation = Mathf.Sqrt(point.eastDisplacement * point.eastDisplacement + point.northDisplacement * point.northDisplacement);
                point.hValue = i * 0.1f;
                point.markTimestamp = System.DateTime.Now.AddMinutes(i);
                
                points.Add(point);
            }
            
            return points;
        }
        
        /// <summary>
        /// 创建随机轨迹
        /// </summary>
        private List<TrajectoryPoint> CreateRandomTrajectory()
        {
            List<TrajectoryPoint> points = new List<TrajectoryPoint>();
            
            Vector3 currentPos = Vector3.zero;
            
            for (int i = 0; i < pointCount; i++)
            {
                TrajectoryPoint point = new TrajectoryPoint();
                
                // 随机移动
                Vector3 randomOffset = new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(1f, 3f), // 向下移动
                    Random.Range(-1f, 1f)
                );
                
                currentPos += randomOffset;
                
                point.eastDisplacement = currentPos.x;
                point.northDisplacement = currentPos.z;
                point.verticalDepth = -currentPos.y;
                
                // 随机倾角和方位角
                point.inclination = Random.Range(0f, 60f);
                point.azimuth = Random.Range(0f, 360f);
                
                // 其他参数
                point.rodLength = -currentPos.y;
                point.gravitySum = Random.Range(0.98f, 1.02f);
                point.trueAzimuth = point.azimuth;
                point.avgInclination = point.inclination;
                point.avgMagneticAzimuth = point.azimuth;
                point.xCoordinate = i * 0.5f;
                point.lateralDeviation = Mathf.Sqrt(point.eastDisplacement * point.eastDisplacement + point.northDisplacement * point.northDisplacement);
                point.hValue = i * 0.1f;
                point.markTimestamp = System.DateTime.Now.AddMinutes(i);
                
                points.Add(point);
            }
            
            return points;
        }
        
        /// <summary>
        /// 演示交互功能
        /// </summary>
        [ContextMenu("演示交互功能")]
        public void DemoInteraction()
        {
            if (visualizer == null)
            {
                Debug.LogWarning("可视化器未找到，请先创建示例");
                return;
            }
            
            Debug.Log("开始演示交互功能...");
            
            // 演示自动旋转
            StartCoroutine(DemoAutoRotation());
            
            // 演示颜色方案切换
            StartCoroutine(DemoColorSchemes());
        }
        
        /// <summary>
        /// 演示自动旋转
        /// </summary>
        private System.Collections.IEnumerator DemoAutoRotation()
        {
            Debug.Log("演示自动旋转...");
            
            visualizer.autoRotate = true;
            visualizer.autoRotateSpeed = 30f;
            
            yield return new WaitForSeconds(5f);
            
            visualizer.autoRotate = false;
            Debug.Log("自动旋转演示完成");
        }
        
        /// <summary>
        /// 演示颜色方案
        /// </summary>
        private System.Collections.IEnumerator DemoColorSchemes()
        {
            Debug.Log("演示颜色方案...");
            
            // 深度着色
            visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Depth;
            visualizer.UpdateFromProcessor();
            yield return new WaitForSeconds(2f);
            
            // 倾角着色
            visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Inclination;
            visualizer.UpdateFromProcessor();
            yield return new WaitForSeconds(2f);
            
            // 方位角着色
            visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Azimuth;
            visualizer.UpdateFromProcessor();
            yield return new WaitForSeconds(2f);
            
            // 恢复默认
            visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Depth;
            visualizer.UpdateFromProcessor();
            
            Debug.Log("颜色方案演示完成");
        }
        
        /// <summary>
        /// 切换示例类型
        /// </summary>
        [ContextMenu("切换示例类型")]
        public void SwitchExampleType()
        {
            // 循环切换示例类型
            int currentIndex = (int)exampleType;
            int nextIndex = (currentIndex + 1) % System.Enum.GetValues(typeof(ExampleType)).Length;
            exampleType = (ExampleType)nextIndex;
            
            Debug.Log($"切换到示例类型: {exampleType}");
            
            // 重新创建示例
            CreateExample();
        }
        
        /// <summary>
        /// 重置示例
        /// </summary>
        [ContextMenu("重置示例")]
        public void ResetExample()
        {
            if (visualizer != null)
            {
                visualizer.ResetView();
                visualizer.autoRotate = false;
                visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Depth;
                visualizer.UpdateFromProcessor();
            }
            
            Debug.Log("示例已重置");
        }
        
        /// <summary>
        /// 显示示例信息
        /// </summary>
        [ContextMenu("显示示例信息")]
        public void ShowExampleInfo()
        {
            Debug.Log("=== 三维轨迹可视化示例信息 ===");
            Debug.Log($"示例类型: {exampleType}");
            Debug.Log($"轨迹点数量: {pointCount}");
            Debug.Log($"螺旋半径: {spiralRadius}");
            Debug.Log($"螺旋高度: {spiralHeight}");
            Debug.Log($"螺旋圈数: {spiralTurns}");
            
            if (visualizer != null)
            {
                Debug.Log($"可视化器状态: 已创建");
                Debug.Log($"轨迹线显示: {visualizer.showTrajectoryLine}");
                Debug.Log($"轨迹点显示: {visualizer.showTrajectoryPoints}");
                Debug.Log($"颜色方案: {visualizer.colorScheme}");
            }
            else
            {
                Debug.Log("可视化器状态: 未创建");
            }
            
            Debug.Log("=== 示例信息显示完成 ===");
        }
    }
} 