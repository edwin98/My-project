using System;
using System.Collections.Generic;
using System.Linq;
using DrillingDataProcessor.Core.Interfaces;
using DrillingDataProcessor.Models;

namespace DrillingDataProcessor.Core.Modules
{
    /// <summary>
    /// 轨迹计算器实现
    /// </summary>
    public class TrajectoryCalculatorImpl : ITrajectoryCalculator
    {
        public event Action<string>? OnLogMessage;
        
        /// <summary>
        /// 提取轨迹点
        /// </summary>
        public List<TrajectoryPoint> ExtractTrajectoryPoints(List<DrillingData> allData, List<DateTime> markedTimestamps)
        {
            LogMessage("正在提取轨迹点...");
            
            var rawData = allData.OrderBy(d => d.Timestamp).ToList();
            var trajectoryPoints = new List<TrajectoryPoint>();
            
            foreach (var markTimestamp in markedTimestamps)
            {
                var markIndex = rawData.FindIndex(d => d.Timestamp == markTimestamp);
                if (markIndex >= 0)
                {
                    // 从标记行的下一行开始查找
                    int searchStartIndex = markIndex + 1;
                    TrajectoryPoint? trajectoryPoint = null;
                    
                    // 向后搜索，找到第一个重力值为0.999或1.0的行
                    for (int i = searchStartIndex; i < rawData.Count; i++)
                    {
                        var data = rawData[i];
                        
                        // 检查重力值是否为0.999或1.0（允许小的浮点误差）
                        if (Math.Abs(data.GravitySum - 0.999f) < 0.001f || 
                            Math.Abs(data.GravitySum - 1.0f) < 0.001f)
                        {
                            trajectoryPoint = new TrajectoryPoint
                            {
                                MarkTimestamp = markTimestamp,
                                Inclination = data.Inclination,
                                Azimuth = data.Azimuth,
                                GravitySum = data.GravitySum
                            };
                            
                            LogMessage($"标记时间 {markTimestamp:yyyy-MM-dd HH:mm:ss} 后找到轨迹点，重力值: {data.GravitySum:F3}");
                            break; // 找到第一个符合条件的点就停止
                        }
                    }
                    
                    if (trajectoryPoint != null)
                    {
                        trajectoryPoints.Add(trajectoryPoint);
                    }
                    else
                    {
                        LogMessage($"警告: 标记时间 {markTimestamp:yyyy-MM-dd HH:mm:ss} 后未找到合适的轨迹点");
                    }
                }
            }
            
            LogMessage($"提取到 {trajectoryPoints.Count} 个轨迹点");
            return trajectoryPoints;
        }
        
        /// <summary>
        /// 计算三维轨迹
        /// </summary>
        public List<TrajectoryPoint> CalculateTrajectory(List<TrajectoryPoint> trajectoryPoints, TrajectoryCalculationParameters parameters)
        {
            LogMessage("正在计算轨迹...");
            
            for (int i = 0; i < trajectoryPoints.Count; i++)
            {
                var point = trajectoryPoints[i];
                
                // 基础计算
                point.TrueAzimuth = point.Azimuth - parameters.MagneticDeclination;
                point.RodLength = parameters.DepthStep * i; // 杆长 = 深度步长 * 索引
                
                // 计算平均值（当前点和前一点的平均）
                if (i > 0)
                {
                    var prevPoint = trajectoryPoints[i - 1];
                    point.AvgInclination = (point.Inclination + prevPoint.Inclination) / 2f;
                    point.AvgMagneticAzimuth = (point.Azimuth + prevPoint.Azimuth) / 2f;
                }
                else
                {
                    // 第一个点使用自身值
                    point.AvgInclination = point.Inclination;
                    point.AvgMagneticAzimuth = point.Azimuth;
                }
                
                // 计算位移（从第二个点开始累积计算）
                if (i > 0)
                {
                    var prevPoint = trajectoryPoints[i - 1];
                    
                    // 转换为弧度
                    float avgInclinationRad = point.AvgInclination * MathF.PI / 180f;
                    float avgMagneticAzimuthRad = point.AvgMagneticAzimuth * MathF.PI / 180f;
                    float surveyLineRad = parameters.SurveyLineTrueAzimuth * MathF.PI / 180f;
                    
                    // 计算三角函数值
                    float cosInclination = MathF.Cos(avgInclinationRad);
                    float sinInclination = MathF.Sin(avgInclinationRad);
                    float sinAzimuth = MathF.Sin(avgMagneticAzimuthRad);
                    float cosAzimuth = MathF.Cos(avgMagneticAzimuthRad);
                    
                    // 计算相对于勘探线的方位角
                    float relativeAzimuthRad = avgMagneticAzimuthRad - surveyLineRad;
                    float cosRelative = MathF.Cos(relativeAzimuthRad);
                    float sinRelative = MathF.Sin(relativeAzimuthRad);
                    
                    // 累积计算位移（参考Python实现）
                    point.EastDisplacement = prevPoint.EastDisplacement + parameters.DepthStep * cosInclination * sinAzimuth;
                    point.NorthDisplacement = prevPoint.NorthDisplacement + parameters.DepthStep * cosInclination * cosAzimuth;
                    point.VerticalDepth = prevPoint.VerticalDepth + parameters.DepthStep * cosInclination;
                    point.XCoordinate = prevPoint.XCoordinate + parameters.DepthStep * cosInclination * cosRelative;
                    point.LateralDeviation = prevPoint.LateralDeviation + parameters.DepthStep * cosInclination * sinRelative;
                    point.HValue = parameters.InitialH + point.VerticalDepth;
                }
                else
                {
                    // 第一个点的初始值
                    point.EastDisplacement = 0f;
                    point.NorthDisplacement = 0f;
                    point.VerticalDepth = 0f;
                    point.XCoordinate = 0f;
                    point.LateralDeviation = 0f;
                    point.HValue = parameters.InitialH;
                }
            }
            
            LogMessage($"轨迹计算完成，处理了 {trajectoryPoints.Count} 个轨迹点");
            return trajectoryPoints;
        }
        
        /// <summary>
        /// 获取轨迹统计信息
        /// </summary>
        public TrajectoryStatistics GetTrajectoryStatistics(List<TrajectoryPoint> trajectoryPoints)
        {
            var stats = new TrajectoryStatistics();
            
            if (!trajectoryPoints.Any())
            {
                return stats;
            }
            
            stats.TotalPoints = trajectoryPoints.Count;
            stats.MaxEastDisplacement = trajectoryPoints.Max(p => Math.Abs(p.EastDisplacement));
            stats.MaxNorthDisplacement = trajectoryPoints.Max(p => Math.Abs(p.NorthDisplacement));
            stats.MaxVerticalDepth = trajectoryPoints.Max(p => p.VerticalDepth);
            stats.MaxInclination = trajectoryPoints.Max(p => p.Inclination);
            stats.MaxAzimuth = trajectoryPoints.Max(p => p.Azimuth);
            
            // 计算总长度（相邻点之间的距离之和）
            float totalLength = 0f;
            for (int i = 1; i < trajectoryPoints.Count; i++)
            {
                var prev = trajectoryPoints[i - 1];
                var curr = trajectoryPoints[i];
                
                float dx = curr.EastDisplacement - prev.EastDisplacement;
                float dy = curr.NorthDisplacement - prev.NorthDisplacement;
                float dz = curr.VerticalDepth - prev.VerticalDepth;
                
                totalLength += MathF.Sqrt(dx * dx + dy * dy + dz * dz);
            }
            stats.TotalLength = totalLength;
            
            return stats;
        }
        
        private void LogMessage(string message)
        {
            OnLogMessage?.Invoke(message);
        }
    }
} 