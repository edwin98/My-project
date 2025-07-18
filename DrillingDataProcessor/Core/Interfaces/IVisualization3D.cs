using System.Collections.Generic;
using DrillingDataProcessor.Models;

namespace DrillingDataProcessor.Core.Interfaces
{
    /// <summary>
    /// 三维可视化模块接口
    /// </summary>
    public interface IVisualization3D
    {
        /// <summary>
        /// 生成三维轨迹可视化数据
        /// </summary>
        /// <param name="trajectoryPoints">轨迹点</param>
        /// <param name="options">可视化选项</param>
        /// <returns>可视化数据</returns>
        Visualization3DData GenerateTrajectoryVisualization(List<TrajectoryPoint> trajectoryPoints, Visualization3DOptions options);
        
        /// <summary>
        /// 导出为HTML文件（可交互）
        /// </summary>
        /// <param name="data">可视化数据</param>
        /// <param name="filePath">输出文件路径</param>
        void ExportToHTML(Visualization3DData data, string filePath);
        
        /// <summary>
        /// 导出为PLY格式（点云）
        /// </summary>
        /// <param name="data">可视化数据</param>
        /// <param name="filePath">输出文件路径</param>
        void ExportToPLY(Visualization3DData data, string filePath);
        
        /// <summary>
        /// 导出为OBJ格式（3D模型）
        /// </summary>
        /// <param name="data">可视化数据</param>
        /// <param name="filePath">输出文件路径</param>
        void ExportToOBJ(Visualization3DData data, string filePath);
    }
    
    /// <summary>
    /// 三维可视化选项
    /// </summary>
    public class Visualization3DOptions
    {
        public bool ShowTrajectoryLine { get; set; } = true;
        public bool ShowTrajectoryPoints { get; set; } = true;
        public bool ShowCoordinateAxis { get; set; } = true;
        public bool ShowDepthLabels { get; set; } = true;
        public bool EnableInteractiveControls { get; set; } = true;
        public string ColorScheme { get; set; } = "depth"; // depth, inclination, azimuth
        public float LineWidth { get; set; } = 2.0f;
        public float PointSize { get; set; } = 5.0f;
        public string Title { get; set; } = "钻孔轨迹三维可视化";
    }
    
    /// <summary>
    /// 三维可视化数据
    /// </summary>
    public class Visualization3DData
    {
        public List<Point3D> TrajectoryLine { get; set; } = new List<Point3D>();
        public List<Point3D> TrajectoryPoints { get; set; } = new List<Point3D>();
        public List<string> Labels { get; set; } = new List<string>();
        public List<float> Colors { get; set; } = new List<float>();
        public BoundingBox3D Bounds { get; set; } = new BoundingBox3D();
        public string HtmlTemplate { get; set; } = "";
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// 三维点
    /// </summary>
    public class Point3D
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
        public float Depth { get; set; }
        public float Inclination { get; set; }
        public float Azimuth { get; set; }
        
        public Point3D(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
    
    /// <summary>
    /// 三维边界框
    /// </summary>
    public class BoundingBox3D
    {
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }
        public float MinZ { get; set; }
        public float MaxZ { get; set; }
        
        public float Width => MaxX - MinX;
        public float Height => MaxY - MinY;
        public float Depth => MaxZ - MinZ;
        public Point3D Center => new Point3D((MinX + MaxX) / 2, (MinY + MaxY) / 2, (MinZ + MaxZ) / 2);
    }
} 