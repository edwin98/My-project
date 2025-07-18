using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using DrillingDataProcessor.Core.Interfaces;
using DrillingDataProcessor.Models;

namespace DrillingDataProcessor.Core.Modules
{
    /// <summary>
    /// 三维轨迹可视化器实现
    /// </summary>
    public class Trajectory3DVisualizer : IVisualization3D
    {
        public event Action<string>? OnLogMessage;
        
        /// <summary>
        /// 生成三维轨迹可视化数据
        /// </summary>
        public Visualization3DData GenerateTrajectoryVisualization(List<TrajectoryPoint> trajectoryPoints, Visualization3DOptions options)
        {
            LogMessage("开始生成三维轨迹可视化数据...");
            
            var data = new Visualization3DData();
            
            if (trajectoryPoints == null || !trajectoryPoints.Any())
            {
                LogMessage("警告：没有轨迹点数据");
                return data;
            }
            
            // 生成轨迹线和轨迹点
            GenerateTrajectoryGeometry(trajectoryPoints, data, options);
            
            // 计算边界框
            CalculateBoundingBox(data);
            
            // 生成颜色数据
            GenerateColorData(trajectoryPoints, data, options);
            
            // 生成标签
            GenerateLabels(trajectoryPoints, data, options);
            
            // 生成HTML模板
            GenerateHtmlTemplate(data, options);
            
            // 添加元数据
            AddMetadata(trajectoryPoints, data, options);
            
            LogMessage($"三维可视化数据生成完成：{data.TrajectoryPoints.Count} 个轨迹点");
            
            return data;
        }
        
        /// <summary>
        /// 导出为HTML文件（可交互）
        /// </summary>
        public void ExportToHTML(Visualization3DData data, string filePath)
        {
            LogMessage($"正在导出HTML文件: {filePath}");
            
            try
            {
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                var htmlContent = GenerateCompleteHtmlContent(data);
                File.WriteAllText(filePath, htmlContent, Encoding.UTF8);
                
                LogMessage($"HTML文件导出成功: {filePath}");
            }
            catch (Exception ex)
            {
                LogMessage($"HTML文件导出失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 导出为PLY格式（点云）
        /// </summary>
        public void ExportToPLY(Visualization3DData data, string filePath)
        {
            LogMessage($"正在导出PLY文件: {filePath}");
            
            try
            {
                var plyContent = GeneratePlyContent(data);
                File.WriteAllText(filePath, plyContent, Encoding.ASCII);
                
                LogMessage($"PLY文件导出成功: {filePath}");
            }
            catch (Exception ex)
            {
                LogMessage($"PLY文件导出失败: {ex.Message}");
                throw;
            }
        }
        
        /// <summary>
        /// 导出为OBJ格式（3D模型）
        /// </summary>
        public void ExportToOBJ(Visualization3DData data, string filePath)
        {
            LogMessage($"正在导出OBJ文件: {filePath}");
            
            try
            {
                var objContent = GenerateObjContent(data);
                File.WriteAllText(filePath, objContent, Encoding.ASCII);
                
                LogMessage($"OBJ文件导出成功: {filePath}");
            }
            catch (Exception ex)
            {
                LogMessage($"OBJ文件导出失败: {ex.Message}");
                throw;
            }
        }
        
        private void GenerateTrajectoryGeometry(List<TrajectoryPoint> trajectoryPoints, Visualization3DData data, Visualization3DOptions options)
        {
            foreach (var point in trajectoryPoints)
            {
                var point3D = new Point3D(point.EastDisplacement, point.NorthDisplacement, -point.VerticalDepth)
                {
                    Depth = point.RodLength,
                    Inclination = point.Inclination,
                    Azimuth = point.Azimuth
                };
                
                if (options.ShowTrajectoryLine)
                {
                    data.TrajectoryLine.Add(point3D);
                }
                
                if (options.ShowTrajectoryPoints)
                {
                    data.TrajectoryPoints.Add(point3D);
                }
            }
        }
        
        private void CalculateBoundingBox(Visualization3DData data)
        {
            if (!data.TrajectoryPoints.Any()) return;
            
            data.Bounds.MinX = data.TrajectoryPoints.Min(p => p.X);
            data.Bounds.MaxX = data.TrajectoryPoints.Max(p => p.X);
            data.Bounds.MinY = data.TrajectoryPoints.Min(p => p.Y);
            data.Bounds.MaxY = data.TrajectoryPoints.Max(p => p.Y);
            data.Bounds.MinZ = data.TrajectoryPoints.Min(p => p.Z);
            data.Bounds.MaxZ = data.TrajectoryPoints.Max(p => p.Z);
        }
        
        private void GenerateColorData(List<TrajectoryPoint> trajectoryPoints, Visualization3DData data, Visualization3DOptions options)
        {
            data.Colors.Clear();
            
            switch (options.ColorScheme.ToLower())
            {
                case "depth":
                    var maxDepth = trajectoryPoints.Max(p => p.RodLength);
                    var minDepth = trajectoryPoints.Min(p => p.RodLength);
                    foreach (var point in trajectoryPoints)
                    {
                        float normalized = maxDepth > minDepth ? (point.RodLength - minDepth) / (maxDepth - minDepth) : 0f;
                        data.Colors.Add(normalized);
                    }
                    break;
                    
                case "inclination":
                    var maxInc = trajectoryPoints.Max(p => p.Inclination);
                    var minInc = trajectoryPoints.Min(p => p.Inclination);
                    foreach (var point in trajectoryPoints)
                    {
                        float normalized = maxInc > minInc ? (point.Inclination - minInc) / (maxInc - minInc) : 0f;
                        data.Colors.Add(normalized);
                    }
                    break;
                    
                case "azimuth":
                    foreach (var point in trajectoryPoints)
                    {
                        float normalized = point.Azimuth / 360f;
                        data.Colors.Add(normalized);
                    }
                    break;
                    
                default:
                    // 默认深度着色
                    for (int i = 0; i < trajectoryPoints.Count; i++)
                    {
                        data.Colors.Add((float)i / trajectoryPoints.Count);
                    }
                    break;
            }
        }
        
        private void GenerateLabels(List<TrajectoryPoint> trajectoryPoints, Visualization3DData data, Visualization3DOptions options)
        {
            if (!options.ShowDepthLabels) return;
            
            data.Labels.Clear();
            for (int i = 0; i < trajectoryPoints.Count; i++)
            {
                var point = trajectoryPoints[i];
                data.Labels.Add($"点{i + 1}: 深度{point.RodLength:F1}m, 倾角{point.Inclination:F1}°, 方位{point.Azimuth:F1}°");
            }
        }
        
        private void GenerateHtmlTemplate(Visualization3DData data, Visualization3DOptions options)
        {
            data.HtmlTemplate = GetHtmlTemplate(options);
        }
        
        private void AddMetadata(List<TrajectoryPoint> trajectoryPoints, Visualization3DData data, Visualization3DOptions options)
        {
            data.Metadata["title"] = options.Title;
            data.Metadata["pointCount"] = trajectoryPoints.Count;
            data.Metadata["maxDepth"] = trajectoryPoints.Max(p => p.RodLength);
            data.Metadata["maxInclination"] = trajectoryPoints.Max(p => p.Inclination);
            data.Metadata["maxAzimuth"] = trajectoryPoints.Max(p => p.Azimuth);
            data.Metadata["generatedTime"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.Metadata["colorScheme"] = options.ColorScheme;
        }
        
        private string GenerateCompleteHtmlContent(Visualization3DData data)
        {
            var pointsJson = JsonSerializer.Serialize(data.TrajectoryPoints.Select(p => new { x = p.X, y = p.Y, z = p.Z }).ToArray());
            var colorsJson = JsonSerializer.Serialize(data.Colors);
            var labelsJson = JsonSerializer.Serialize(data.Labels);
            var boundsJson = JsonSerializer.Serialize(new 
            { 
                min = new { x = data.Bounds.MinX, y = data.Bounds.MinY, z = data.Bounds.MinZ },
                max = new { x = data.Bounds.MaxX, y = data.Bounds.MaxY, z = data.Bounds.MaxZ }
            });
            
            return data.HtmlTemplate
                .Replace("{{TRAJECTORY_POINTS}}", pointsJson)
                .Replace("{{COLORS}}", colorsJson)
                .Replace("{{LABELS}}", labelsJson)
                .Replace("{{BOUNDS}}", boundsJson)
                .Replace("{{TITLE}}", data.Metadata.GetValueOrDefault("title", "钻孔轨迹可视化").ToString());
        }
        
        private string GeneratePlyContent(Visualization3DData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ply");
            sb.AppendLine("format ascii 1.0");
            sb.AppendLine($"element vertex {data.TrajectoryPoints.Count}");
            sb.AppendLine("property float x");
            sb.AppendLine("property float y");
            sb.AppendLine("property float z");
            sb.AppendLine("property uchar red");
            sb.AppendLine("property uchar green");
            sb.AppendLine("property uchar blue");
            sb.AppendLine("end_header");
            
            for (int i = 0; i < data.TrajectoryPoints.Count; i++)
            {
                var point = data.TrajectoryPoints[i];
                var colorValue = i < data.Colors.Count ? data.Colors[i] : 0f;
                
                // 将颜色值转换为RGB
                var (r, g, b) = ColorValueToRGB(colorValue);
                
                sb.AppendLine($"{point.X:F6} {point.Y:F6} {point.Z:F6} {r} {g} {b}");
            }
            
            return sb.ToString();
        }
        
        private string GenerateObjContent(Visualization3DData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("# 钻孔轨迹 3D 模型");
            sb.AppendLine($"# 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine();
            
            // 写入顶点
            foreach (var point in data.TrajectoryPoints)
            {
                sb.AppendLine($"v {point.X:F6} {point.Y:F6} {point.Z:F6}");
            }
            
            // 写入线段（连接相邻点）
            sb.AppendLine();
            sb.AppendLine("# 轨迹线");
            for (int i = 1; i < data.TrajectoryPoints.Count; i++)
            {
                sb.AppendLine($"l {i} {i + 1}");
            }
            
            return sb.ToString();
        }
        
        private (byte r, byte g, byte b) ColorValueToRGB(float value)
        {
            // 将0-1的值映射到彩虹色谱
            value = Math.Max(0, Math.Min(1, value));
            
            byte r, g, b;
            if (value < 0.25f)
            {
                r = 0;
                g = (byte)(255 * value * 4);
                b = 255;
            }
            else if (value < 0.5f)
            {
                r = 0;
                g = 255;
                b = (byte)(255 * (0.5f - value) * 4);
            }
            else if (value < 0.75f)
            {
                r = (byte)(255 * (value - 0.5f) * 4);
                g = 255;
                b = 0;
            }
            else
            {
                r = 255;
                g = (byte)(255 * (1 - value) * 4);
                b = 0;
            }
            
            return (r, g, b);
        }
        
        private string GetHtmlTemplate(Visualization3DOptions options)
        {
            return @"
<!DOCTYPE html>
<html lang='zh-CN'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{{TITLE}}</title>
    <script src='https://cdn.jsdelivr.net/npm/three@0.157.0/build/three.min.js'></script>
    <script src='https://cdn.jsdelivr.net/npm/three@0.157.0/examples/js/controls/OrbitControls.js'></script>
    <style>
        body { margin: 0; padding: 0; overflow: hidden; font-family: Arial, sans-serif; background: #000; }
        #container { width: 100vw; height: 100vh; position: relative; }
        #info { position: absolute; top: 10px; left: 10px; color: white; background: rgba(0,0,0,0.5); padding: 10px; border-radius: 5px; }
        #controls { position: absolute; top: 10px; right: 10px; color: white; background: rgba(0,0,0,0.5); padding: 10px; border-radius: 5px; }
        button { margin: 2px; padding: 5px 10px; background: #444; color: white; border: none; border-radius: 3px; cursor: pointer; }
        button:hover { background: #666; }
        button.active { background: #0a84ff; }
    </style>
</head>
<body>
    <div id='container'>
        <div id='info'>
            <h3>{{TITLE}}</h3>
            <p>轨迹点数: <span id='pointCount'>0</span></p>
            <p>深度范围: <span id='depthRange'>0</span></p>
            <p>当前视角: <span id='viewInfo'>默认</span></p>
        </div>
        <div id='controls'>
            <button onclick='resetView()'>重置视角</button><br>
            <button onclick='togglePoints()' id='pointsBtn' class='active'>显示点</button><br>
            <button onclick='toggleLines()' id='linesBtn' class='active'>显示线</button><br>
            <button onclick='toggleAxis()' id='axisBtn' class='active'>显示坐标轴</button><br>
            <button onclick='changeColorScheme()' id='colorBtn'>深度着色</button><br>
            <button onclick='exportView()'>导出视图</button>
        </div>
    </div>

    <script>
        // 数据注入点
        const trajectoryPoints = {{TRAJECTORY_POINTS}};
        const colors = {{COLORS}};
        const labels = {{LABELS}};
        const bounds = {{BOUNDS}};
        
        // Three.js 场景设置
        let scene, camera, renderer, controls;
        let trajectoryLine, trajectoryPointsMesh, coordinateAxes;
        let currentColorScheme = 'depth';
        
        function init() {
            // 创建场景
            scene = new THREE.Scene();
            scene.background = new THREE.Color(0x222222);
            
            // 创建相机
            const aspect = window.innerWidth / window.innerHeight;
            camera = new THREE.PerspectiveCamera(75, aspect, 0.1, 10000);
            
            // 创建渲染器
            renderer = new THREE.WebGLRenderer({ antialias: true });
            renderer.setSize(window.innerWidth, window.innerHeight);
            renderer.shadowMap.enabled = true;
            renderer.shadowMap.type = THREE.PCFSoftShadowMap;
            document.getElementById('container').appendChild(renderer.domElement);
            
            // 创建控制器
            controls = new THREE.OrbitControls(camera, renderer.domElement);
            controls.enableDamping = true;
            controls.dampingFactor = 0.05;
            
            // 添加光照
            addLighting();
            
            // 创建轨迹
            createTrajectory();
            
            // 创建坐标轴
            createCoordinateAxes();
            
            // 设置相机位置
            resetView();
            
            // 更新界面信息
            updateInfo();
            
            // 开始渲染循环
            animate();
        }
        
        function addLighting() {
            const ambientLight = new THREE.AmbientLight(0x404040, 0.4);
            scene.add(ambientLight);
            
            const directionalLight = new THREE.DirectionalLight(0xffffff, 0.8);
            directionalLight.position.set(1, 1, 1);
            directionalLight.castShadow = true;
            scene.add(directionalLight);
        }
        
        function createTrajectory() {
            // 创建轨迹线
            const lineGeometry = new THREE.BufferGeometry();
            const linePositions = new Float32Array(trajectoryPoints.length * 3);
            const lineColors = new Float32Array(trajectoryPoints.length * 3);
            
            for (let i = 0; i < trajectoryPoints.length; i++) {
                const point = trajectoryPoints[i];
                linePositions[i * 3] = point.x;
                linePositions[i * 3 + 1] = point.y;
                linePositions[i * 3 + 2] = point.z;
                
                const color = getColorFromValue(colors[i] || 0);
                lineColors[i * 3] = color.r;
                lineColors[i * 3 + 1] = color.g;
                lineColors[i * 3 + 2] = color.b;
            }
            
            lineGeometry.setAttribute('position', new THREE.BufferAttribute(linePositions, 3));
            lineGeometry.setAttribute('color', new THREE.BufferAttribute(lineColors, 3));
            
            const lineMaterial = new THREE.LineBasicMaterial({ 
                vertexColors: true, 
                linewidth: " + options.LineWidth + @" 
            });
            
            trajectoryLine = new THREE.Line(lineGeometry, lineMaterial);
            scene.add(trajectoryLine);
            
            // 创建轨迹点
            const pointGeometry = new THREE.BufferGeometry();
            pointGeometry.setAttribute('position', new THREE.BufferAttribute(linePositions, 3));
            pointGeometry.setAttribute('color', new THREE.BufferAttribute(lineColors, 3));
            
            const pointMaterial = new THREE.PointsMaterial({ 
                vertexColors: true, 
                size: " + options.PointSize + @" 
            });
            
            trajectoryPointsMesh = new THREE.Points(pointGeometry, pointMaterial);
            scene.add(trajectoryPointsMesh);
        }
        
        function createCoordinateAxes() {
            coordinateAxes = new THREE.Group();
            
            // X轴 - 红色
            const xGeometry = new THREE.BufferGeometry().setFromPoints([
                new THREE.Vector3(0, 0, 0),
                new THREE.Vector3(100, 0, 0)
            ]);
            const xMaterial = new THREE.LineBasicMaterial({ color: 0xff0000 });
            const xAxis = new THREE.Line(xGeometry, xMaterial);
            coordinateAxes.add(xAxis);
            
            // Y轴 - 绿色
            const yGeometry = new THREE.BufferGeometry().setFromPoints([
                new THREE.Vector3(0, 0, 0),
                new THREE.Vector3(0, 100, 0)
            ]);
            const yMaterial = new THREE.LineBasicMaterial({ color: 0x00ff00 });
            const yAxis = new THREE.Line(yGeometry, yMaterial);
            coordinateAxes.add(yAxis);
            
            // Z轴 - 蓝色
            const zGeometry = new THREE.BufferGeometry().setFromPoints([
                new THREE.Vector3(0, 0, 0),
                new THREE.Vector3(0, 0, 100)
            ]);
            const zMaterial = new THREE.LineBasicMaterial({ color: 0x0000ff });
            const zAxis = new THREE.Line(zGeometry, zMaterial);
            coordinateAxes.add(zAxis);
            
            scene.add(coordinateAxes);
        }
        
        function getColorFromValue(value) {
            // 彩虹色谱映射
            const hue = (1 - value) * 240 / 360; // 从红到蓝
            const color = new THREE.Color().setHSL(hue, 1, 0.5);
            return color;
        }
        
        function resetView() {
            const center = bounds ? {
                x: (bounds.min.x + bounds.max.x) / 2,
                y: (bounds.min.y + bounds.max.y) / 2,
                z: (bounds.min.z + bounds.max.z) / 2
            } : { x: 0, y: 0, z: 0 };
            
            const size = bounds ? Math.max(
                bounds.max.x - bounds.min.x,
                bounds.max.y - bounds.min.y,
                bounds.max.z - bounds.min.z
            ) : 100;
            
            camera.position.set(center.x + size, center.y + size, center.z + size);
            controls.target.set(center.x, center.y, center.z);
            controls.update();
            
            document.getElementById('viewInfo').textContent = '俯视视角';
        }
        
        function togglePoints() {
            trajectoryPointsMesh.visible = !trajectoryPointsMesh.visible;
            const btn = document.getElementById('pointsBtn');
            btn.classList.toggle('active');
            btn.textContent = trajectoryPointsMesh.visible ? '隐藏点' : '显示点';
        }
        
        function toggleLines() {
            trajectoryLine.visible = !trajectoryLine.visible;
            const btn = document.getElementById('linesBtn');
            btn.classList.toggle('active');
            btn.textContent = trajectoryLine.visible ? '隐藏线' : '显示线';
        }
        
        function toggleAxis() {
            coordinateAxes.visible = !coordinateAxes.visible;
            const btn = document.getElementById('axisBtn');
            btn.classList.toggle('active');
            btn.textContent = coordinateAxes.visible ? '隐藏坐标轴' : '显示坐标轴';
        }
        
        function changeColorScheme() {
            const schemes = ['depth', 'inclination', 'azimuth'];
            const currentIndex = schemes.indexOf(currentColorScheme);
            currentColorScheme = schemes[(currentIndex + 1) % schemes.length];
            
            const schemeNames = { depth: '深度着色', inclination: '倾角着色', azimuth: '方位着色' };
            document.getElementById('colorBtn').textContent = schemeNames[currentColorScheme];
            
            // 重新创建轨迹以应用新的颜色方案
            scene.remove(trajectoryLine);
            scene.remove(trajectoryPointsMesh);
            createTrajectory();
        }
        
        function exportView() {
            const link = document.createElement('a');
            link.download = 'trajectory_view.png';
            link.href = renderer.domElement.toDataURL();
            link.click();
        }
        
        function updateInfo() {
            document.getElementById('pointCount').textContent = trajectoryPoints.length;
            if (bounds) {
                const depthRange = `${bounds.min.z.toFixed(1)}m 到 ${bounds.max.z.toFixed(1)}m`;
                document.getElementById('depthRange').textContent = depthRange;
            }
        }
        
        function animate() {
            requestAnimationFrame(animate);
            controls.update();
            renderer.render(scene, camera);
        }
        
        function onWindowResize() {
            camera.aspect = window.innerWidth / window.innerHeight;
            camera.updateProjectionMatrix();
            renderer.setSize(window.innerWidth, window.innerHeight);
        }
        
        window.addEventListener('resize', onWindowResize);
        
        // 初始化
        init();
    </script>
</body>
</html>";
        }
        
        private void LogMessage(string message)
        {
            OnLogMessage?.Invoke(message);
        }
    }
} 