using System;
using System.Collections.Generic;
using DrillingDataProcessor.Models;

namespace DrillingDataProcessor.Core.Interfaces
{
    /// <summary>
    /// 轨迹计算模块接口
    /// </summary>
    public interface ITrajectoryCalculator
    {
        /// <summary>
        /// 提取轨迹点
        /// </summary>
        /// <param name="allData">所有钻井数据</param>
        /// <param name="markedTimestamps">标记行时间戳</param>
        /// <returns>轨迹点列表</returns>
        List<TrajectoryPoint> ExtractTrajectoryPoints(List<DrillingData> allData, List<DateTime> markedTimestamps);
        
        /// <summary>
        /// 计算三维轨迹
        /// </summary>
        /// <param name="trajectoryPoints">轨迹点</param>
        /// <param name="parameters">计算参数</param>
        /// <returns>计算后的轨迹点</returns>
        List<TrajectoryPoint> CalculateTrajectory(List<TrajectoryPoint> trajectoryPoints, TrajectoryCalculationParameters parameters);
        
        /// <summary>
        /// 获取轨迹统计信息
        /// </summary>
        /// <param name="trajectoryPoints">轨迹点</param>
        /// <returns>统计信息</returns>
        TrajectoryStatistics GetTrajectoryStatistics(List<TrajectoryPoint> trajectoryPoints);
    }
    
    /// <summary>
    /// 轨迹计算参数
    /// </summary>
    public class TrajectoryCalculationParameters
    {
        public float DepthStep { get; set; } = 1.5f;
        public float MagneticDeclination { get; set; } = 0f;
        public float SurveyLineTrueAzimuth { get; set; } = 90f;
        public float InitialH { get; set; } = 0f;
    }
    
    /// <summary>
    /// 轨迹统计信息
    /// </summary>
    public class TrajectoryStatistics
    {
        public int TotalPoints { get; set; }
        public float MaxEastDisplacement { get; set; }
        public float MaxNorthDisplacement { get; set; }
        public float MaxVerticalDepth { get; set; }
        public float TotalLength { get; set; }
        public float MaxInclination { get; set; }
        public float MaxAzimuth { get; set; }
        public string Summary => $"轨迹点: {TotalPoints}, 最大垂深: {MaxVerticalDepth:F2}m, 总长度: {TotalLength:F2}m";
    }
} 