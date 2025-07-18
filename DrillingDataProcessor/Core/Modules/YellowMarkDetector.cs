using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using DrillingDataProcessor.Core.Interfaces;
using DrillingDataProcessor.Models;

namespace DrillingDataProcessor.Core.Modules
{
    /// <summary>
    /// 黄色标记行检测器实现
    /// </summary>
    public class YellowMarkDetector : IMarkDetector
    {
        private readonly List<DrillingData> _allData;
        private MarkDetectionResult _lastResult;
        
        public event Action<string>? OnLogMessage;
        
        public YellowMarkDetector(List<DrillingData> allData)
        {
            _allData = allData ?? throw new ArgumentNullException(nameof(allData));
            _lastResult = new MarkDetectionResult();
        }
        
        /// <summary>
        /// 检测Excel表格中的黄色标记行
        /// </summary>
        public List<DateTime> DetectMarkedRows(ISheet sheet)
        {
            var yellowSerials = new List<DateTime>();
            var yellowRowIndices = new List<int>();
            
            _lastResult = new MarkDetectionResult();
            _lastResult.TotalRowsScanned = sheet.LastRowNum + 1;
            
            LogMessage("开始检测黄色标记行...");
            
            // 遍历所有行，检测黄色填充
            for (int rowIndex = 0; rowIndex <= sheet.LastRowNum; rowIndex++)
            {
                IRow row = sheet.GetRow(rowIndex);
                if (row == null) continue;

                // 只检查每行的第一个单元格
                ICell cell = row.GetCell(0);
                if (cell != null && IsYellowCell(cell))
                {
                    yellowRowIndices.Add(rowIndex);
                    _lastResult.MarkedRowIndices.Add(rowIndex);
                    
                    string logMessage = $"检测到黄色行: Excel行号 {rowIndex + 1}, 数据索引 {rowIndex}";
                    LogMessage(logMessage);
                    _lastResult.DetectionLog.Add(logMessage);
                }
            }
            
            _lastResult.MarkedRowsFound = yellowRowIndices.Count;
            LogMessage($"总共检测到 {yellowRowIndices.Count} 个黄色行");
            
            // 将行索引转换为时间戳
            foreach (var rowIndex in yellowRowIndices)
            {
                int dataIndex = rowIndex; // Excel行号直接对应数据索引（无标题行）
                if (dataIndex >= 0 && dataIndex < _allData.Count)
                {
                    yellowSerials.Add(_allData[dataIndex].Timestamp);
                }
            }
            
            return yellowSerials;
        }
        
        /// <summary>
        /// 获取检测结果统计
        /// </summary>
        public MarkDetectionResult GetDetectionResult()
        {
            return _lastResult;
        }
        
        /// <summary>
        /// 检查单元格是否为黄色填充
        /// </summary>
        private bool IsYellowCell(ICell cell)
        {
            try
            {
                if (cell.CellStyle?.FillForegroundColor != null)
                {
                    var colorIndex = cell.CellStyle.FillForegroundColor;
                    
                    // NPOI中常见的黄色索引值
                    // 13 = 亮黄色, 43 = 黄色, 44 = 浅黄色
                    if (colorIndex != 64)
                    {
                        return true;
                    }
                    
                    // 检查RGB颜色（如果是XSSFWorkbook）
                    if (cell.Sheet.Workbook is XSSFWorkbook && cell.CellStyle is XSSFCellStyle xssfStyle)
                    {
                        var fillColor = xssfStyle.FillForegroundColorColor;
                        if (fillColor != null)
                        {
                            // 检查是否为黄色RGB值 (FFFFFF00, FFFF00等)
                            string colorHex = fillColor.ToString()?.ToUpper() ?? "";
                            if (colorHex.Contains("FFFF00") || colorHex.Contains("FFFFFF00"))
                            {
                                return true;
                            }
                        }
                    }
                }
                
                return false;
            }
            catch (Exception ex)
            {
                LogMessage($"检查单元格颜色时出错: {ex.Message}");
                return false;
            }
        }
        
        private void LogMessage(string message)
        {
            OnLogMessage?.Invoke(message);
        }
    }
} 