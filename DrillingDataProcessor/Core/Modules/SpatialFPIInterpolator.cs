using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using DrillingDataProcessor.Models;

namespace DrillingDataProcessor.Core.Modules
{
    /// <summary>
    /// 空间FPI插值器 - 对轨迹点涉及的立方体空间进行FPI值插值
    /// </summary>
    public class SpatialFPIInterpolator
    {
        /// <summary>
        /// 三维空间点
        /// </summary>
        public class Point3D
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float FPI { get; set; }

            public Point3D(float x, float y, float z, float fpi = float.NaN)
            {
                X = x;
                Y = y;
                Z = z;
                FPI = fpi;
            }

            /// <summary>
            /// 计算到另一点的距离
            /// </summary>
            public float DistanceTo(Point3D other)
            {
                return (float)Math.Sqrt(
                    Math.Pow(X - other.X, 2) + 
                    Math.Pow(Y - other.Y, 2) + 
                    Math.Pow(Z - other.Z, 2)
                );
            }
        }

        /// <summary>
        /// 立方体空间定义
        /// </summary>
        public class CubicSpace
        {
            public Point3D MinBounds { get; set; }
            public Point3D MaxBounds { get; set; }
            public float Resolution { get; set; } // 空间分辨率（米）
            public List<Point3D> GridPoints { get; set; } = new List<Point3D>();

            public CubicSpace(Point3D minBounds, Point3D maxBounds, float resolution)
            {
                MinBounds = minBounds;
                MaxBounds = maxBounds;
                Resolution = resolution;
                GenerateGrid();
            }

            /// <summary>
            /// 生成网格点
            /// </summary>
            private void GenerateGrid()
            {
                GridPoints.Clear();
                
                for (float x = MinBounds.X; x <= MaxBounds.X; x += Resolution)
                {
                    for (float y = MinBounds.Y; y <= MaxBounds.Y; y += Resolution)
                    {
                        for (float z = MinBounds.Z; z <= MaxBounds.Z; z += Resolution)
                        {
                            GridPoints.Add(new Point3D(x, y, z));
                        }
                    }
                }
            }

            /// <summary>
            /// 获取网格维度信息
            /// </summary>
            public (int xCount, int yCount, int zCount) GetGridDimensions()
            {
                int xCount = (int)Math.Round((MaxBounds.X - MinBounds.X) / Resolution) + 1;
                int yCount = (int)Math.Round((MaxBounds.Y - MinBounds.Y) / Resolution) + 1;
                int zCount = (int)Math.Round((MaxBounds.Z - MinBounds.Z) / Resolution) + 1;
                return (xCount, yCount, zCount);
            }
        }

        /// <summary>
        /// 插值结果
        /// </summary>
        public class InterpolationResult
        {
            public CubicSpace Space { get; set; }
            public List<Point3D> InterpolatedPoints { get; set; } = new List<Point3D>();
            public int ValidPointsCount { get; set; }
            public float MinFPI { get; set; }
            public float MaxFPI { get; set; }
            public float AvgFPI { get; set; }
            public DateTime ProcessedTime { get; set; }
        }

        // 配置参数
        public float SpatialResolution { get; set; } = 0.5f; // 空间分辨率，默认0.5米
        public float InfluenceRadius { get; set; } = 5.0f;    // 影响半径，默认5米
        public int MinNeighborPoints { get; set; } = 3;       // 最小邻近点数量
        public float BoundaryExpansion { get; set; } = 2.0f;  // 边界扩展，默认2米

        // 事件
        public event Action<string> OnLogMessage;

        /// <summary>
        /// 根据轨迹点数据进行空间FPI插值
        /// </summary>
        /// <param name="trajectoryPoints">轨迹点列表</param>
        /// <returns>插值结果</returns>
        public InterpolationResult InterpolateFPI(List<TrajectoryPoint> trajectoryPoints)
        {
            LogMessage("开始进行空间FPI插值...");

            // 1. 过滤有效的FPI轨迹点
            var validTrajectoryPoints = FilterValidFPIPoints(trajectoryPoints);
            if (validTrajectoryPoints.Count == 0)
            {
                throw new InvalidOperationException("没有找到有效的FPI轨迹点数据");
            }

            LogMessage($"找到 {validTrajectoryPoints.Count} 个有效FPI轨迹点");

            // 2. 定义立方体空间范围
            var cubicSpace = DefineCubicSpace(validTrajectoryPoints);
            LogMessage($"定义立方体空间: {cubicSpace.GridPoints.Count} 个网格点");

            // 3. 执行空间插值
            var interpolatedPoints = PerformSpatialInterpolation(cubicSpace, validTrajectoryPoints);
            LogMessage($"完成空间插值，生成 {interpolatedPoints.Count} 个插值点");

            // 4. 生成结果
            var result = new InterpolationResult
            {
                Space = cubicSpace,
                InterpolatedPoints = interpolatedPoints,
                ValidPointsCount = interpolatedPoints.Count(p => !float.IsNaN(p.FPI)),
                ProcessedTime = DateTime.Now
            };

            // 计算统计信息
            var validInterpolatedPoints = interpolatedPoints.Where(p => !float.IsNaN(p.FPI)).ToList();
            if (validInterpolatedPoints.Count > 0)
            {
                result.MinFPI = validInterpolatedPoints.Min(p => p.FPI);
                result.MaxFPI = validInterpolatedPoints.Max(p => p.FPI);
                result.AvgFPI = validInterpolatedPoints.Average(p => p.FPI);
            }

            LogMessage($"插值完成: 有效点 {result.ValidPointsCount}, FPI范围 [{result.MinFPI:F2}, {result.MaxFPI:F2}]");
            return result;
        }

        /// <summary>
        /// 过滤有效的FPI轨迹点
        /// </summary>
        private List<Point3D> FilterValidFPIPoints(List<TrajectoryPoint> trajectoryPoints)
        {
            var validPoints = new List<Point3D>();

            foreach (var tp in trajectoryPoints)
            {
                // 这里需要从轨迹点获取FPI值，假设我们有相关联的FPI数据
                // 实际实现中可能需要根据深度匹配FPI值
                var fpiValue = GetFPIForTrajectoryPoint(tp);
                
                if (!float.IsNaN(fpiValue) && fpiValue > 0)
                {
                    validPoints.Add(new Point3D(
                        tp.EastDisplacement,
                        tp.NorthDisplacement, 
                        tp.VerticalDepth,
                        fpiValue
                    ));
                }
            }

            return validPoints;
        }

        /// <summary>
        /// 获取轨迹点对应的FPI值
        /// 这里可以根据实际需求修改获取FPI的逻辑
        /// </summary>
        private float GetFPIForTrajectoryPoint(TrajectoryPoint trajectoryPoint)
        {
            // 如果轨迹点直接包含FPI信息，可以直接返回
            // 否则需要根据深度等信息从其他数据源匹配
            // 这里假设通过某种方式获取到FPI值
            
            // 临时实现：可以从外部传入FPI数据或者使用其他匹配逻辑
            // 实际项目中可能需要传入额外的FPI数据集
            return float.NaN; // 需要根据实际情况实现
        }

        /// <summary>
        /// 定义立方体空间
        /// </summary>
        private CubicSpace DefineCubicSpace(List<Point3D> trajectoryPoints)
        {
            var minX = trajectoryPoints.Min(p => p.X) - BoundaryExpansion;
            var maxX = trajectoryPoints.Max(p => p.X) + BoundaryExpansion;
            var minY = trajectoryPoints.Min(p => p.Y) - BoundaryExpansion;
            var maxY = trajectoryPoints.Max(p => p.Y) + BoundaryExpansion;
            var minZ = trajectoryPoints.Min(p => p.Z) - BoundaryExpansion;
            var maxZ = trajectoryPoints.Max(p => p.Z) + BoundaryExpansion;

            var minBounds = new Point3D(minX, minY, minZ);
            var maxBounds = new Point3D(maxX, maxY, maxZ);

            return new CubicSpace(minBounds, maxBounds, SpatialResolution);
        }

        /// <summary>
        /// 执行空间插值
        /// </summary>
        private List<Point3D> PerformSpatialInterpolation(CubicSpace cubicSpace, List<Point3D> knownPoints)
        {
            var result = new List<Point3D>();

            foreach (var gridPoint in cubicSpace.GridPoints)
            {
                var interpolatedFPI = InterpolateAtPoint(gridPoint, knownPoints);
                result.Add(new Point3D(gridPoint.X, gridPoint.Y, gridPoint.Z, interpolatedFPI));
            }

            return result;
        }

        /// <summary>
        /// 在指定点进行插值计算
        /// 使用反距离加权插值(IDW)方法
        /// </summary>
        private float InterpolateAtPoint(Point3D targetPoint, List<Point3D> knownPoints)
        {
            // 找到影响范围内的点
            var nearbyPoints = knownPoints
                .Where(p => p.DistanceTo(targetPoint) <= InfluenceRadius)
                .OrderBy(p => p.DistanceTo(targetPoint))
                .ToList();

            if (nearbyPoints.Count < MinNeighborPoints)
            {
                return float.NaN; // 附近没有足够的点
            }

            // 检查是否有完全重合的点
            var exactMatch = nearbyPoints.FirstOrDefault(p => p.DistanceTo(targetPoint) < 0.001f);
            if (exactMatch != null)
            {
                return exactMatch.FPI;
            }

            // 反距离加权插值 (IDW)
            float weightedSum = 0f;
            float weightSum = 0f;
            float power = 2f; // IDW指数

            foreach (var point in nearbyPoints)
            {
                float distance = point.DistanceTo(targetPoint);
                if (distance > 0)
                {
                    float weight = 1f / (float)Math.Pow(distance, power);
                    weightedSum += weight * point.FPI;
                    weightSum += weight;
                }
            }

            return weightSum > 0 ? weightedSum / weightSum : float.NaN;
        }

        /// <summary>
        /// 重载方法：直接传入FPI数据进行插值
        /// </summary>
        public InterpolationResult InterpolateFPI(List<TrajectoryPoint> trajectoryPoints, Dictionary<float, float> depthFPIMapping)
        {
            LogMessage("开始进行空间FPI插值（使用深度-FPI映射）...");

            // 1. 过滤有效的轨迹点并关联FPI值
            var validTrajectoryPoints = new List<Point3D>();
            
            foreach (var tp in trajectoryPoints)
            {
                // 根据深度匹配FPI值
                var fpiValue = FindNearestFPI(tp.RodLength, depthFPIMapping);
                
                if (!float.IsNaN(fpiValue) && fpiValue > 0)
                {
                    validTrajectoryPoints.Add(new Point3D(
                        tp.EastDisplacement,
                        tp.NorthDisplacement, 
                        tp.VerticalDepth,
                        fpiValue
                    ));
                }
            }

            if (validTrajectoryPoints.Count == 0)
            {
                throw new InvalidOperationException("没有找到有效的FPI轨迹点数据");
            }

            LogMessage($"找到 {validTrajectoryPoints.Count} 个有效FPI轨迹点");

            // 2. 定义立方体空间范围
            var cubicSpace = DefineCubicSpace(validTrajectoryPoints);
            LogMessage($"定义立方体空间: {cubicSpace.GridPoints.Count} 个网格点");

            // 3. 执行空间插值
            var interpolatedPoints = PerformSpatialInterpolation(cubicSpace, validTrajectoryPoints);
            LogMessage($"完成空间插值，生成 {interpolatedPoints.Count} 个插值点");

            // 4. 生成结果
            var result = new InterpolationResult
            {
                Space = cubicSpace,
                InterpolatedPoints = interpolatedPoints,
                ValidPointsCount = interpolatedPoints.Count(p => !float.IsNaN(p.FPI)),
                ProcessedTime = DateTime.Now
            };

            // 计算统计信息
            var validInterpolatedPoints = interpolatedPoints.Where(p => !float.IsNaN(p.FPI)).ToList();
            if (validInterpolatedPoints.Count > 0)
            {
                result.MinFPI = validInterpolatedPoints.Min(p => p.FPI);
                result.MaxFPI = validInterpolatedPoints.Max(p => p.FPI);
                result.AvgFPI = validInterpolatedPoints.Average(p => p.FPI);
            }

            LogMessage($"插值完成: 有效点 {result.ValidPointsCount}, FPI范围 [{result.MinFPI:F2}, {result.MaxFPI:F2}]");
            return result;
        }

        /// <summary>
        /// 根据深度查找最近的FPI值
        /// </summary>
        private float FindNearestFPI(float depth, Dictionary<float, float> depthFPIMapping)
        {
            if (depthFPIMapping.ContainsKey(depth))
            {
                return depthFPIMapping[depth];
            }

            // 找到最近的深度值
            var nearestDepth = depthFPIMapping.Keys
                .OrderBy(d => Math.Abs(d - depth))
                .FirstOrDefault();

            return depthFPIMapping.TryGetValue(nearestDepth, out float fpi) ? fpi : float.NaN;
        }

        /// <summary>
        /// 保存插值结果
        /// </summary>
        public void SaveInterpolationResult(InterpolationResult result, string outputPath)
        {
            try
            {
                // 确保输出目录存在
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 准备保存数据
                var saveData = new
                {
                    processedTime = result.ProcessedTime,
                    summary = new
                    {
                        totalGridPoints = result.Space.GridPoints.Count,
                        validInterpolatedPoints = result.ValidPointsCount,
                        spatialResolution = SpatialResolution,
                        influenceRadius = InfluenceRadius,
                        bounds = new
                        {
                            min = new { x = result.Space.MinBounds.X, y = result.Space.MinBounds.Y, z = result.Space.MinBounds.Z },
                            max = new { x = result.Space.MaxBounds.X, y = result.Space.MaxBounds.Y, z = result.Space.MaxBounds.Z }
                        },
                        fpiStatistics = new
                        {
                            min = result.MinFPI,
                            max = result.MaxFPI,
                            avg = result.AvgFPI
                        }
                    },
                    gridDimensions = result.Space.GetGridDimensions(),
                    interpolatedPoints = result.InterpolatedPoints.Select(p => new
                    {
                        x = p.X,
                        y = p.Y,
                        z = p.Z,
                        fpi = float.IsNaN(p.FPI) ? null : (float?)p.FPI
                    })
                };

                // 保存为JSON
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                var json = JsonSerializer.Serialize(saveData, options);
                File.WriteAllText(outputPath, json);

                LogMessage($"插值结果已保存到: {outputPath}");
            }
            catch (Exception ex)
            {
                LogMessage($"保存插值结果失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 导出为三维可视化格式
        /// </summary>
        public void ExportFor3DVisualization(InterpolationResult result, string outputPath)
        {
            try
            {
                var directory = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 只导出有效的插值点
                var validPoints = result.InterpolatedPoints.Where(p => !float.IsNaN(p.FPI)).ToList();

                var visualizationData = new
                {
                    title = "FPI空间插值可视化",
                    type = "spatial_interpolation",
                    processedTime = result.ProcessedTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    bounds = new
                    {
                        min = new { x = result.Space.MinBounds.X, y = result.Space.MinBounds.Y, z = result.Space.MinBounds.Z },
                        max = new { x = result.Space.MaxBounds.X, y = result.Space.MaxBounds.Y, z = result.Space.MaxBounds.Z }
                    },
                    resolution = SpatialResolution,
                    statistics = new
                    {
                        totalPoints = validPoints.Count,
                        minFPI = result.MinFPI,
                        maxFPI = result.MaxFPI,
                        avgFPI = result.AvgFPI
                    },
                    points = validPoints.Select(p => new
                    {
                        position = new { x = p.X, y = p.Y, z = p.Z },
                        fpi = p.FPI
                    })
                };

                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                var json = JsonSerializer.Serialize(visualizationData, options);
                File.WriteAllText(outputPath, json);

                LogMessage($"三维可视化数据已导出到: {outputPath}");
            }
            catch (Exception ex)
            {
                LogMessage($"导出三维可视化数据失败: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 记录日志消息
        /// </summary>
        private void LogMessage(string message)
        {
            OnLogMessage?.Invoke($"[SpatialFPIInterpolator] {message}");
        }
    }
} 