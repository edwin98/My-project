using System;
using System.IO;
using DrillingDataProcessor.Core;

namespace DrillingDataProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("钻井数据处理器 - 控制台版本");
            Console.WriteLine("=====================================");
            
            // 创建处理器实例
            var processor = new DrillingDataProcessorCore();
            
            // 配置事件处理
            processor.OnLogMessage += message => 
            {
                Console.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} - {message}");
            };
            
            // OnErrorMessage 事件已移除，错误通过OnException事件处理
            
            processor.OnException += (message, ex) => 
            {
                Console.WriteLine($"[EXCEPTION] {DateTime.Now:HH:mm:ss} - {message}");
                Console.WriteLine($"详细错误: {ex.Message}");
                if (ex.StackTrace != null)
                {
                    Console.WriteLine($"堆栈跟踪: {ex.StackTrace}");
                }
            };
            
            // 配置处理器参数
            ConfigureProcessor(processor);
            
            // 验证输入文件
            if (!File.Exists(processor.InputExcelPath))
            {
                Console.WriteLine($"错误：Excel文件不存在: {processor.InputExcelPath}");
                Console.WriteLine("请将测试Excel文件放在TestData目录中");
                Console.WriteLine("按任意键退出...");
                Console.ReadKey();
                return;
            }
            
            try
            {
                Console.WriteLine("开始处理数据...");
                var startTime = DateTime.Now;
                
                // 执行数据处理
                bool success = processor.ProcessDrillingDataAndGenerateCharts();
                
                var endTime = DateTime.Now;
                var duration = endTime - startTime;
                
                if (success)
                {
                    Console.WriteLine($"\n处理完成！总耗时: {duration.TotalSeconds:F2} 秒");
                    Console.WriteLine("\n=== 处理结果统计 ===");
                    Console.WriteLine(processor.GetProcessingReport());
                    
                    Console.WriteLine("\n输出文件位置:");
                    Console.WriteLine($"- CSV文件: {processor.OutputPath}");
                    Console.WriteLine($"- JSON文件: {processor.JsonOutputPath}");
                    Console.WriteLine($"- 3D可视化文件: {processor.Visualization3DOutputPath}");
                }
                else
                {
                    Console.WriteLine("数据处理失败！");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"程序运行异常: {ex.Message}");
                Console.WriteLine($"详细错误: {ex}");
            }
            
            Console.WriteLine("\n按任意键退出...");
            Console.ReadKey();
        }

        static void ConfigureProcessor(DrillingDataProcessorCore processor)
        {
            // 文件路径配置
            processor.InputExcelPath = Path.Combine("TestData", "J16原始.xlsx");
            processor.OutputPath = Path.Combine("Output", "CSV");
            processor.JsonOutputPath = Path.Combine("Output", "JSON");
            
            // 处理参数
            processor.DepthStep = 1.5f;
            processor.DepthInterval = 0.2f;
            processor.MagneticDeclination = 0f;
            processor.SurveyLineTrueAzimuth = 90f;
            processor.InitialH = 0f;
            
            // 过滤条件
            processor.MinRotationSpeed = 10f;
            processor.MinDrillPressure = 200f;
            processor.MinTorque = 200f;
            processor.GravityMin = 0.98f;
            processor.GravityMax = 1.02f;
            
            // 可视化设置
            processor.GenerateCharts = true;
            processor.Generate3DVisualization = true;
            processor.Visualization3DOutputPath = Path.Combine("Output", "3D");
            
            Console.WriteLine("处理器配置完成:");
            Console.WriteLine($"- 输入文件: {processor.InputExcelPath}");
            Console.WriteLine($"- 输出目录: {processor.OutputPath}");
            Console.WriteLine($"- JSON目录: {processor.JsonOutputPath}");
            Console.WriteLine($"- 3D可视化目录: {processor.Visualization3DOutputPath}");
            Console.WriteLine($"- 深度步长: {processor.DepthStep}m");
            Console.WriteLine($"- 深度间隔: {processor.DepthInterval}m");
            Console.WriteLine($"- 生成图表: {processor.GenerateCharts}");
            Console.WriteLine($"- 生成3D可视化: {processor.Generate3DVisualization}");
            Console.WriteLine();
        }
    }
} 