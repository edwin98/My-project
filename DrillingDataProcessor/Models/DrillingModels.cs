using System;
using System.Collections.Generic;

namespace DrillingDataProcessor.Models
{
    public class DrillingChartDataPoint
    {
        public string Key { get; set; } = string.Empty;
        public float Value { get; set; }
    }

    public class DrillingChartSeries
    {
        public string Name { get; set; } = string.Empty;
        public bool Display { get; set; } = true;
        public List<DrillingChartDataPoint> Data { get; set; } = new List<DrillingChartDataPoint>();
    }

    public class DrillingChartXAxis
    {
        public string Title { get; set; } = string.Empty;
        public List<string> Label { get; set; } = new List<string>();
    }

    public class DrillingChartData
    {
        public List<DrillingChartSeries> Series { get; set; } = new List<DrillingChartSeries>();
        public DrillingChartXAxis XAxis { get; set; } = new DrillingChartXAxis();
    }

    public class DrillingData
    {
        public int OriginalIndex { get; set; }
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public float Torque { get; set; }
        public float DrillPressure { get; set; }
        public float RotationSpeed { get; set; }
        public float Temperature { get; set; }
        public float Inclination { get; set; }
        public float Azimuth { get; set; }
        public float GravitySum { get; set; }
        public float MagneticStrength { get; set; }
        public float MagneticInclination { get; set; }
        public float Voltage { get; set; }
        public float Depth { get; set; }
        public float Fpi { get; set; }
        public float Ucs { get; set; }
        public bool IsMarked { get; set; } // 标记是否为黄色标记行
    }

    public class TrajectoryPoint
    {
        public DateTime MarkTimestamp { get; set; }
        public float Inclination { get; set; }
        public float Azimuth { get; set; }
        public float GravitySum { get; set; }
        public float TrueAzimuth { get; set; }
        public float RodLength { get; set; }
        public float AvgInclination { get; set; }
        public float AvgMagneticAzimuth { get; set; }
        public float EastDisplacement { get; set; }
        public float NorthDisplacement { get; set; }
        public float VerticalDepth { get; set; }
        public float XCoordinate { get; set; }
        public float LateralDeviation { get; set; }
        public float HValue { get; set; }
    }
} 