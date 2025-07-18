using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.Collections;
using System.Threading.Tasks;

// 使用全局别名避免命名冲突
using DrillingDataItem = DrillingData.DrillingData;

namespace DrillingData
{
    // 数据模型 - 使用Unity序列化
    [System.Serializable]
    public class DrillingChartDataPoint
    {
        public string key;
        public float value;
    }

    [System.Serializable]
    public class DrillingChartSeries
    {
        public string name;
        public bool display = true;
        public List<DrillingChartDataPoint> data = new List<DrillingChartDataPoint>();
    }

    [System.Serializable]
    public class DrillingChartXAxis
    {
        public string title;
        public List<string> label = new List<string>();
    }

    [System.Serializable]
    public class DrillingChartData
    {
        public List<DrillingChartSeries> series = new List<DrillingChartSeries>();
        public DrillingChartXAxis xAxis = new DrillingChartXAxis();
    }

    [System.Serializable]
    public class DrillingData
    {
        public int originalIndex;
        public string date;
        public string time;
        public DateTime timestamp;
        public float torque;
        public float drillPressure;
        public float rotationSpeed;
        public float temperature;
        public float inclination;
        public float azimuth;
        public float gravitySum;
        public float magneticStrength;
        public float magneticInclination;
        public float voltage;
        public float depth;
        public float fpi;
        public float ucs;
        public bool isMarked;
    }

    [System.Serializable]
    public class TrajectoryPoint
    {
        public DateTime markTimestamp;
        public float inclination;
        public float azimuth;
        public float gravitySum;
        public float trueAzimuth;
        public float rodLength;
        public float avgInclination;
        public float avgMagneticAzimuth;
        public float eastDisplacement;
        public float northDisplacement;
        public float verticalDepth;
        public float xCoordinate;
        public float lateralDeviation;
        public float hValue;
    }

    // 核心处理器 - 纯C#逻辑
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
        
        // 标记行检测
        public bool UseAdvancedMarkDetection { get; set; } = true;
        public float MarkGravityTarget { get; set; } = 1.0f;
        public float MarkGravityTolerance { get; set; } = 0.001f;
        public int MarkDetectionWindow { get; set; } = 5;
        
        // 可视化设置
        public bool GenerateCharts { get; set; } = true;
        
        // 缓存变量
        private List<DrillingDataItem> allData = new List<DrillingDataItem>();
        private List<DateTime> yellowSerials = new List<DateTime>();
        private List<DrillingDataItem> filteredData = new List<DrillingDataItem>();
        private List<TrajectoryPoint> trajectoryPoints = new List<TrajectoryPoint>();
        private Dictionary<float, List<DrillingDataItem>> depthGroupCache = new Dictionary<float, List<DrillingDataItem>>();
        
        // 事件
        public event Action<string> OnLogMessage;
        public event Action<string, Exception> OnException;

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

        private void ClearCache()
        {
            depthGroupCache.Clear();
            allData.Clear();
            yellowSerials.Clear();
            filteredData.Clear();
            trajectoryPoints.Clear();
        }

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
                
                int estimatedRows = sheet.LastRowNum;
                allData.Capacity = estimatedRows;
                
                for (int row = 1; row <= sheet.LastRowNum; row++)
                {
                    IRow dataRow = sheet.GetRow(row);
                    if (dataRow == null) continue;
                    
                    DrillingDataItem data = new DrillingDataItem
                    {
                        originalIndex = GetCellIntValue(dataRow.GetCell(0)),
                        date = GetCellStringValue(dataRow.GetCell(1)),
                        time = GetCellStringValue(dataRow.GetCell(2)),
                        torque = GetCellFloatValue(dataRow.GetCell(3)),
                        drillPressure = GetCellFloatValue(dataRow.GetCell(4)),
                        rotationSpeed = GetCellFloatValue(dataRow.GetCell(5)),
                        temperature = GetCellFloatValue(dataRow.GetCell(6)),
                        inclination = GetCellFloatValue(dataRow.GetCell(7)),
                        azimuth = GetCellFloatValue(dataRow.GetCell(8)),
                        gravitySum = GetCellFloatValue(dataRow.GetCell(9)),
                        magneticStrength = GetCellFloatValue(dataRow.GetCell(10)),
                        magneticInclination = GetCellFloatValue(dataRow.GetCell(11)),
                        voltage = GetCellFloatValue(dataRow.GetCell(12)),
                        isMarked = false
                    };
                    
                    if (DateTime.TryParse($"{data.date} {data.time}", out DateTime timestamp))
                    {
                        data.timestamp = timestamp;
                    }
                    
                    allData.Add(data);
                }
                
                DetectYellowMarkedRowsEnhanced(sheet);
                workbook.Close();
            }
            
            FilterInvalidData();
            LogMessage($"加载完成：总数据 {allData.Count} 条，过滤后 {filteredData.Count} 条");
        }

        // 添加其他核心方法的实现...
        // 这里省略了具体实现以节省空间，实际代码中需要包含所有方法
        #region Stubbed Core Methods (TODO: Implement real logic)

        // 深度插值（占位实现）
        private void InterpolateDepthByRowOptimized()
        {
            // TODO: 实现深度插值算法
        }

        // FPI 计算（占位实现）
        private void CalculateFPIOptimized()
        {
            // TODO: 实现 FPI 计算
        }

        // 提取轨迹点（占位实现）
        private void ExtractTrajectoryPoints()
        {
            // TODO: 实现轨迹点提取逻辑
        }

        // 计算轨迹（占位实现）
        private void CalculateTrajectory()
        {
            // TODO: 实现轨迹计算逻辑
        }

        // 保存结果（占位实现）
        private void SaveResults()
        {
            // TODO: 实现结果保存逻辑
        }

        // 生成图表数据（占位实现）
        private void GenerateChartDataOptimized()
        {
            // TODO: 实现图表数据生成
        }

        // 检测黄色标记行（占位实现）
        private void DetectYellowMarkedRowsEnhanced(ISheet sheet)
        {
            // TODO: 实现标记行检测逻辑
        }

        // 过滤无效数据（占位实现）
        private void FilterInvalidData()
        {
            // TODO: 实现数据过滤逻辑
        }

        #endregion

        private void LogMessage(string message)
        {
            OnLogMessage?.Invoke(message);
        }
        
        /// <summary>
        /// 获取轨迹点数据
        /// </summary>
        /// <returns>轨迹点列表</returns>
        public List<TrajectoryPoint> GetTrajectoryPoints()
        {
            return new List<TrajectoryPoint>(trajectoryPoints);
        }
        
        /// <summary>
        /// 获取钻井数据
        /// </summary>
        /// <returns>钻井数据列表</returns>
        public List<DrillingDataItem> GetDrillingData()
        {
            return new List<DrillingDataItem>(filteredData);
        }
        
        /// <summary>
        /// 获取处理报告
        /// </summary>
        /// <returns>处理报告字符串</returns>
        public string GetProcessingReport()
        {
            return $"轨迹点数量: {trajectoryPoints.Count}, 钻井数据点: {filteredData.Count}";
        }

        // 辅助方法
        private string GetCellStringValue(ICell cell)
        {
            if (cell == null) return "";
            
            switch (cell.CellType)
            {
                case CellType.String:
                    return cell.StringCellValue;
                case CellType.Numeric:
                    return cell.NumericCellValue.ToString();
                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();
                case CellType.Formula:
                    return cell.StringCellValue;
                default:
                    return "";
            }
        }

        private int GetCellIntValue(ICell cell)
        {
            if (cell == null) return 0;
            
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    return (int)cell.NumericCellValue;
                case CellType.String:
                    if (int.TryParse(cell.StringCellValue, out int result))
                        return result;
                    return 0;
                case CellType.Formula:
                    return (int)cell.NumericCellValue;
                default:
                    return 0;
            }
        }

        private float GetCellFloatValue(ICell cell)
        {
            if (cell == null) return float.NaN;
            
            switch (cell.CellType)
            {
                case CellType.Numeric:
                    return (float)cell.NumericCellValue;
                case CellType.String:
                    if (float.TryParse(cell.StringCellValue, out float result))
                        return result;
                    return float.NaN;
                case CellType.Formula:
                    return (float)cell.NumericCellValue;
                default:
                    return float.NaN;
            }
        }

        // 省略其他方法的完整实现...
        // 在实际项目中需要包含所有数据处理方法
    }
    // Unity MonoBehaviour 包装器
    public class UnityDrillingDataProcessor : MonoBehaviour
{
    [Header("文件设置")]
    public string inputExcelPath = "Assets/Data/J16原始.xlsx";
    public string outputPath = "Assets/Data/";
    public string jsonOutputPath = "Assets/Json/chartsdata/porous/";
    
    [Header("处理参数")]
    public float depthStep = 1.5f;
    public float depthInterval = 0.2f;
    public float magneticDeclination = 0f;
    public float surveyLineTrueAzimuth = 90f;
    public float initialH = 0f;
    
    [Header("过滤条件")]
    public float minRotationSpeed = 10f;
    public float minDrillPressure = 200f;
    public float minTorque = 200f;
    public float gravityMin = 0.98f;
    public float gravityMax = 1.02f;
    
    [Header("标记行检测")]
    public bool useAdvancedMarkDetection = true;
    public float markGravityTarget = 1.0f;
    public float markGravityTolerance = 0.001f;
    public int markDetectionWindow = 5;
    
    [Header("可视化设置")]
    public bool generateCharts = true;
    public string chartTitle = "钻井参数分析";
    
    [Header("自动触发设置")]
    public bool enableAutoTrigger = false;
    public bool monitorFileChanges = true;
    public float fileChangeDelay = 2f;
    public float minProcessInterval = 30f;
    
    private DrillingDataProcessorCore coreProcessor;
    private FileSystemWatcher fileWatcher;
    private DateTime lastProcessTime;
    private bool isProcessing = false;
    private Coroutine fileChangeCoroutine;

    void Start()
    {
        InitializeCoreProcessor();
        
        if (enableAutoTrigger && monitorFileChanges)
        {
            InitializeFileWatcher();
        }
    }

    void OnDestroy()
    {
        StopFileWatcher();
    }

    private void InitializeCoreProcessor()
    {
        coreProcessor = new DrillingDataProcessorCore();
        
        // 配置核心处理器
        coreProcessor.InputExcelPath = inputExcelPath;
        coreProcessor.OutputPath = outputPath;
        coreProcessor.JsonOutputPath = jsonOutputPath;
        coreProcessor.DepthStep = depthStep;
        coreProcessor.DepthInterval = depthInterval;
        coreProcessor.MagneticDeclination = magneticDeclination;
        coreProcessor.SurveyLineTrueAzimuth = surveyLineTrueAzimuth;
        coreProcessor.InitialH = initialH;
        coreProcessor.MinRotationSpeed = minRotationSpeed;
        coreProcessor.MinDrillPressure = minDrillPressure;
        coreProcessor.MinTorque = minTorque;
        coreProcessor.GravityMin = gravityMin;
        coreProcessor.GravityMax = gravityMax;
        coreProcessor.UseAdvancedMarkDetection = useAdvancedMarkDetection;
        coreProcessor.MarkGravityTarget = markGravityTarget;
        coreProcessor.MarkGravityTolerance = markGravityTolerance;
        coreProcessor.MarkDetectionWindow = markDetectionWindow;
        coreProcessor.GenerateCharts = generateCharts;
        
        // 订阅事件
        coreProcessor.OnLogMessage += message => Debug.Log($"[钻井数据处理器] {message}");
        coreProcessor.OnException += (message, ex) => 
        {
            Debug.LogError($"[钻井数据处理器] {message}");
            Debug.LogException(ex);
        };
    }

    [ContextMenu("处理钻井数据并生成图表")]
    public void ProcessDrillingDataAndGenerateCharts()
    {
        if (isProcessing)
        {
            Debug.LogWarning("数据处理正在进行中，请稍后再试");
            return;
        }

        StartCoroutine(ProcessDataCoroutine());
    }

    private IEnumerator ProcessDataCoroutine()
    {
        isProcessing = true;
        lastProcessTime = DateTime.Now;
        
        try
        {
            // 更新核心处理器配置
            UpdateCoreProcessorConfig();
            
            // 在协程中执行处理
            bool success = false;
            bool taskCompleted = false;
            
            Task.Run(() =>
            {
                try
                {
                    success = coreProcessor.ProcessDrillingDataAndGenerateCharts();
                }
                catch (Exception e)
                {
                    Debug.LogError($"数据处理任务异常: {e.Message}");
                    success = false;
                }
                finally
                {
                    taskCompleted = true;
                }
            });
            
            // 等待处理完成
            while (!taskCompleted)
            {
                yield return new WaitForSeconds(0.1f);
            }
            
            if (success)
            {
                Debug.Log("数据处理完成！");
                Debug.Log(coreProcessor.GetProcessingReport());
            }
            else
            {
                Debug.LogError("数据处理失败！");
            }
        }
        finally
        {
            isProcessing = false;
        }
    }

    private void UpdateCoreProcessorConfig()
    {
        coreProcessor.InputExcelPath = inputExcelPath;
        coreProcessor.OutputPath = outputPath;
        coreProcessor.JsonOutputPath = jsonOutputPath;
        coreProcessor.DepthStep = depthStep;
        coreProcessor.DepthInterval = depthInterval;
        coreProcessor.MagneticDeclination = magneticDeclination;
        coreProcessor.SurveyLineTrueAzimuth = surveyLineTrueAzimuth;
        coreProcessor.InitialH = initialH;
        coreProcessor.MinRotationSpeed = minRotationSpeed;
        coreProcessor.MinDrillPressure = minDrillPressure;
        coreProcessor.MinTorque = minTorque;
        coreProcessor.GravityMin = gravityMin;
        coreProcessor.GravityMax = gravityMax;
        coreProcessor.UseAdvancedMarkDetection = useAdvancedMarkDetection;
        coreProcessor.MarkGravityTarget = markGravityTarget;
        coreProcessor.MarkGravityTolerance = markGravityTolerance;
        coreProcessor.MarkDetectionWindow = markDetectionWindow;
        coreProcessor.GenerateCharts = generateCharts;
    }

    private void InitializeFileWatcher()
    {
        try
        {
            string directory = Path.GetDirectoryName(inputExcelPath);
            string fileName = Path.GetFileName(inputExcelPath);
            
            if (!Directory.Exists(directory))
            {
                Debug.LogWarning($"监控目录不存在: {directory}");
                return;
            }

            if (fileWatcher != null)
            {
                StopFileWatcher();
            }

            fileWatcher = new FileSystemWatcher(directory, fileName);
            fileWatcher.Created += OnFileChanged;
            fileWatcher.Changed += OnFileChanged;
            fileWatcher.EnableRaisingEvents = true;
            
            Debug.Log($"开始监控文件变化: {inputExcelPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"初始化文件监控失败: {e.Message}");
        }
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (ShouldTriggerProcessing())
        {
            Debug.Log($"检测到文件变化: {e.Name}");
            
            if (fileChangeCoroutine != null)
            {
                StopCoroutine(fileChangeCoroutine);
            }
            
            fileChangeCoroutine = StartCoroutine(TriggerFileChangeProcessing());
        }
    }

    private IEnumerator TriggerFileChangeProcessing()
    {
        yield return new WaitForSeconds(fileChangeDelay);
        
        if (File.Exists(inputExcelPath))
        {
            try
            {
                using (FileStream fs = File.OpenRead(inputExcelPath))
                {
                    // 文件可以正常打开，说明写入完成
                }
                
                Debug.Log($"文件变化触发数据处理: {inputExcelPath}");
                ProcessDrillingDataAndGenerateCharts();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"文件仍在写入中，跳过处理: {e.Message}");
            }
        }
    }

    private bool ShouldTriggerProcessing()
    {
        if (isProcessing) return false;
        if ((DateTime.Now - lastProcessTime).TotalSeconds < minProcessInterval) return false;
        if (!File.Exists(inputExcelPath)) return false;
        return true;
    }

    private void StopFileWatcher()
    {
        if (fileWatcher != null)
        {
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Dispose();
            fileWatcher = null;
            Debug.Log("文件监控已停止");
        }
        
        if (fileChangeCoroutine != null)
        {
            StopCoroutine(fileChangeCoroutine);
            fileChangeCoroutine = null;
        }
    }

    [ContextMenu("启动文件监控")]
    public void StartFileWatcher()
    {
        if (!enableAutoTrigger)
        {
            enableAutoTrigger = true;
            Debug.Log("已启用自动触发功能");
        }
        
        InitializeFileWatcher();
    }

    [ContextMenu("停止文件监控")]
    public void StopFileWatcherManual()
    {
        StopFileWatcher();
        enableAutoTrigger = false;
        Debug.Log("已停用自动触发功能");
    }

    [ContextMenu("获取处理状态")]
    public void GetProcessingStatus()
    {
        Debug.Log($"处理状态: {(isProcessing ? "处理中" : "空闲")}");
        Debug.Log($"文件监控: {(fileWatcher?.EnableRaisingEvents == true ? "已启用" : "已停用")}");
        Debug.Log($"上次处理时间: {lastProcessTime:yyyy-MM-dd HH:mm:ss}");
        
        if (coreProcessor != null)
        {
            Debug.Log("核心处理器已初始化");
        }
        else
        {
            Debug.LogWarning("核心处理器未初始化");
        }
    }

    [ContextMenu("生成数据统计报告")]
    public void GenerateDataReport()
    {
        if (coreProcessor != null)
        {
            Debug.Log(coreProcessor.GetProcessingReport());
        }
        else
        {
            Debug.LogWarning("核心处理器未初始化，无法生成报告");
        }
    }
    
    /// <summary>
    /// 获取轨迹点数据
    /// </summary>
    /// <returns>轨迹点列表</returns>
    public List<TrajectoryPoint> GetTrajectoryPoints()
    {
        return coreProcessor != null ? coreProcessor.GetTrajectoryPoints() : new List<TrajectoryPoint>();
    }
    
    /// <summary>
    /// 获取处理后的钻井数据
    /// </summary>
    /// <returns>钻井数据列表</returns>
    public List<DrillingDataItem> GetDrillingData()
    {
        if (coreProcessor != null)
        {
            return coreProcessor.GetDrillingData();
        }
        return new List<DrillingDataItem>();
    }

    public string GetProcessingReport() => coreProcessor != null ? coreProcessor.GetProcessingReport() : string.Empty;
    }
} 