using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;
using DrillingDataProcessor.Models;
using DrillingDataProcessor.Core.Interfaces;
using DrillingDataProcessor.Core.Modules;

namespace DrillingDataProcessor.Core
{
    public class DrillingDataProcessorCore
    {
        // 配置参数
        public string InputExcelPath { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public string JsonOutputPath { get; set; } = string.Empty;
        
        // 处理参数
        public float DepthStep { get; set; } = 1.5f;
        public float DepthInterval { get; set; } = 0.2f;
        public float MagneticDeclination { get; set; } = 0f;
        public float SurveyLineTrueAzimuth { get; set; } = 90f;
        public float InitialH { get; set; } = 0f;
        
        // 过滤条件
        public float MinRotationSpeed { get; set; } = 10f;
        public float MinDrillPressure { get; set; } = 200f;
        public float MinTorque { get; set; } = 200f;
        public float GravityMin { get; set; } = 0.98f;
        public float GravityMax { get; set; } = 1.02f;
        
        // 可视化设置
        public bool GenerateCharts { get; set; } = true;
        public bool Generate3DVisualization { get; set; } = true;
        public string Visualization3DOutputPath { get; set; } = string.Empty;
        
        // 缓存变量
        private List<DrillingData> allData = new List<DrillingData>();
        private List<DateTime> yellowSerials = new List<DateTime>();
        private List<DrillingData> filteredData = new List<DrillingData>();
        private List<TrajectoryPoint> trajectoryPoints = new List<TrajectoryPoint>();
        private Dictionary<float, List<DrillingData>> depthGroupCache = new Dictionary<float, List<DrillingData>>();
        
        // 模块实例
        private IMarkDetector? markDetector;
        private ITrajectoryCalculator? trajectoryCalculator;
        private IVisualization3D? visualization3D;
        private VoxelFPIInterpolator? voxelInterpolator;
        
        // 体素插值设置（默认插值方式）
        public bool EnableVoxelInterpolation { get; set; } = true;
        public float VoxelSize { get; set; } = 2.0f;
        public float VoxelInfluenceRadius { get; set; } = 15.0f;
        public int VoxelMinNeighborPoints { get; set; } = 1;
        public float VoxelBoundaryExpansion { get; set; } = 5.0f;
        
        // 事件
        public event Action<string>? OnLogMessage;
        public event Action<string, Exception>? OnException;

        public DrillingDataProcessorCore()
        {
            // 默认构造函数
            InitializeModules();
        }
        
        /// <summary>
        /// 初始化模块
        /// </summary>
        private void InitializeModules()
        {
            trajectoryCalculator = new TrajectoryCalculatorImpl();
            visualization3D = new Trajectory3DVisualizer();
            
            // 注册事件处理
            if (trajectoryCalculator is TrajectoryCalculatorImpl impl)
            {
                impl.OnLogMessage += LogMessage;
            }
            
            if (visualization3D is Trajectory3DVisualizer visualizer)
            {
                visualizer.OnLogMessage += LogMessage;
            }
        }

        /// <summary>
        /// 主处理函数
        /// </summary>
        public bool ProcessDrillingDataAndGenerateCharts()
        {
            try
            {
                LogMessage("开始处理钻井数据...");
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();
                
                // 清空缓存
                ClearCache();
                
                // 1. 加载和预处理数据
                LoadAndPreprocessData();
                LogMessage($"数据加载完成: {stopwatch.ElapsedMilliseconds}ms");
                
                // 2. 深度插值
                InterpolateDepthByRowOptimized();
                LogMessage($"深度插值完成: {stopwatch.ElapsedMilliseconds}ms");
                
                // 3. 计算FPI和UCS
                CalculateFPIOptimized();
                LogMessage($"FPI计算完成: {stopwatch.ElapsedMilliseconds}ms");
                
                // 4. 提取轨迹点
                ExtractTrajectoryPoints();
                LogMessage($"轨迹点提取完成: {stopwatch.ElapsedMilliseconds}ms");
                
                // 5. 计算轨迹
                CalculateTrajectory();
                LogMessage($"轨迹计算完成: {stopwatch.ElapsedMilliseconds}ms");
                
                // 6. 保存结果
                SaveResults();
                LogMessage($"结果保存完成: {stopwatch.ElapsedMilliseconds}ms");
                
                // 7. 生成图表数据
                if (GenerateCharts)
                {
                    GenerateChartDataOptimized();
                    LogMessage($"图表生成完成: {stopwatch.ElapsedMilliseconds}ms");
                }
                
                // 8. 生成三维可视化
                if (Generate3DVisualization)
                {
                    Generate3DTrajectoryVisualization();
                    LogMessage($"三维可视化生成完成: {stopwatch.ElapsedMilliseconds}ms");
                }
                
                // 9. 体素FPI插值（默认启用）
                if (EnableVoxelInterpolation && trajectoryPoints.Count > 0)
                {
                    PerformVoxelInterpolation();
                    LogMessage($"体素FPI插值完成: {stopwatch.ElapsedMilliseconds}ms");
                }
                
                stopwatch.Stop();
                LogMessage($"数据处理完成！总耗时: {stopwatch.ElapsedMilliseconds}ms");
                return true;
            }
            catch (Exception e)
            {
                OnException?.Invoke($"数据处理过程中发生错误: {e.Message}", e);
                return false;
            }
        }

        /// <summary>
        /// 清空缓存
        /// </summary>
        private void ClearCache()
        {
            depthGroupCache.Clear();
            allData.Clear();
            yellowSerials.Clear();
            filteredData.Clear();
            trajectoryPoints.Clear();
        }

        /// <summary>
        /// 初始化空间插值器
        /// </summary>
        private void InitializeVoxelInterpolator()
        {
            voxelInterpolator = new VoxelFPIInterpolator
            {
                VoxelSize = this.VoxelSize,
                InfluenceRadius = this.VoxelInfluenceRadius,
                MinNeighborPoints = this.VoxelMinNeighborPoints,
                BoundaryExpansion = this.VoxelBoundaryExpansion,
                FillEntireSpace = true,
                MaxInterpolationDistance = 50.0f
            };
            
            // 订阅日志事件
            voxelInterpolator.OnLogMessage += message => LogMessage($"[体素插值] {message}");
        }

        /// <summary>
        /// 执行体素FPI插值（默认插值方式）
        /// </summary>
        private void PerformVoxelInterpolation()
        {
            try
            {
                LogMessage("开始执行体素FPI插值...");
                
                // 初始化插值器
                if (voxelInterpolator == null)
                {
                    InitializeVoxelInterpolator();
                }
                
                // 准备FPI数据映射
                var depthFPIMapping = CreateDepthFPIMapping();
                
                if (depthFPIMapping.Count == 0)
                {
                    LogMessage("警告: 没有有效的FPI数据，跳过体素插值");
                    return;
                }
                
                // 执行插值
                var interpolationResult = voxelInterpolator.InterpolateFPIToVoxels(trajectoryPoints, depthFPIMapping);
                
                // 保存插值结果
                SaveVoxelInterpolationResults(interpolationResult);
                
                LogMessage($"体素插值成功: 生成 {interpolationResult.ValidVoxelCount} 个有效体素");
            }
            catch (Exception ex)
            {
                LogMessage($"体素插值失败: {ex.Message}");
                OnException?.Invoke("体素插值过程中发生错误", ex);
            }
        }

        /// <summary>
        /// 创建深度-FPI映射关系
        /// </summary>
        private Dictionary<float, float> CreateDepthFPIMapping()
        {
            var mapping = new Dictionary<float, float>();
            
            // 从已处理的数据中提取深度和FPI值
            var validFpiData = filteredData
                .Where(d => !float.IsNaN(d.Fpi) && !float.IsNaN(d.Depth) && d.Fpi > 0)
                .GroupBy(d => Math.Round(d.Depth, 1)) // 按深度分组，精确到小数点后1位
                .ToList();
            
            foreach (var group in validFpiData)
            {
                var avgFpi = group.Average(d => d.Fpi);
                mapping[(float)group.Key] = avgFpi;
            }
            
            LogMessage($"创建深度-FPI映射: {mapping.Count} 个深度点");
            return mapping;
        }

        /// <summary>
        /// 保存体素插值结果（默认插值方式）
        /// </summary>
        private void SaveVoxelInterpolationResults(VoxelFPIInterpolator.VoxelInterpolationResult result)
        {
            try
            {
                // 确保输出目录存在 - 直接保存到项目根目录的Output\VoxelInterpolation
                var projectRoot = GetProjectRootDirectory();
                var interpolationOutputPath = Path.Combine(projectRoot, "Output", "VoxelInterpolation");
                Directory.CreateDirectory(interpolationOutputPath);
                
                // 使用批量导出功能保存所有格式
                voxelInterpolator.ExportAllFormats(result, interpolationOutputPath, "voxel_fpi");
                
                LogMessage($"体素插值结果已保存到: {interpolationOutputPath}");
            }
            catch (Exception ex)
            {
                LogMessage($"保存体素插值结果失败: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// 加载和预处理数据
        /// </summary>
        private void LoadAndPreprocessData()
        {
            LogMessage("正在加载Excel数据...");
            
            if (!File.Exists(InputExcelPath))
            {
                throw new FileNotFoundException($"Excel文件不存在: {InputExcelPath}");
            }
            
            using (FileStream fs = new FileStream(InputExcelPath, FileMode.Open, FileAccess.Read))
            {
                IWorkbook workbook = new XSSFWorkbook(fs);
                ISheet sheet = workbook.GetSheetAt(0);
                
                // 预分配容量提升性能
                int estimatedRows = sheet.LastRowNum;
                allData.Capacity = estimatedRows;
                
                // 读取数据
                for (int row = 1; row <= sheet.LastRowNum; row++)
                {
                    IRow dataRow = sheet.GetRow(row);
                    if (dataRow == null) continue;
                    
                    DrillingData data = new DrillingData
                    {
                        OriginalIndex = GetCellIntValue(dataRow.GetCell(0)),
                        Date = "",
                        Time = "",
                        Torque = GetCellFloatValue(dataRow.GetCell(3)),
                        DrillPressure = GetCellFloatValue(dataRow.GetCell(4)),
                        RotationSpeed = GetCellFloatValue(dataRow.GetCell(5)),
                        Temperature = GetCellFloatValue(dataRow.GetCell(6)),
                        Inclination = GetCellFloatValue(dataRow.GetCell(7)),
                        Azimuth = GetCellFloatValue(dataRow.GetCell(8)),
                        GravitySum = GetCellFloatValue(dataRow.GetCell(9)),
                        MagneticStrength = GetCellFloatValue(dataRow.GetCell(10)),
                        MagneticInclination = GetCellFloatValue(dataRow.GetCell(11)),
                        Voltage = GetCellFloatValue(dataRow.GetCell(12)),
                        IsMarked = false
                    };
                    
                    // 解析时间戳 - 参考Python pandas.to_datetime的逻辑
                    data.Timestamp = ParseTimestamp(dataRow.GetCell(1), dataRow.GetCell(2), out string dateStr, out string timeStr);
                    data.Date = dateStr;
                    data.Time = timeStr;
                    
                    allData.Add(data);
                }
                
                // 检测标记行
                DetectYellowMarkedRowsEnhanced(sheet);
                
                workbook.Close();
            }
            
            // 过滤无效数据
            FilterInvalidData();
            LogMessage($"加载完成：总数据 {allData.Count} 条，过滤后 {filteredData.Count} 条");
        }

        /// <summary>
        /// 过滤无效数据
        /// </summary>
        private void FilterInvalidData()
        {
            filteredData = allData.Where(data => 
                data.RotationSpeed >= MinRotationSpeed &&
                data.DrillPressure >= MinDrillPressure &&
                data.Torque >= MinTorque &&
                !(data.GravitySum >= GravityMin && data.GravitySum <= GravityMax)
            ).ToList();
            
            var markedData = allData.Where(data => yellowSerials.Contains(data.Timestamp)).ToList();
            filteredData.AddRange(markedData);
            
            filteredData = filteredData
                .GroupBy(data => data.Timestamp)
                .Select(g => g.First())
                .OrderBy(data => data.Timestamp)
                .ToList();
        }

        /// <summary>
        /// 改进的黄色标记行检测 - 基于Excel单元格填充颜色
        /// </summary>
        private void DetectYellowMarkedRowsEnhanced(ISheet sheet)
        {
            yellowSerials.Clear();
            
            // 初始化标记检测器
            if (markDetector == null)
            {
                markDetector = new YellowMarkDetector(allData);
                if (markDetector is YellowMarkDetector detector)
                {
                    detector.OnLogMessage += LogMessage;
                }
            }
            
            LogMessage("开始检测黄色标记行...");
            
            try
            {
                yellowSerials = markDetector.DetectMarkedRows(sheet);
            }
            catch (Exception e)
            {
                LogMessage($"黄色标记行检测失败: {e.Message}");
                throw new InvalidOperationException("无法检测黄色标记行，请检查Excel文件格式", e);
            }
            
            LogMessage($"检测到 {yellowSerials.Count} 个黄色标记行");
            
            // 标记对应的数据
            MarkDataRows();
        }





        /// <summary>
        /// 标记数据行
        /// </summary>
        private void MarkDataRows()
        {
            foreach (var data in allData)
            {
                data.IsMarked = yellowSerials.Contains(data.Timestamp);
            }
            
            LogMessage($"已标记 {allData.Count(d => d.IsMarked)} 行数据为标记行");
        }

        /// <summary>
        /// 优化的深度插值
        /// </summary>
        private void InterpolateDepthByRowOptimized()
        {
            LogMessage("正在进行深度插值...");
            
            var sortedData = filteredData.OrderBy(d => d.Timestamp).ToList();
            var markIndices = new List<int>();
            
            // 找到所有标记行的索引
            for (int i = 0; i < sortedData.Count; i++)
            {
                if (yellowSerials.Contains(sortedData[i].Timestamp))
                {
                    markIndices.Add(i);
                }
            }
            
            if (markIndices.Count < 2)
            {
                LogMessage("标记行数量不足，无法进行深度插值");
                return;
            }
            
            // 在标记行之间进行深度插值
            for (int i = 0; i < markIndices.Count - 1; i++)
            {
                int startIdx = markIndices[i];
                int endIdx = markIndices[i + 1];
                int numRows = endIdx - startIdx + 1;
                
                if (numRows <= 1) continue;
                
                float startDepth = DepthStep * i;
                float endDepth = DepthStep * (i + 1);
                
                // 使用线性插值分配深度
                for (int j = 0; j < numRows; j++)
                {
                    if (startIdx + j < sortedData.Count)
                    {
                        float t = numRows > 1 ? (float)j / (numRows - 1) : 0f;
                        float depth = Lerp(startDepth, endDepth, t);
                        sortedData[startIdx + j].Depth = depth;
                    }
                }
            }
            
            // 处理第一个标记行之前的数据
            if (markIndices.Count > 0 && markIndices[0] > 0)
            {
                float firstMarkDepth = 0f;
                for (int i = 0; i < markIndices[0]; i++)
                {
                    sortedData[i].Depth = firstMarkDepth - (markIndices[0] - i) * (DepthStep / markIndices[0]);
                }
            }
            
            // 处理最后一个标记行之后的数据
            if (markIndices.Count > 0 && markIndices[markIndices.Count - 1] < sortedData.Count - 1)
            {
                int lastMarkIdx = markIndices[markIndices.Count - 1];
                float lastMarkDepth = DepthStep * (markIndices.Count - 1);
                
                for (int i = lastMarkIdx + 1; i < sortedData.Count; i++)
                {
                    sortedData[i].Depth = lastMarkDepth + (i - lastMarkIdx) * (DepthStep / (sortedData.Count - lastMarkIdx));
                }
            }
            
            filteredData = sortedData;
            LogMessage($"深度插值完成，处理了 {markIndices.Count} 个标记行段");
        }

        /// <summary>
        /// 优化的FPI计算
        /// </summary>
        private void CalculateFPIOptimized()
        {
            LogMessage("正在计算FPI和UCS...");
            
            var sortedData = filteredData.OrderBy(d => d.Depth).ToList();
            
            // 按深度区间分组计算
            var depthGroups = sortedData
                .Where(d => !float.IsNaN(d.Depth))
                .GroupBy(d => MathF.Floor(d.Depth / DepthInterval) * DepthInterval)
                .ToList();
            
            foreach (var group in depthGroups)
            {
                var sectionData = group.OrderBy(d => d.Timestamp).ToList();
                
                if (sectionData.Count < 2) continue;
                
                // 计算时间差
                var timeDeltas = new List<float>();
                for (int i = 1; i < sectionData.Count; i++)
                {
                    float timeDelta = (float)(sectionData[i].Timestamp - sectionData[i - 1].Timestamp).TotalSeconds;
                    timeDeltas.Add(timeDelta);
                }
                
                // 计算累积值
                float totalRevs = 0f;
                float totalWob = 0f;
                float totalTime = timeDeltas.Sum();
                
                for (int i = 0; i < sectionData.Count; i++)
                {
                    float timeDelta = i < timeDeltas.Count ? timeDeltas[i] : 1f;
                    totalRevs += sectionData[i].RotationSpeed * timeDelta / 60f; // 转换为转数
                    totalWob += sectionData[i].DrillPressure * timeDelta;
                }
                
                // 计算FPI和UCS
                float penetrationPerRev = totalRevs > 0 ? DepthInterval / totalRevs : float.NaN;
                float avgWob = totalTime > 0 ? totalWob / totalTime : float.NaN;
                float fpi = (penetrationPerRev > 0 && !float.IsNaN(penetrationPerRev) && avgWob > 0) 
                           ? avgWob / penetrationPerRev : float.NaN;
                float ucs = !float.IsNaN(fpi) ? 3.076f * fpi : float.NaN;
                
                // 将计算结果赋值给该区间的所有数据
                foreach (var data in sectionData)
                {
                    var originalData = sortedData.FirstOrDefault(d => 
                        d.Timestamp == data.Timestamp && d.OriginalIndex == data.OriginalIndex);
                    if (originalData != null)
                    {
                        originalData.Fpi = fpi;
                        originalData.Ucs = ucs;
                    }
                }
            }
            
            filteredData = sortedData;
            
            var validFpiCount = filteredData.Count(d => !float.IsNaN(d.Fpi));
            LogMessage($"FPI计算完成，有效数据 {validFpiCount} 条");
        }

        /// <summary>
        /// 提取轨迹点 - 使用轨迹计算模块
        /// </summary>
        private void ExtractTrajectoryPoints()
        {
            if (trajectoryCalculator == null)
            {
                throw new InvalidOperationException("轨迹计算器未初始化");
            }
            
            trajectoryPoints = trajectoryCalculator.ExtractTrajectoryPoints(allData, yellowSerials);
        }

        /// <summary>
        /// 计算轨迹 - 使用轨迹计算模块
        /// </summary>
        private void CalculateTrajectory()
        {
            if (trajectoryCalculator == null)
            {
                throw new InvalidOperationException("轨迹计算器未初始化");
            }
            
            var parameters = new TrajectoryCalculationParameters
            {
                DepthStep = DepthStep,
                MagneticDeclination = MagneticDeclination,
                SurveyLineTrueAzimuth = SurveyLineTrueAzimuth,
                InitialH = InitialH
            };
            
            trajectoryPoints = trajectoryCalculator.CalculateTrajectory(trajectoryPoints, parameters);
        }

        /// <summary>
        /// 保存结果
        /// </summary>
        private void SaveResults()
        {
            LogMessage("正在保存结果...");
            
            if (!Directory.Exists(OutputPath))
            {
                Directory.CreateDirectory(OutputPath);
            }
            
            // 保存主数据文件
            SaveToCSV(filteredData, Path.Combine(OutputPath, "J16原始_depth_FPI_UCS.csv"));
            
            // 保存轨迹点文件
            SaveTrajectoryPoints();
            
            // 保存轨迹计算结果
            SaveTrajectoryCalculated();
            
            // 保存标记行数据
            SaveMarkData();
            
            LogMessage("结果保存完成！");
        }

        /// <summary>
        /// 优化的图表数据生成
        /// </summary>
        private void GenerateChartDataOptimized()
        {
            LogMessage("正在生成图表数据...");
            
            // 预先创建深度分组缓存
            BuildDepthGroupCache();
            
            // 生成图表数据
            GenerateFPIChartDataOptimized();
            GenerateDrillingParametersChartDataOptimized();
            GenerateTrajectoryChartDataOptimized();
            
            LogMessage("图表数据生成完成！");
        }

        /// <summary>
        /// 构建深度分组缓存
        /// </summary>
        private void BuildDepthGroupCache()
        {
            depthGroupCache.Clear();
            
            foreach (var data in filteredData)
            {
                if (float.IsNaN(data.Depth)) continue;
                
                float depthKey = MathF.Floor(data.Depth / DepthInterval) * DepthInterval;
                
                if (!depthGroupCache.ContainsKey(depthKey))
                {
                    depthGroupCache[depthKey] = new List<DrillingData>();
                }
                
                depthGroupCache[depthKey].Add(data);
            }
        }

        // 辅助方法
        private void LogMessage(string message)
        {
            OnLogMessage?.Invoke(message);
        }

        /// <summary>
        /// 获取项目根目录路径
        /// </summary>
        private string GetProjectRootDirectory()
        {
            // 从当前执行文件位置向上查找包含.csproj文件的目录
            string currentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            DirectoryInfo? directory = new DirectoryInfo(currentDirectory);
            
            while (directory != null)
            {
                // 查找.csproj文件
                if (directory.GetFiles("*.csproj").Length > 0)
                {
                    return directory.FullName;
                }
                
                directory = directory.Parent;
            }
            
            // 如果找不到，使用当前目录
            return currentDirectory;
        }
        
        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        /// <summary>
        /// 解析Excel时间戳 - 简化版本
        /// </summary>
        private DateTime ParseTimestamp(ICell? dateCell, ICell? timeCell, out string dateStr, out string timeStr)
        {
            dateStr = "";
            timeStr = "";
            
            try
            {
                // 优先处理Excel日期格式
                if (dateCell?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(dateCell))
                {
                    var date = dateCell.DateCellValue;
                    dateStr = date.ToString("yyyy-MM-dd");
                    
                    if (timeCell?.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(timeCell))
                    {
                        var time = timeCell.DateCellValue;
                        timeStr = time.ToString("HH:mm:ss");
                        return new DateTime(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second);
                    }
                    else
                    {
                        timeStr = GetCellStringValue(timeCell);
                        if (DateTime.TryParse($"{dateStr} {timeStr}", out DateTime combined))
                            return combined;
                    }
                }
                
                // 处理字符串格式
                dateStr = GetCellStringValue(dateCell);
                timeStr = GetCellStringValue(timeCell);
                
                if (!string.IsNullOrEmpty(dateStr) && !string.IsNullOrEmpty(timeStr))
                {
                    if (DateTime.TryParse($"{dateStr} {timeStr}", out DateTime parsed))
                        return parsed;
                }
                
                // 单一单元格完整时间
                if (dateCell != null && timeCell == null)
                {
                    if (dateCell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(dateCell))
                    {
                        var fullDateTime = dateCell.DateCellValue;
                        dateStr = fullDateTime.ToString("yyyy-MM-dd");
                        timeStr = fullDateTime.ToString("HH:mm:ss");
                        return fullDateTime;
                    }
                }
                
                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                LogMessage($"时间戳解析失败: {ex.Message}");
                dateStr = GetCellStringValue(dateCell);
                timeStr = GetCellStringValue(timeCell);
                return DateTime.MinValue;
            }
        }

        private string GetCellStringValue(ICell? cell)
        {
            if (cell == null) return "";
            
            try
            {
                switch (cell.CellType)
                {
                    case CellType.String:
                        return cell.StringCellValue;
                    case CellType.Numeric:
                        return DateUtil.IsCellDateFormatted(cell) 
                            ? cell.DateCellValue.ToString("yyyy-MM-dd HH:mm:ss")
                            : cell.NumericCellValue.ToString();
                    case CellType.Boolean:
                        return cell.BooleanCellValue.ToString();
                    case CellType.Formula:
                        return cell.StringCellValue;
                    default:
                        return "";
                }
            }
            catch
            {
                return "";
            }
        }

        private int GetCellIntValue(ICell? cell)
        {
            if (cell == null) return 0;
            
            try
            {
                return cell.CellType switch
                {
                    CellType.Numeric or CellType.Formula => (int)cell.NumericCellValue,
                    CellType.String => int.TryParse(cell.StringCellValue, out int result) ? result : 0,
                    _ => 0
                };
            }
            catch
            {
                return 0;
            }
        }

        private float GetCellFloatValue(ICell? cell)
        {
            if (cell == null) return float.NaN;
            
            try
            {
                return cell.CellType switch
                {
                    CellType.Numeric or CellType.Formula => (float)cell.NumericCellValue,
                    CellType.String => float.TryParse(cell.StringCellValue, out float result) ? result : float.NaN,
                    _ => float.NaN
                };
            }
            catch
            {
                return float.NaN;
            }
        }

        // 以下是缺失的方法实现，需要从Unity代码中移植过来
        private void SaveToCSV(List<DrillingData> data, string filePath)
        {
            var csvContent = new System.Text.StringBuilder();
            csvContent.AppendLine("原序号,日期,时间,扭矩,钻压,转速,温度,倾角,方位角,重力和,磁强,磁倾,电压,深度,FPI,UCS,是否标记");
            
            foreach (var item in data)
            {
                csvContent.AppendLine($"{item.OriginalIndex},{item.Date},{item.Time},{item.Torque:F2}," +
                                    $"{item.DrillPressure:F2},{item.RotationSpeed:F2},{item.Temperature:F2}," +
                                    $"{item.Inclination:F3},{item.Azimuth:F3},{item.GravitySum:F3}," +
                                    $"{item.MagneticStrength:F2},{item.MagneticInclination:F3},{item.Voltage:F2}," +
                                    $"{item.Depth:F2},{item.Fpi:F3},{item.Ucs:F2},{item.IsMarked}");
            }
            
            File.WriteAllText(filePath, csvContent.ToString());
            LogMessage($"主数据文件已保存: {filePath}");
        }

        private void SaveTrajectoryPoints()
        {
            var csvContent = new System.Text.StringBuilder();
            csvContent.AppendLine("序号,标记时间,倾角,方位角,重力和");
            
            for (int i = 0; i < trajectoryPoints.Count; i++)
            {
                var point = trajectoryPoints[i];
                csvContent.AppendLine($"{i + 1},{point.MarkTimestamp:yyyy-MM-dd HH:mm:ss}," +
                                    $"{point.Inclination:F3},{point.Azimuth:F3},{point.GravitySum:F3}");
            }
            
            string filePath = Path.Combine(OutputPath, "trajectory_points.csv");
            File.WriteAllText(filePath, csvContent.ToString());
            LogMessage($"轨迹点文件已保存: {filePath}");
        }

        private void SaveTrajectoryCalculated()
        {
            var csvContent = new System.Text.StringBuilder();
            csvContent.AppendLine("序号,杆长,平均倾角,平均磁方位角,E位移,N位移,垂深,X坐标,侧向偏移,H值,FPI,UCS");
            
            for (int i = 0; i < trajectoryPoints.Count; i++)
            {
                var point = trajectoryPoints[i];
                
                // 查找对应的FPI和UCS值
                var depthRange = 0.5f;
                var correspondingData = filteredData
                    .Where(d => Math.Abs(d.Depth - point.RodLength) <= depthRange && 
                               !float.IsNaN(d.Fpi) && !float.IsNaN(d.Ucs))
                    .OrderBy(d => Math.Abs(d.Depth - point.RodLength))
                    .FirstOrDefault();
                
                float fpiValue = correspondingData?.Fpi ?? float.NaN;
                float ucsValue = correspondingData?.Ucs ?? float.NaN;
                
                csvContent.AppendLine($"{i + 1},{point.RodLength:F2},{point.AvgInclination:F3}," +
                                    $"{point.AvgMagneticAzimuth:F3},{point.EastDisplacement:F3}," +
                                    $"{point.NorthDisplacement:F3},{point.VerticalDepth:F3}," +
                                    $"{point.XCoordinate:F3},{point.LateralDeviation:F3},{point.HValue:F3}," +
                                    $"{fpiValue:F3},{ucsValue:F3}");
            }
            
            string filePath = Path.Combine(OutputPath, "trajectory_calculated.csv");
            File.WriteAllText(filePath, csvContent.ToString());
            LogMessage($"轨迹计算结果已保存: {filePath}");
            
            // 同时保存轨迹点与FPI关联的专用文件
            SaveTrajectoryWithFPIValues();
        }

        /// <summary>
        /// 保存轨迹点坐标与FPI值关联的专用CSV文件
        /// </summary>
        private void SaveTrajectoryWithFPIValues()
        {
            var csvContent = new System.Text.StringBuilder();
            csvContent.AppendLine("序号,X坐标,Y坐标,Z坐标,深度,倾角,方位角,FPI,UCS,标记时间戳");
            
            for (int i = 0; i < trajectoryPoints.Count; i++)
            {
                var point = trajectoryPoints[i];
                
                // 查找对应的FPI和UCS值
                var depthRange = 0.5f;
                var correspondingData = filteredData
                    .Where(d => Math.Abs(d.Depth - point.RodLength) <= depthRange && 
                               !float.IsNaN(d.Fpi) && !float.IsNaN(d.Ucs))
                    .OrderBy(d => Math.Abs(d.Depth - point.RodLength))
                    .FirstOrDefault();
                
                // 如果没找到精确匹配，扩大搜索范围
                if (correspondingData == null)
                {
                    correspondingData = filteredData
                        .Where(d => !float.IsNaN(d.Fpi) && !float.IsNaN(d.Ucs))
                        .OrderBy(d => Math.Abs(d.Depth - point.RodLength))
                        .FirstOrDefault();
                }
                
                float fpiValue = correspondingData?.Fpi ?? float.NaN;
                float ucsValue = correspondingData?.Ucs ?? float.NaN;
                
                csvContent.AppendLine($"{i + 1},{point.EastDisplacement:F3},{point.NorthDisplacement:F3}," +
                                    $"{-point.VerticalDepth:F3},{point.RodLength:F2}," +
                                    $"{point.Inclination:F3},{point.Azimuth:F3}," +
                                    $"{fpiValue:F3},{ucsValue:F3}," +
                                    $"\"{point.MarkTimestamp:yyyy-MM-dd HH:mm:ss}\"");
            }
            
            string filePath = Path.Combine(OutputPath, "trajectory_points_with_fpi.csv");
            File.WriteAllText(filePath, csvContent.ToString());
            LogMessage($"轨迹点坐标与FPI关联文件已保存: {filePath}");
        }

        private void SaveMarkData()
        {
            var markedData = allData.Where(d => d.IsMarked).OrderBy(d => d.Timestamp).ToList();
            
            var csvContent = new System.Text.StringBuilder();
            csvContent.AppendLine("序号,标记时间戳,原序号,日期,时间,扭矩,钻压,转速,温度,倾角,方位角,重力和,磁强,磁倾,电压,深度");
            
            for (int i = 0; i < markedData.Count; i++)
            {
                var item = markedData[i];
                csvContent.AppendLine($"{i + 1},{item.Timestamp:yyyy-MM-dd HH:mm:ss},{item.OriginalIndex}," +
                                    $"{item.Date},{item.Time},{item.Torque:F2},{item.DrillPressure:F2}," +
                                    $"{item.RotationSpeed:F2},{item.Temperature:F2},{item.Inclination:F3}," +
                                    $"{item.Azimuth:F3},{item.GravitySum:F3},{item.MagneticStrength:F2}," +
                                    $"{item.MagneticInclination:F3},{item.Voltage:F2},{item.Depth:F2}");
            }
            
            string filePath = Path.Combine(OutputPath, "mark_data.csv");
            File.WriteAllText(filePath, csvContent.ToString());
            LogMessage($"标记行数据已保存: {filePath} (共 {markedData.Count} 条)");
        }

        private void GenerateFPIChartDataOptimized()
        {
            var chartData = new DrillingChartData();
            chartData.XAxis.Title = "深度 (m)";
            
            var fpiSeries = new DrillingChartSeries { Name = "FPI贯入指数", Display = true };
            var ucsSeries = new DrillingChartSeries { Name = "UCS煤岩强度", Display = true };
            
            var sortedDepths = depthGroupCache.Keys.OrderBy(k => k).ToList();
            
            foreach (var depthKey in sortedDepths)
            {
                var group = depthGroupCache[depthKey];
                var validFpiData = group.Where(d => !float.IsNaN(d.Fpi)).ToList();
                
                if (validFpiData.Count > 0)
                {
                    float avgFpi = validFpiData.Average(d => d.Fpi);
                    float avgUcs = validFpiData.Average(d => d.Ucs);
                    
                    string key = $"{depthKey:F1}m";
                    
                    fpiSeries.Data.Add(new DrillingChartDataPoint { Key = key, Value = avgFpi });
                    ucsSeries.Data.Add(new DrillingChartDataPoint { Key = key, Value = avgUcs });
                    chartData.XAxis.Label.Add(key);
                }
            }
            
            chartData.Series.Add(fpiSeries);
            chartData.Series.Add(ucsSeries);
            
            SaveChartDataToJson(chartData, "fpi_analysis.json");
        }

        private void GenerateDrillingParametersChartDataOptimized()
        {
            var chartData = new DrillingChartData();
            chartData.XAxis.Title = "深度 (m)";
            
            var pressureSeries = new DrillingChartSeries { Name = "钻压", Display = true };
            var torqueSeries = new DrillingChartSeries { Name = "扭矩", Display = true };
            var speedSeries = new DrillingChartSeries { Name = "转速", Display = true };
            
            var sortedDepths = depthGroupCache.Keys.OrderBy(k => k).ToList();
            
            foreach (var depthKey in sortedDepths)
            {
                var group = depthGroupCache[depthKey];
                
                if (group.Count > 0)
                {
                    float avgPressure = group.Average(d => d.DrillPressure);
                    float avgTorque = group.Average(d => d.Torque);
                    float avgSpeed = group.Average(d => d.RotationSpeed);
                    
                    string key = $"{depthKey:F1}m";
                    
                    pressureSeries.Data.Add(new DrillingChartDataPoint { Key = key, Value = avgPressure });
                    torqueSeries.Data.Add(new DrillingChartDataPoint { Key = key, Value = avgTorque });
                    speedSeries.Data.Add(new DrillingChartDataPoint { Key = key, Value = avgSpeed });
                }
            }
            
            // 设置X轴标签
            chartData.XAxis.Label = depthGroupCache.Keys.OrderBy(k => k).Select(k => $"{k:F1}m").ToList();
            
            chartData.Series.Add(pressureSeries);
            chartData.Series.Add(torqueSeries);
            chartData.Series.Add(speedSeries);
            
            SaveChartDataToJson(chartData, "drilling_parameters.json");
        }

        private void GenerateTrajectoryChartDataOptimized()
        {
            var chartData = new DrillingChartData();
            chartData.XAxis.Title = "深度 (m)";
            
            var eastSeries = new DrillingChartSeries { Name = "E位移", Display = true };
            var northSeries = new DrillingChartSeries { Name = "N位移", Display = true };
            var verticalSeries = new DrillingChartSeries { Name = "垂深", Display = true };
            
            foreach (var point in trajectoryPoints)
            {
                string key = $"{point.RodLength:F1}m";
                
                eastSeries.Data.Add(new DrillingChartDataPoint { Key = key, Value = point.EastDisplacement });
                northSeries.Data.Add(new DrillingChartDataPoint { Key = key, Value = point.NorthDisplacement });
                verticalSeries.Data.Add(new DrillingChartDataPoint { Key = key, Value = point.VerticalDepth });
                chartData.XAxis.Label.Add(key);
            }
            
            chartData.Series.Add(eastSeries);
            chartData.Series.Add(northSeries);
            chartData.Series.Add(verticalSeries);
            
            SaveChartDataToJson(chartData, "trajectory_analysis.json");
        }

        private void SaveChartDataToJson(DrillingChartData chartData, string fileName)
        {
            try
            {
                if (!Directory.Exists(JsonOutputPath))
                {
                    Directory.CreateDirectory(JsonOutputPath);
                }
                
                string jsonContent = JsonConvert.SerializeObject(chartData, Formatting.Indented);
                string filePath = Path.Combine(JsonOutputPath, fileName);
                
                File.WriteAllText(filePath, jsonContent);
                
                LogMessage($"图表数据已保存: {filePath}");
            }
            catch (Exception e)
            {
                OnException?.Invoke($"保存图表数据时发生错误: {e.Message}", e);
            }
        }
        
        /// <summary>
        /// 生成三维轨迹可视化
        /// </summary>
        private void Generate3DTrajectoryVisualization()
        {
            LogMessage("正在生成三维轨迹可视化...");
            
            try
            {
                // 确保输出目录存在
                var outputPath = string.IsNullOrEmpty(Visualization3DOutputPath) ? OutputPath : Visualization3DOutputPath;
                if (!Directory.Exists(outputPath))
                {
                    Directory.CreateDirectory(outputPath);
                }
                
                // 生成Unity JSON格式
                GenerateUnityTrajectoryJSON(outputPath);
                
                // 生成离线HTML可视化
                GenerateOfflineHTML(outputPath);
                
                LogMessage("三维可视化生成完成");
            }
            catch (Exception e)
            {
                OnException?.Invoke($"生成三维可视化时发生错误: {e.Message}", e);
            }
        }

        /// <summary>
        /// 生成Unity轨迹JSON格式
        /// </summary>
        private void GenerateUnityTrajectoryJSON(string outputPath)
        {
            try
            {
                var unityTrajectoryData = new
                {
                    points = trajectoryPoints.Select((point, index) => {
                        // 根据轨迹点的深度范围查找对应的FPI和UCS数据
                        var depthRange = 0.5f; // 查找深度范围±0.5米
                        var correspondingData = filteredData
                            .Where(d => Math.Abs(d.Depth - point.RodLength) <= depthRange && 
                                       !float.IsNaN(d.Fpi) && !float.IsNaN(d.Ucs))
                            .OrderBy(d => Math.Abs(d.Depth - point.RodLength))
                            .FirstOrDefault();
                        
                        // 如果没找到精确匹配，扩大搜索范围
                        if (correspondingData == null)
                        {
                            correspondingData = filteredData
                                .Where(d => !float.IsNaN(d.Fpi) && !float.IsNaN(d.Ucs))
                                .OrderBy(d => Math.Abs(d.Depth - point.RodLength))
                                .FirstOrDefault();
                        }
                        
                        // 如果还是没找到，使用该深度区间的平均值
                        float fpiValue = float.NaN;
                        float ucsValue = float.NaN;
                        
                        if (correspondingData != null)
                        {
                            fpiValue = correspondingData.Fpi;
                            ucsValue = correspondingData.Ucs;
                        }
                        else
                        {
                            // 使用深度区间的平均FPI和UCS
                            var nearbyData = filteredData
                                .Where(d => Math.Abs(d.Depth - point.RodLength) <= 2.0f && 
                                           !float.IsNaN(d.Fpi) && !float.IsNaN(d.Ucs))
                                .ToList();
                            
                            if (nearbyData.Count > 0)
                            {
                                fpiValue = nearbyData.Average(d => d.Fpi);
                                ucsValue = nearbyData.Average(d => d.Ucs);
                            }
                        }
                        
                        return new
                        {
                            x = point.EastDisplacement,
                            y = point.NorthDisplacement,
                            z = -point.VerticalDepth, // Unity使用负Z作为深度
                            depth = point.RodLength,
                            inclination = point.Inclination,
                            azimuth = point.Azimuth,
                            fpi = fpiValue,
                            ucs = ucsValue,
                            // 添加额外的统计信息
                            pointIndex = index,
                            markTimestamp = point.MarkTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                            gravitySum = point.GravitySum
                        };
                    }).ToList(),
                    title = "钻孔轨迹三维可视化",
                    totalLength = trajectoryPoints.Count > 0 ? trajectoryPoints.Last().RodLength : 0f,
                    maxDepth = trajectoryPoints.Count > 0 ? trajectoryPoints.Max(p => p.VerticalDepth) : 0f,
                    bounds = new
                    {
                        x = trajectoryPoints.Count > 0 ? trajectoryPoints.Max(p => p.EastDisplacement) - trajectoryPoints.Min(p => p.EastDisplacement) : 0f,
                        y = trajectoryPoints.Count > 0 ? trajectoryPoints.Max(p => p.VerticalDepth) - trajectoryPoints.Min(p => p.VerticalDepth) : 0f,
                        z = trajectoryPoints.Count > 0 ? trajectoryPoints.Max(p => p.NorthDisplacement) - trajectoryPoints.Min(p => p.NorthDisplacement) : 0f
                    },
                    // 添加FPI和UCS的统计信息
                    statistics = new
                    {
                        validFpiPoints = trajectoryPoints.Count(p => {
                            var data = filteredData.FirstOrDefault(d => Math.Abs(d.Depth - p.RodLength) <= 0.5f && !float.IsNaN(d.Fpi));
                            return data != null;
                        }),
                        avgFpi = filteredData.Where(d => !float.IsNaN(d.Fpi)).Count() > 0 ? 
                                filteredData.Where(d => !float.IsNaN(d.Fpi)).Average(d => d.Fpi) : 0f,
                        maxFpi = filteredData.Where(d => !float.IsNaN(d.Fpi)).Count() > 0 ? 
                                filteredData.Where(d => !float.IsNaN(d.Fpi)).Max(d => d.Fpi) : 0f,
                        minFpi = filteredData.Where(d => !float.IsNaN(d.Fpi)).Count() > 0 ? 
                                filteredData.Where(d => !float.IsNaN(d.Fpi)).Min(d => d.Fpi) : 0f,
                        avgUcs = filteredData.Where(d => !float.IsNaN(d.Ucs)).Count() > 0 ? 
                                filteredData.Where(d => !float.IsNaN(d.Ucs)).Average(d => d.Ucs) : 0f,
                        maxUcs = filteredData.Where(d => !float.IsNaN(d.Ucs)).Count() > 0 ? 
                                filteredData.Where(d => !float.IsNaN(d.Ucs)).Max(d => d.Ucs) : 0f,
                        minUcs = filteredData.Where(d => !float.IsNaN(d.Ucs)).Count() > 0 ? 
                                filteredData.Where(d => !float.IsNaN(d.Ucs)).Min(d => d.Ucs) : 0f
                    }
                };
                
                string unityJsonPath = Path.Combine(outputPath, "trajectory_data.json");
                string unityJsonContent = JsonConvert.SerializeObject(unityTrajectoryData, Formatting.Indented);
                File.WriteAllText(unityJsonPath, unityJsonContent);
                
                LogMessage($"Unity轨迹JSON已保存: {unityJsonPath}");
                
                // 同时保存到Unity项目的StreamingAssets目录
                string unityProjectPath = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(OutputPath)));
                string streamingAssetsPath = Path.Combine(unityProjectPath, "Assets", "StreamingAssets");
                
                if (Directory.Exists(streamingAssetsPath))
                {
                    string unityStreamingPath = Path.Combine(streamingAssetsPath, "trajectory_data.json");
                    File.WriteAllText(unityStreamingPath, unityJsonContent);
                    LogMessage($"Unity StreamingAssets轨迹数据已保存: {unityStreamingPath}");
                }
            }
            catch (Exception e)
            {
                LogMessage($"生成Unity JSON时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 生成离线HTML可视化
        /// </summary>
        private void GenerateOfflineHTML(string outputPath)
        {
            try
            {
                var htmlContent = GenerateOfflineHTMLContent();
                string htmlPath = Path.Combine(outputPath, "trajectory_3d_offline.html");
                File.WriteAllText(htmlPath, htmlContent);
                LogMessage($"离线HTML可视化已保存: {htmlPath}");
            }
            catch (Exception e)
            {
                LogMessage($"生成离线HTML时发生错误: {e.Message}");
            }
        }

        /// <summary>
        /// 生成离线HTML内容
        /// </summary>
        private string GenerateOfflineHTMLContent()
        {
            var trajectoryDataJson = JsonConvert.SerializeObject(
                trajectoryPoints.Select((point, index) => {
                    // 根据轨迹点的深度范围查找对应的FPI和UCS数据
                    var correspondingData = filteredData
                        .Where(d => Math.Abs(d.Depth - point.RodLength) <= 0.5f && 
                                   !float.IsNaN(d.Fpi) && !float.IsNaN(d.Ucs))
                        .OrderBy(d => Math.Abs(d.Depth - point.RodLength))
                        .FirstOrDefault();
                    
                    // 如果没找到精确匹配，扩大搜索范围
                    if (correspondingData == null)
                    {
                        correspondingData = filteredData
                            .Where(d => !float.IsNaN(d.Fpi) && !float.IsNaN(d.Ucs))
                            .OrderBy(d => Math.Abs(d.Depth - point.RodLength))
                            .FirstOrDefault();
                    }
                    
                    return new
                    {
                        x = point.EastDisplacement,
                        y = point.NorthDisplacement,
                        z = -point.VerticalDepth,
                        depth = point.RodLength,
                        inc = point.Inclination,
                        azi = point.Azimuth,
                        fpi = correspondingData?.Fpi ?? float.NaN,
                        ucs = correspondingData?.Ucs ?? float.NaN,
                        pointIndex = index,
                        markTime = point.MarkTimestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                        gravity = point.GravitySum
                    };
                }).ToArray(),
                Formatting.None
            );

            return $@"<!DOCTYPE html>
<html lang='zh-CN'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>钻孔轨迹三维可视化 - 离线版本</title>
    <style>
        body {{ 
            margin: 0; 
            padding: 0; 
            overflow: hidden; 
            font-family: Arial, sans-serif; 
            background: #111; 
        }}
        #container {{ 
            width: 100vw; 
            height: 100vh; 
            position: relative; 
        }}
        #info {{ 
            position: absolute; 
            top: 10px; 
            left: 10px; 
            color: white; 
            background: rgba(0,0,0,0.7); 
            padding: 15px; 
            border-radius: 5px; 
            font-size: 14px;
            max-width: 250px;
        }}
        #controls {{ 
            position: absolute; 
            top: 10px; 
            right: 10px; 
            color: white; 
            background: rgba(0,0,0,0.7); 
            padding: 15px; 
            border-radius: 5px; 
        }}
        button {{ 
            margin: 2px; 
            padding: 8px 12px; 
            background: #444; 
            color: white; 
            border: none; 
            border-radius: 3px; 
            cursor: pointer; 
            display: block;
            width: 120px;
        }}
        button:hover {{ background: #666; }}
        button.active {{ background: #0a84ff; }}
        canvas {{ display: block; }}
    </style>
</head>
<body>
    <div id='container'>
        <div id='info'>
            <h3>钻孔轨迹三维可视化</h3>
            <p>轨迹点数: <span id='pointCount'>{trajectoryPoints.Count}</span></p>
            <p>深度范围: <span id='depthRange'>0-{(trajectoryPoints.Count > 0 ? trajectoryPoints.Max(p => p.VerticalDepth) : 0):F1}m</span></p>
            <p>最大E位移: {(trajectoryPoints.Count > 0 ? trajectoryPoints.Max(p => p.EastDisplacement) : 0):F1}m</p>
            <p>轨迹总长度: {(trajectoryPoints.Count > 0 ? trajectoryPoints.Last().RodLength : 0):F1}m</p>
            <div id='fpiStats' style='margin-top: 10px; padding-top: 10px; border-top: 1px solid #444;'></div>
        </div>
        <div id='controls'>
            <button onclick='resetView()'>重置视角</button>
            <button onclick='togglePoints()' id='pointsBtn' class='active'>显示轨迹点</button>
            <button onclick='toggleLines()' id='linesBtn' class='active'>显示轨迹线</button>
            <button onclick='toggleAxis()' id='axisBtn' class='active'>显示坐标轴</button>
            <button onclick='changeColorScheme()' id='colorBtn'>深度着色</button>
            <button onclick='exportData()'>导出数据</button>
        </div>
    </div>

    <script>
        // 轨迹数据
        const trajectoryData = {trajectoryDataJson};
        
        // 3D可视化代码（简化版Canvas实现）
        let canvas, ctx;
        let camera = {{x: 25, y: -5, z: -25}};
        let showPoints = true, showLines = true, showAxis = true;
        let colorScheme = 'depth';
        let isDragging = false, lastMouseX = 0, lastMouseY = 0;

        function init() {{
            canvas = document.createElement('canvas');
            canvas.width = window.innerWidth;
            canvas.height = window.innerHeight;
            canvas.style.display = 'block';
            document.getElementById('container').appendChild(canvas);
            
            ctx = canvas.getContext('2d');
            
            // 添加鼠标事件
            canvas.addEventListener('mousedown', onMouseDown);
            canvas.addEventListener('mousemove', onMouseMove);
            canvas.addEventListener('mouseup', onMouseUp);
            canvas.addEventListener('wheel', onWheel);
            window.addEventListener('resize', onResize);
            
            // 计算和显示FPI/UCS统计信息
            updateFpiStats();
            
            render();
        }}

        function updateFpiStats() {{
            const validFpiData = trajectoryData.filter(p => !isNaN(p.fpi));
            const validUcsData = trajectoryData.filter(p => !isNaN(p.ucs));
            
            let statsHtml = '<h4>FPI/UCS统计</h4>';
            
            if (validFpiData.length > 0) {{
                const avgFpi = validFpiData.reduce((sum, p) => sum + p.fpi, 0) / validFpiData.length;
                const maxFpi = Math.max(...validFpiData.map(p => p.fpi));
                const minFpi = Math.min(...validFpiData.map(p => p.fpi));
                
                statsHtml += `<p>FPI有效点: ${{validFpiData.length}}</p>`;
                statsHtml += `<p>FPI平均: ${{avgFpi.toFixed(2)}}</p>`;
                statsHtml += `<p>FPI范围: ${{minFpi.toFixed(2)}} - ${{maxFpi.toFixed(2)}}</p>`;
            }} else {{
                statsHtml += '<p>FPI: 无数据</p>';
            }}
            
            if (validUcsData.length > 0) {{
                const avgUcs = validUcsData.reduce((sum, p) => sum + p.ucs, 0) / validUcsData.length;
                const maxUcs = Math.max(...validUcsData.map(p => p.ucs));
                const minUcs = Math.min(...validUcsData.map(p => p.ucs));
                
                statsHtml += `<p>UCS有效点: ${{validUcsData.length}}</p>`;
                statsHtml += `<p>UCS平均: ${{avgUcs.toFixed(2)}} MPa</p>`;
                statsHtml += `<p>UCS范围: ${{minUcs.toFixed(2)}} - ${{maxUcs.toFixed(2)}} MPa</p>`;
            }} else {{
                statsHtml += '<p>UCS: 无数据</p>';
            }}
            
            document.getElementById('fpiStats').innerHTML = statsHtml;
        }}

        function project3D(x, y, z) {{
            const distance = 100;
            const scale = distance / (distance + z + 100);
            return {{
                x: (x - camera.x) * scale + canvas.width / 2,
                y: (y - camera.y) * scale + canvas.height / 2,
                scale: scale
            }};
        }}

        function getColor(value, scheme) {{
            switch(scheme) {{
                case 'depth':
                    const r = Math.floor(255 * value);
                    const g = Math.floor(255 * (1 - value));
                    return `rgb(${{r}}, ${{g}}, 100)`;
                default:
                    return `hsl(${{360 * value}}, 100%, 50%)`;
            }}
        }}

        function render() {{
            ctx.fillStyle = '#111';
            ctx.fillRect(0, 0, canvas.width, canvas.height);

            // 绘制坐标轴
            if (showAxis) {{
                drawAxes();
            }}

            // 绘制轨迹线
            if (showLines) {{
                drawTrajectoryLine();
            }}

            // 绘制轨迹点
            if (showPoints) {{
                drawTrajectoryPoints();
            }}
        }}

        function drawAxes() {{
            const origin = project3D(0, 0, 0);
            const xAxis = project3D(20, 0, 0);
            const yAxis = project3D(0, 20, 0);
            const zAxis = project3D(0, 0, -20);

            // X轴 - 红色
            ctx.strokeStyle = '#ff4444';
            ctx.lineWidth = 2;
            ctx.beginPath();
            ctx.moveTo(origin.x, origin.y);
            ctx.lineTo(xAxis.x, xAxis.y);
            ctx.stroke();
            
            // Y轴 - 绿色
            ctx.strokeStyle = '#44ff44';
            ctx.beginPath();
            ctx.moveTo(origin.x, origin.y);
            ctx.lineTo(yAxis.x, yAxis.y);
            ctx.stroke();

            // Z轴 - 蓝色
            ctx.strokeStyle = '#4444ff';
            ctx.beginPath();
            ctx.moveTo(origin.x, origin.y);
            ctx.lineTo(zAxis.x, zAxis.y);
            ctx.stroke();

            // 坐标轴标签
            ctx.fillStyle = '#fff';
            ctx.font = '12px Arial';
            ctx.fillText('X(E)', xAxis.x + 5, xAxis.y);
            ctx.fillText('Y(N)', yAxis.x + 5, yAxis.y);
            ctx.fillText('Z(深度)', zAxis.x + 5, zAxis.y);
        }}

        function drawTrajectoryLine() {{
            ctx.strokeStyle = '#ffaa00';
            ctx.lineWidth = 3;
            ctx.beginPath();

            for (let i = 0; i < trajectoryData.length; i++) {{
                const point = trajectoryData[i];
                const projected = project3D(point.x, point.y, point.z);
                
                if (i === 0) {{
                    ctx.moveTo(projected.x, projected.y);
                }} else {{
                    ctx.lineTo(projected.x, projected.y);
                }}
            }}
            ctx.stroke();
        }}

        function drawTrajectoryPoints() {{
            for (let i = 0; i < trajectoryData.length; i++) {{
                const point = trajectoryData[i];
                const projected = project3D(point.x, point.y, point.z);
                const value = i / (trajectoryData.length - 1);
                
                ctx.fillStyle = getColor(value, colorScheme);
                ctx.beginPath();
                ctx.arc(projected.x, projected.y, 4 * projected.scale, 0, Math.PI * 2);
                ctx.fill();

                // 点标签
                if (i % 5 === 0) {{
                    ctx.fillStyle = '#fff';
                    ctx.font = '10px Arial';
                    ctx.fillText(`${{point.depth.toFixed(1)}}m`, projected.x + 5, projected.y - 5);
                }}
            }}
        }}

        // 鼠标事件处理
        function onMouseDown(e) {{
            // 检查是否点击到轨迹点
            const rect = canvas.getBoundingClientRect();
            const mouseX = e.clientX - rect.left;
            const mouseY = e.clientY - rect.top;
            
            let clickedPoint = null;
            let minDistance = 20; // 点击检测范围
            
            for (let i = 0; i < trajectoryData.length; i++) {{
                const point = trajectoryData[i];
                const projected = project3D(point.x, point.y, point.z);
                const distance = Math.sqrt(Math.pow(mouseX - projected.x, 2) + Math.pow(mouseY - projected.y, 2));
                
                if (distance < minDistance) {{
                    minDistance = distance;
                    clickedPoint = {{ data: point, index: i }};
                }}
            }}
            
            if (clickedPoint) {{
                // 显示轨迹点信息
                showPointInfo(clickedPoint.data, clickedPoint.index);
            }} else {{
                // 开始拖拽
                isDragging = true;
                lastMouseX = e.clientX;
                lastMouseY = e.clientY;
            }}
        }}

        function showPointInfo(point, index) {{
            let info = `轨迹点 ${{index}}:\\n`;
            info += `深度: ${{point.depth.toFixed(1)}}m\\n`;
            info += `倾角: ${{point.inc.toFixed(1)}}°\\n`;
            info += `方位角: ${{point.azi.toFixed(1)}}°\\n`;
            info += `坐标: (${{point.x.toFixed(2)}}, ${{point.y.toFixed(2)}}, ${{point.z.toFixed(2)}})\\n`;
            
            if (!isNaN(point.fpi)) {{
                info += `FPI: ${{point.fpi.toFixed(2)}}\\n`;
            }}
            
            if (!isNaN(point.ucs)) {{
                info += `UCS: ${{point.ucs.toFixed(2)}} MPa\\n`;
            }}
            
            if (point.markTime) {{
                info += `标记时间: ${{point.markTime}}\\n`;
            }}
            
            if (!isNaN(point.gravity)) {{
                info += `重力和: ${{point.gravity.toFixed(3)}}`;
            }}
            
            alert(info);
        }}

        function onMouseMove(e) {{
            if (isDragging) {{
                const deltaX = e.clientX - lastMouseX;
                const deltaY = e.clientY - lastMouseY;
                
                camera.x += deltaX * 0.1;
                camera.y += deltaY * 0.1;
                
                lastMouseX = e.clientX;
                lastMouseY = e.clientY;
                render();
            }}
        }}

        function onMouseUp(e) {{
            isDragging = false;
        }}

        function onWheel(e) {{
            camera.z += e.deltaY * 0.01;
            render();
        }}

        function onResize() {{
            canvas.width = window.innerWidth;
            canvas.height = window.innerHeight;
            render();
        }}

        // 控制函数
        function resetView() {{
            camera = {{x: 25, y: -5, z: -25}};
            render();
        }}

        function togglePoints() {{
            showPoints = !showPoints;
            const btn = document.getElementById('pointsBtn');
            btn.classList.toggle('active');
            btn.textContent = showPoints ? '隐藏轨迹点' : '显示轨迹点';
            render();
        }}

        function toggleLines() {{
            showLines = !showLines;
            const btn = document.getElementById('linesBtn');
            btn.classList.toggle('active');
            btn.textContent = showLines ? '隐藏轨迹线' : '显示轨迹线';
            render();
        }}

        function toggleAxis() {{
            showAxis = !showAxis;
            const btn = document.getElementById('axisBtn');
            btn.classList.toggle('active');
            btn.textContent = showAxis ? '隐藏坐标轴' : '显示坐标轴';
            render();
        }}

        function changeColorScheme() {{
            const schemes = ['depth', 'inclination', 'azimuth'];
            const currentIndex = schemes.indexOf(colorScheme);
            colorScheme = schemes[(currentIndex + 1) % schemes.length];
            
            const schemeNames = {{ depth: '深度着色', inclination: '倾角着色', azimuth: '方位着色' }};
            document.getElementById('colorBtn').textContent = schemeNames[colorScheme];
            render();
        }}

        function exportData() {{
            const dataStr = JSON.stringify(trajectoryData, null, 2);
            const blob = new Blob([dataStr], {{type: 'application/json'}});
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = 'trajectory_data.json';
            a.click();
            URL.revokeObjectURL(url);
        }}

        // 初始化
        window.addEventListener('load', init);
    </script>
</body>
</html>";
        }

        /// <summary>
        /// 获取处理结果统计信息
        /// </summary>
        public string GetProcessingReport()
        {
            var report = new System.Text.StringBuilder();
            report.AppendLine("=== 钻井数据统计报告 ===");
            report.AppendLine($"总数据量: {allData.Count} 条");
            report.AppendLine($"有效数据量: {filteredData.Count} 条");
            report.AppendLine($"标记行数量: {yellowSerials.Count} 个");
            report.AppendLine($"轨迹点数量: {trajectoryPoints.Count} 个");
            report.AppendLine($"深度分组数量: {depthGroupCache.Count} 个");
            
            if (filteredData.Count > 0)
            {
                var validFpiData = filteredData.Where(d => !float.IsNaN(d.Fpi)).ToList();
                report.AppendLine($"有效FPI数据: {validFpiData.Count} 条");
                
                if (validFpiData.Count > 0)
                {
                    report.AppendLine($"FPI平均值: {validFpiData.Average(d => d.Fpi):F2}");
                    report.AppendLine($"FPI最大值: {validFpiData.Max(d => d.Fpi):F2}");
                    report.AppendLine($"FPI最小值: {validFpiData.Min(d => d.Fpi):F2}");
                }
                
                report.AppendLine($"钻压平均值: {filteredData.Average(d => d.DrillPressure):F2}");
                report.AppendLine($"扭矩平均值: {filteredData.Average(d => d.Torque):F2}");
                report.AppendLine($"转速平均值: {filteredData.Average(d => d.RotationSpeed):F2}");
            }
            
            if (trajectoryPoints.Count > 0)
            {
                report.AppendLine($"最大E位移: {trajectoryPoints.Max(p => p.EastDisplacement):F2} m");
                report.AppendLine($"最大N位移: {trajectoryPoints.Max(p => p.NorthDisplacement):F2} m");
                report.AppendLine($"最大垂深: {trajectoryPoints.Max(p => p.VerticalDepth):F2} m");
            }
            
            report.AppendLine("=== 报告结束 ===");
            
            return report.ToString();
        }
    }
} 