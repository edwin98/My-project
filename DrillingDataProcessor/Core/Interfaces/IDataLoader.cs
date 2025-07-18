using System.Collections.Generic;
using DrillingDataProcessor.Models;

namespace DrillingDataProcessor.Core.Interfaces
{
    /// <summary>
    /// 数据加载模块接口
    /// </summary>
    public interface IDataLoader
    {
        /// <summary>
        /// 从Excel文件加载钻井数据
        /// </summary>
        /// <param name="filePath">Excel文件路径</param>
        /// <returns>加载的钻井数据列表</returns>
        List<DrillingData> LoadFromExcel(string filePath);
        
        /// <summary>
        /// 验证数据完整性
        /// </summary>
        /// <param name="data">待验证的数据</param>
        /// <returns>验证结果</returns>
        DataValidationResult ValidateData(List<DrillingData> data);
    }
    
    /// <summary>
    /// 数据验证结果
    /// </summary>
    public class DataValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public int TotalRecords { get; set; }
        public int ValidRecords { get; set; }
        public double CompletionRate => TotalRecords > 0 ? (double)ValidRecords / TotalRecords : 0;
    }
} 