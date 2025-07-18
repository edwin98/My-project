using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DrillingDataProcessor.Core;
using DrillingDataProcessor.Core.Modules;
using DrillingDataProcessor.Models;

namespace DrillingDataProcessor
{
    /// <summary>
    /// 空间FPI插值使用示例
    /// </summary>
    public class SpatialInterpolationExample
    {
        private readonly DrillingDataProcessorCore processor;
        private readonly SpatialFPIInterpolator spatialInterpolator;

        public SpatialInterpolationExample()
        {
            processor = new DrillingDataProcessorCore();
            spatialInterpolator = new SpatialFPIInterpolator();
            
            // 配置空间插值参数
            spatialInterpolator.SpatialResolution = 0.5f;     // 0.5米分辨率
            spatialInterpolator.InfluenceRadius = 5.0f;       // 5米影响半径
            spatialInterpolator.MinNeighborPoints = 3;        // 最少3个邻近点
            spatialInterpolator.BoundaryExpansion = 2.0f;     // 边界扩展2米
            
            // 订阅日志事件
            spatialInterpolator.OnLogMessage += Console.WriteLine;
        }

        /// <summary>
        /// 执行完整的数据处理和空间插值流程
        /// </summary>
        public void RunCompleteWorkflow(string excelPath, string outputDir)
        {
            try
            {
                Console.WriteLine("开始执行完整的数据处理和空间插值流程...");
                
                // 1. 配置数据处理器
                ConfigureProcessor(excelPath, outputDir);
                
                // 2. 执行基础数据处理
                Console.WriteLine("步骤1: 执行基础数据处理...");
                if (!processor.ProcessDrillingDataAndGenerateCharts())
                {
                    throw new Exception("数据处理失败");
                }
                
                // 3. 获取处理结果
                Console.WriteLine("步骤2: 获取处理结果...");
                var trajectoryPoints = GetTrajectoryPoints();
                var fpiData = GetFPIData();
                
                if (trajectoryPoints.Count == 0)
                {
                    throw new Exception("没有轨迹点数据");
                }
                
                if (fpiData.Count == 0)
                {
                    throw new Exception("没有FPI数据");
                }
                
                Console.WriteLine($"找到 {trajectoryPoints.Count} 个轨迹点和 {fpiData.Count} 个FPI数据点");
                
                // 4. 执行空间插值
                Console.WriteLine("步骤3: 执行空间FPI插值...");
                var interpolationResult = spatialInterpolator.InterpolateFPI(trajectoryPoints, fpiData);
                
                // 5. 保存插值结果
                Console.WriteLine("步骤4: 保存插值结果...");
                SaveInterpolationResults(interpolationResult, outputDir);
                
                Console.WriteLine("完整流程执行成功!");
                
                // 6. 打印结果摘要
                PrintResultSummary(interpolationResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"执行失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 仅执行空间插值（假设已有处理好的数据）
        /// </summary>
        public void RunInterpolationOnly(string trajectoryDataPath, string fpiDataPath, string outputDir)
        {
            try
            {
                Console.WriteLine("开始执行空间FPI插值...");
                
                // 1. 加载轨迹数据
                var trajectoryPoints = LoadTrajectoryData(trajectoryDataPath);
                Console.WriteLine($"加载了 {trajectoryPoints.Count} 个轨迹点");
                
                // 2. 加载FPI数据
                var fpiData = LoadFPIData(fpiDataPath);
                Console.WriteLine($"加载了 {fpiData.Count} 个FPI数据点");
                
                // 3. 执行插值
                var interpolationResult = spatialInterpolator.InterpolateFPI(trajectoryPoints, fpiData);
                
                // 4. 保存结果
                SaveInterpolationResults(interpolationResult, outputDir);
                
                Console.WriteLine("空间插值完成!");
                PrintResultSummary(interpolationResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"插值失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 配置数据处理器
        /// </summary>
        private void ConfigureProcessor(string excelPath, string outputDir)
        {
            processor.InputExcelPath = excelPath;
            processor.OutputPath = outputDir;
            processor.JsonOutputPath = Path.Combine(outputDir, "JSON");
            processor.Visualization3DOutputPath = Path.Combine(outputDir, "3D");
            
            // 确保输出目录存在
            Directory.CreateDirectory(outputDir);
            Directory.CreateDirectory(processor.JsonOutputPath);
            Directory.CreateDirectory(processor.Visualization3DOutputPath);
        }

        /// <summary>
        /// 获取轨迹点数据（从处理器或文件）
        /// </summary>
        private List<TrajectoryPoint> GetTrajectoryPoints()
        {
            // 尝试从处理器获取轨迹点数据
            // 如果处理器不支持直接获取，则从输出文件读取
            var trajectoryJsonPath = Path.Combine(processor.Visualization3DOutputPath, "trajectory_data.json");
            
            if (File.Exists(trajectoryJsonPath))
            {
                return LoadTrajectoryData(trajectoryJsonPath);
            }
            
            throw new FileNotFoundException($"轨迹数据文件不存在: {trajectoryJsonPath}");
        }

        /// <summary>
        /// 获取FPI数据（从处理器或文件）
        /// </summary>
        private Dictionary<float, float> GetFPIData()
        {
            var fpiJsonPath = Path.Combine(processor.JsonOutputPath, "fpi_analysis.json");
            
            if (File.Exists(fpiJsonPath))
            {
                return LoadFPIData(fpiJsonPath);
            }
            
            throw new FileNotFoundException($"FPI数据文件不存在: {fpiJsonPath}");
        }

        /// <summary>
        /// 加载轨迹数据
        /// </summary>
        private List<TrajectoryPoint> LoadTrajectoryData(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                
                var trajectoryPoints = new List<TrajectoryPoint>();
                
                foreach (var point in data.points)
                {
                    trajectoryPoints.Add(new TrajectoryPoint
                    {
                        EastDisplacement = (float)point.x,
                        NorthDisplacement = (float)point.y,
                        VerticalDepth = Math.Abs((float)point.z), // 确保是正值
                        RodLength = (float)point.depth,
                        Inclination = (float)point.inclination,
                        Azimuth = (float)point.azimuth,
                        MarkTimestamp = DateTime.Parse((string)point.markTimestamp)
                    });
                }
                
                return trajectoryPoints;
            }
            catch (Exception ex)
            {
                throw new Exception($"加载轨迹数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 加载FPI数据
        /// </summary>
        private Dictionary<float, float> LoadFPIData(string filePath)
        {
            try
            {
                var json = File.ReadAllText(filePath);
                dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                
                var fpiData = new Dictionary<float, float>();
                
                foreach (var series in data.Series)
                {
                    if (series.Name == "FPI贯入指数")
                    {
                        foreach (var dataPoint in series.Data)
                        {
                            string key = dataPoint.Key;
                            float depth = float.Parse(key.Replace("m", ""));
                            float fpiValue = (float)dataPoint.Value;
                            
                            fpiData[depth] = fpiValue;
                        }
                        break;
                    }
                }
                
                return fpiData;
            }
            catch (Exception ex)
            {
                throw new Exception($"加载FPI数据失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存插值结果
        /// </summary>
        private void SaveInterpolationResults(SpatialFPIInterpolator.InterpolationResult result, string outputDir)
        {
            var interpolationDir = Path.Combine(outputDir, "SpatialInterpolation");
            Directory.CreateDirectory(interpolationDir);
            
            // 保存详细插值结果
            var detailResultPath = Path.Combine(interpolationDir, "spatial_fpi_interpolation_detailed.json");
            spatialInterpolator.SaveInterpolationResult(result, detailResultPath);
            
            // 保存可视化格式
            var visualizationPath = Path.Combine(interpolationDir, "spatial_fpi_visualization.json");
            spatialInterpolator.ExportFor3DVisualization(result, visualizationPath);
            
            // 保存摘要信息
            var summaryPath = Path.Combine(interpolationDir, "interpolation_summary.txt");
            SaveSummaryReport(result, summaryPath);
        }

        /// <summary>
        /// 保存摘要报告
        /// </summary>
        private void SaveSummaryReport(SpatialFPIInterpolator.InterpolationResult result, string filePath)
        {
            var report = $@"空间FPI插值结果摘要
===================

处理时间: {result.ProcessedTime:yyyy-MM-dd HH:mm:ss}

空间配置:
- 分辨率: {spatialInterpolator.SpatialResolution}m
- 影响半径: {spatialInterpolator.InfluenceRadius}m
- 最小邻近点: {spatialInterpolator.MinNeighborPoints}
- 边界扩展: {spatialInterpolator.BoundaryExpansion}m

空间范围:
- X: [{result.Space.MinBounds.X:F2}, {result.Space.MaxBounds.X:F2}]
- Y: [{result.Space.MinBounds.Y:F2}, {result.Space.MaxBounds.Y:F2}]
- Z: [{result.Space.MinBounds.Z:F2}, {result.Space.MaxBounds.Z:F2}]

网格信息:
- 网格维度: {result.Space.GetGridDimensions()}
- 总网格点: {result.Space.GridPoints.Count}
- 有效插值点: {result.ValidPointsCount}

FPI统计:
- 最小值: {result.MinFPI:F2}
- 最大值: {result.MaxFPI:F2}
- 平均值: {result.AvgFPI:F2}

覆盖率: {(float)result.ValidPointsCount / result.Space.GridPoints.Count * 100:F1}%
";

            File.WriteAllText(filePath, report);
        }

        /// <summary>
        /// 打印结果摘要
        /// </summary>
        private void PrintResultSummary(SpatialFPIInterpolator.InterpolationResult result)
        {
            Console.WriteLine("\n=== 空间插值结果摘要 ===");
            Console.WriteLine($"总网格点: {result.Space.GridPoints.Count}");
            Console.WriteLine($"有效插值点: {result.ValidPointsCount}");
            Console.WriteLine($"覆盖率: {(float)result.ValidPointsCount / result.Space.GridPoints.Count * 100:F1}%");
            Console.WriteLine($"FPI范围: [{result.MinFPI:F2}, {result.MaxFPI:F2}]");
            Console.WriteLine($"FPI平均值: {result.AvgFPI:F2}");
            Console.WriteLine($"网格维度: {result.Space.GetGridDimensions()}");
            Console.WriteLine("========================\n");
        }

        /// <summary>
        /// 自定义插值参数的示例
        /// </summary>
        public void RunWithCustomParameters(string excelPath, string outputDir, 
            float resolution = 0.3f, float influenceRadius = 3.0f, int minNeighbors = 2)
        {
            Console.WriteLine("使用自定义参数执行空间插值...");
            
            // 更新插值参数
            spatialInterpolator.SpatialResolution = resolution;
            spatialInterpolator.InfluenceRadius = influenceRadius;
            spatialInterpolator.MinNeighborPoints = minNeighbors;
            
            Console.WriteLine($"分辨率: {resolution}m, 影响半径: {influenceRadius}m, 最小邻近点: {minNeighbors}");
            
            // 执行插值
            RunCompleteWorkflow(excelPath, outputDir);
        }
    }

    /// <summary>
    /// 程序入口示例
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var example = new SpatialInterpolationExample();
                
                // 示例1: 完整流程
                Console.WriteLine("=== 示例1: 完整数据处理和空间插值 ===");
                example.RunCompleteWorkflow(
                    @"TestData\J16原始.xlsx",
                    @"Output\Complete"
                );
                
                // 示例2: 仅空间插值
                Console.WriteLine("\n=== 示例2: 仅执行空间插值 ===");
                example.RunInterpolationOnly(
                    @"Output\3D\trajectory_data.json",
                    @"Output\JSON\fpi_analysis.json",
                    @"Output\InterpolationOnly"
                );
                
                // 示例3: 自定义参数
                Console.WriteLine("\n=== 示例3: 自定义参数插值 ===");
                example.RunWithCustomParameters(
                    @"TestData\J16原始.xlsx",
                    @"Output\CustomParams",
                    resolution: 0.3f,
                    influenceRadius: 3.0f,
                    minNeighbors: 2
                );
                
                Console.WriteLine("\n所有示例执行完成!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序执行失败: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }
    }
} 