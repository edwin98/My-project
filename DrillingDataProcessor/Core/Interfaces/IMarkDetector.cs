using System;
using System.Collections.Generic;
using NPOI.SS.UserModel;

namespace DrillingDataProcessor.Core.Interfaces
{
    /// <summary>
    /// 标记行检测模块接口
    /// </summary>
    public interface IMarkDetector
    {
        /// <summary>
        /// 检测Excel表格中的黄色标记行
        /// </summary>
        /// <param name="sheet">Excel工作表</param>
        /// <returns>标记行的时间戳列表</returns>
        List<DateTime> DetectMarkedRows(ISheet sheet);
        
        /// <summary>
        /// 检测结果统计
        /// </summary>
        MarkDetectionResult GetDetectionResult();
    }
    
    /// <summary>
    /// 标记检测结果
    /// </summary>
    public class MarkDetectionResult
    {
        public int TotalRowsScanned { get; set; }
        public int MarkedRowsFound { get; set; }
        public List<int> MarkedRowIndices { get; set; } = new List<int>();
        public double DetectionRate => TotalRowsScanned > 0 ? (double)MarkedRowsFound / TotalRowsScanned : 0;
        public List<string> DetectionLog { get; set; } = new List<string>();
    }
} 