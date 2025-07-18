using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using DrillingData;
using DrillingDataItem = DrillingData.DrillingData;
using TrajectoryPoint = DrillingData.TrajectoryPoint;

namespace DrillingData
{
    /// <summary>
    /// 三维轨迹可视化测试脚本
    /// </summary>
    public class Trajectory3DTest : MonoBehaviour
    {
        [Header("测试设置")]
        public bool runTestOnStart = true;
        public bool createTestData = true;
        public int testPointCount = 20;
        
        [Header("组件引用")]
        public Trajectory3DVisualizer visualizer;
        public UnityDrillingDataProcessor dataProcessor;
        
        void Start()
        {
            if (runTestOnStart)
            {
                RunTest();
            }
        }
        
        /// <summary>
        /// 运行测试
        /// </summary>
        [ContextMenu("运行测试")]
        public void RunTest()
        {
            Debug.Log("开始三维轨迹可视化测试...");
            
            // 查找或创建组件
            FindOrCreateComponents();
            
            // 创建测试数据
            if (createTestData)
            {
                CreateTestTrajectoryData();
            }
            
            // 测试可视化
            TestVisualization();
            
            Debug.Log("测试完成！");
        }
        
        /// <summary>
        /// 查找或创建组件
        /// </summary>
        private void FindOrCreateComponents()
        {
            // 查找可视化器
            if (visualizer == null)
            {
                visualizer = FindObjectOfType<Trajectory3DVisualizer>();
                if (visualizer == null)
                {
                    GameObject vizObject = new GameObject("TestTrajectory3DVisualizer");
                    visualizer = vizObject.AddComponent<Trajectory3DVisualizer>();
                }
            }
            
            // 查找数据处理器
            if (dataProcessor == null)
            {
                dataProcessor = FindObjectOfType<UnityDrillingDataProcessor>();
                if (dataProcessor == null)
                {
                    GameObject procObject = new GameObject("TestDrillingDataProcessor");
                    dataProcessor = procObject.AddComponent<UnityDrillingDataProcessor>();
                }
            }
            
            // 设置引用
            visualizer.dataProcessor = dataProcessor;
            if (visualizer.targetCamera == null)
            {
                visualizer.targetCamera = Camera.main;
            }
        }
        
        /// <summary>
        /// 创建测试轨迹数据
        /// </summary>
        private void CreateTestTrajectoryData()
        {
            Debug.Log($"创建 {testPointCount} 个测试轨迹点...");
            
            List<TrajectoryPoint> testPoints = new List<TrajectoryPoint>();
            
            for (int i = 0; i < testPointCount; i++)
            {
                TrajectoryPoint point = new TrajectoryPoint();
                
                // 模拟螺旋轨迹
                float angle = i * 30f * Mathf.Deg2Rad; // 每点旋转30度
                float radius = 2f + i * 0.1f; // 逐渐增大半径
                float depth = i * 2f; // 每点深度增加2米
                
                point.eastDisplacement = radius * Mathf.Cos(angle);
                point.northDisplacement = radius * Mathf.Sin(angle);
                point.verticalDepth = depth;
                
                // 其他参数
                point.rodLength = depth;
                point.inclination = 15f + i * 2f; // 倾角逐渐增加
                point.azimuth = i * 30f; // 方位角每点增加30度
                point.gravitySum = 1.0f;
                point.trueAzimuth = point.azimuth;
                point.avgInclination = point.inclination;
                point.avgMagneticAzimuth = point.azimuth;
                point.xCoordinate = i * 0.5f;
                point.lateralDeviation = radius;
                point.hValue = i * 0.1f;
                point.markTimestamp = System.DateTime.Now.AddMinutes(i);
                
                testPoints.Add(point);
            }
            
            // 设置测试数据
            visualizer.SetTrajectoryData(testPoints);
            
            Debug.Log($"测试数据创建完成: {testPoints.Count} 个轨迹点");
        }
        
        /// <summary>
        /// 测试可视化功能
        /// </summary>
        private void TestVisualization()
        {
            Debug.Log("测试可视化功能...");
            
            // 测试基本显示
            TestBasicDisplay();
            
            // 测试交互功能
            TestInteraction();
            
            // 测试颜色方案
            TestColorSchemes();
            
            // 测试性能
            TestPerformance();
        }
        
        /// <summary>
        /// 测试基本显示
        /// </summary>
        private void TestBasicDisplay()
        {
            Debug.Log("测试基本显示功能...");
            
            // 测试轨迹线显示
            visualizer.showTrajectoryLine = true;
            visualizer.showTrajectoryPoints = true;
            visualizer.showCoordinateAxis = true;
            visualizer.showDepthLabels = true;
            
            visualizer.UpdateFromProcessor();
            
            Debug.Log("基本显示测试完成");
        }
        
        /// <summary>
        /// 测试交互功能
        /// </summary>
        private void TestInteraction()
        {
            Debug.Log("测试交互功能...");
            
            // 测试相机控制
            if (visualizer.targetCamera != null)
            {
                // 重置视角
                visualizer.ResetView();
                Debug.Log("视角重置测试完成");
                
                // 测试自动旋转
                visualizer.autoRotate = true;
                visualizer.autoRotateSpeed = 30f;
                Debug.Log("自动旋转测试完成");
            }
            
            Debug.Log("交互功能测试完成");
        }
        
        /// <summary>
        /// 测试颜色方案
        /// </summary>
        private void TestColorSchemes()
        {
            Debug.Log("测试颜色方案...");
            
            // 测试深度着色
            visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Depth;
            visualizer.UpdateFromProcessor();
            Debug.Log("深度着色测试完成");
            
            // 测试倾角着色
            visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Inclination;
            visualizer.UpdateFromProcessor();
            Debug.Log("倾角着色测试完成");
            
            // 测试方位角着色
            visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Azimuth;
            visualizer.UpdateFromProcessor();
            Debug.Log("方位角着色测试完成");
            
            // 恢复默认
            visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Depth;
            visualizer.UpdateFromProcessor();
            
            Debug.Log("颜色方案测试完成");
        }
        
        /// <summary>
        /// 测试性能
        /// </summary>
        private void TestPerformance()
        {
            Debug.Log("测试性能...");
            
            // 测试大数据集
            int largePointCount = 1000;
            Debug.Log($"创建 {largePointCount} 个点进行性能测试...");
            
            List<TrajectoryPoint> largeDataSet = new List<TrajectoryPoint>();
            for (int i = 0; i < largePointCount; i++)
            {
                TrajectoryPoint point = new TrajectoryPoint();
                point.eastDisplacement = Random.Range(-10f, 10f);
                point.northDisplacement = Random.Range(-10f, 10f);
                point.verticalDepth = i * 0.1f;
                point.inclination = Random.Range(0f, 90f);
                point.azimuth = Random.Range(0f, 360f);
                point.rodLength = point.verticalDepth;
                largeDataSet.Add(point);
            }
            
            // 性能测试
            System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
            
            visualizer.SetTrajectoryData(largeDataSet);
            
            stopwatch.Stop();
            Debug.Log($"大数据集可视化耗时: {stopwatch.ElapsedMilliseconds}ms");
            
            // 恢复测试数据
            CreateTestTrajectoryData();
            
            Debug.Log("性能测试完成");
        }
        
        /// <summary>
        /// 测试数据处理器集成
        /// </summary>
        [ContextMenu("测试数据处理器集成")]
        public void TestDataProcessorIntegration()
        {
            Debug.Log("测试数据处理器集成...");
            
            if (dataProcessor == null)
            {
                Debug.LogError("数据处理器未找到！");
                return;
            }
            
            // 测试获取轨迹点
            List<TrajectoryPoint> points = dataProcessor.GetTrajectoryPoints();
            Debug.Log($"从数据处理器获取到 {points.Count} 个轨迹点");
            
            // 测试获取钻井数据
            List<DrillingDataItem> drillingData = dataProcessor.GetDrillingData();
            Debug.Log($"从数据处理器获取到 {drillingData.Count} 个钻井数据点");
            
            // 测试处理报告
            string report = dataProcessor.GetProcessingReport();
            Debug.Log($"处理报告: {report}");
            
            Debug.Log("数据处理器集成测试完成");
        }
        
        /// <summary>
        /// 测试UI控制器
        /// </summary>
        [ContextMenu("测试UI控制器")]
        public void TestUIController()
        {
            Debug.Log("测试UI控制器...");
            
            Trajectory3DController controller = FindObjectOfType<Trajectory3DController>();
            if (controller == null)
            {
                Debug.Log("UI控制器未找到，创建测试控制器...");
                
                GameObject controllerObject = new GameObject("TestTrajectory3DController");
                controller = controllerObject.AddComponent<Trajectory3DController>();
                controller.SetDataProcessor(dataProcessor);
                controller.SetVisualizer(visualizer);
                controller.SetTargetCamera(Camera.main);
            }
            
            // 测试UI更新
            controller.UpdateVisualization();
            Debug.Log("UI控制器测试完成");
        }
        
        /// <summary>
        /// 清理测试
        /// </summary>
        [ContextMenu("清理测试")]
        public void CleanupTest()
        {
            Debug.Log("清理测试环境...");
            
            // 停止自动旋转
            if (visualizer != null)
            {
                visualizer.autoRotate = false;
            }
            
            // 重置视角
            if (visualizer != null)
            {
                visualizer.ResetView();
            }
            
            Debug.Log("测试环境清理完成");
        }
        
        /// <summary>
        /// 生成测试报告
        /// </summary>
        [ContextMenu("生成测试报告")]
        public void GenerateTestReport()
        {
            Debug.Log("=== 三维轨迹可视化测试报告 ===");
            
            // 组件状态
            Debug.Log($"可视化器状态: {(visualizer != null ? "已创建" : "未创建")}");
            Debug.Log($"数据处理器状态: {(dataProcessor != null ? "已创建" : "未创建")}");
            Debug.Log($"相机状态: {(Camera.main != null ? "已找到" : "未找到")}");
            
            // 数据状态
            if (dataProcessor != null)
            {
                List<TrajectoryPoint> points = dataProcessor.GetTrajectoryPoints();
                Debug.Log($"轨迹点数量: {points.Count}");
                
                if (points.Count > 0)
                {
                    float minDepth = points[0].verticalDepth;
                    float maxDepth = points[0].verticalDepth;
                    float minX = points[0].eastDisplacement;
                    float maxX = points[0].eastDisplacement;
                    float minZ = points[0].northDisplacement;
                    float maxZ = points[0].northDisplacement;
                    
                    foreach (var point in points)
                    {
                        if (point.verticalDepth < minDepth) minDepth = point.verticalDepth;
                        if (point.verticalDepth > maxDepth) maxDepth = point.verticalDepth;
                        if (point.eastDisplacement < minX) minX = point.eastDisplacement;
                        if (point.eastDisplacement > maxX) maxX = point.eastDisplacement;
                        if (point.northDisplacement < minZ) minZ = point.northDisplacement;
                        if (point.northDisplacement > maxZ) maxZ = point.northDisplacement;
                    }
                    
                    Debug.Log($"深度范围: {minDepth:F2}m - {maxDepth:F2}m");
                    Debug.Log($"X轴范围: {minX:F2}m - {maxX:F2}m");
                    Debug.Log($"Z轴范围: {minZ:F2}m - {maxZ:F2}m");
                }
            }
            
            // 可视化设置
            if (visualizer != null)
            {
                Debug.Log($"轨迹线显示: {visualizer.showTrajectoryLine}");
                Debug.Log($"轨迹点显示: {visualizer.showTrajectoryPoints}");
                Debug.Log($"坐标轴显示: {visualizer.showCoordinateAxis}");
                Debug.Log($"深度标签显示: {visualizer.showDepthLabels}");
                Debug.Log($"交互控制启用: {visualizer.enableInteractiveControls}");
                Debug.Log($"自动旋转: {visualizer.autoRotate}");
                Debug.Log($"颜色方案: {visualizer.colorScheme}");
            }
            
            Debug.Log("=== 测试报告生成完成 ===");
        }
    }
} 