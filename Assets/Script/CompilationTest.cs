using UnityEngine;
using System.Collections.Generic;
using System.Linq;
// 使用全局别名避免命名冲突
using DrillingDataItem = DrillingData.DrillingData;
using TrajectoryPoint = DrillingData.TrajectoryPoint;

namespace DrillingData
{
    public class CompilationTest : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("编译测试开始...");
            // 测试DrillingData类型
            TestDrillingDataType();
            // 测试TrajectoryPoint类型
            TestTrajectoryPointType();
            // 测试数据处理器
            TestDataProcessor();
            Debug.Log("编译测试完成！");
        }
        private void TestDrillingDataType()
        {
            // 测试DrillingData类型创建
            DrillingDataItem testData = new DrillingDataItem();
            testData.originalIndex = 1;
            testData.torque = 100f;
            testData.drillPressure = 200f;
            testData.rotationSpeed = 50f;
            testData.temperature = 25f;
            testData.inclination = 15f;
            testData.azimuth = 90f;
            testData.gravitySum = 1.0f;
            testData.depth = 10f;
            testData.isMarked = false;
            Debug.Log($"DrillingData类型测试成功: 扭矩={testData.torque}, 钻压={testData.drillPressure}");
        }
        private void TestTrajectoryPointType()
        {
            // 测试TrajectoryPoint类型创建
            TrajectoryPoint testPoint = new TrajectoryPoint();
            testPoint.eastDisplacement = 1.5f;
            testPoint.northDisplacement = 2.0f;
            testPoint.verticalDepth = 15.0f;
            testPoint.inclination = 20f;
            testPoint.azimuth = 45f;
            testPoint.rodLength = 15.0f;
            Debug.Log($"TrajectoryPoint类型测试成功: 东向位移={testPoint.eastDisplacement}, 北向位移={testPoint.northDisplacement}, 深度={testPoint.verticalDepth}");
        }
        private void TestDataProcessor()
        {
            // 测试数据处理器创建
            UnityDrillingDataProcessor processor = gameObject.AddComponent<UnityDrillingDataProcessor>();
            // 测试获取轨迹点方法
            List<TrajectoryPoint> trajectoryPoints = processor.GetTrajectoryPoints();
            Debug.Log($"获取轨迹点成功: {trajectoryPoints.Count} 个点");
            // 测试获取钻井数据方法
            List<DrillingDataItem> drillingData = processor.GetDrillingData();
            Debug.Log($"获取钻井数据成功: {drillingData.Count} 个数据点");
            // 清理测试组件
            DestroyImmediate(processor);
        }
        [ContextMenu("运行编译测试")]
        public void RunCompilationTest()
        {
            Debug.Log("=== 编译测试报告 ===");
            // 测试类型创建
            try
            {
                DrillingDataItem testData = new DrillingDataItem();
                Debug.Log("✅ DrillingData类型创建成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ DrillingData类型创建失败: {e.Message}");
            }
            try
            {
                TrajectoryPoint testPoint = new TrajectoryPoint();
                Debug.Log("✅ TrajectoryPoint类型创建成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ TrajectoryPoint类型创建失败: {e.Message}");
            }
            try
            {
                List<DrillingDataItem> testList = new List<DrillingDataItem>();
                Debug.Log("✅ DrillingData列表创建成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ DrillingData列表创建失败: {e.Message}");
            }
            try
            {
                List<TrajectoryPoint> testList = new List<TrajectoryPoint>();
                Debug.Log("✅ TrajectoryPoint列表创建成功");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ TrajectoryPoint列表创建失败: {e.Message}");
            }
            Debug.Log("=== 编译测试报告完成 ===");
        }
    }
} 