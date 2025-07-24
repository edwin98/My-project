# Unity轨迹可视化使用指南

## 快速开始 🚀

### 1. 基础设置（最简单的方式）

1. **添加可视化组件**
   ```
   在场景中创建空物体 → 添加 TrajectoryVisualizerUnified 组件
   ```

2. **设置数据源**
   ```
   将 UnityDrillingDataProcessor 拖拽到 Data Processor 字段
   ```

3. **运行测试**
   ```
   右键组件 → "创建测试数据" → 自动生成螺旋轨迹演示
   ```

### 2. 完整设置（推荐）

#### 步骤1: 场景准备
```
1. 确保场景中有主相机
2. 创建空物体命名为 "TrajectoryVisualizer"
3. 添加 TrajectoryVisualizerUnified 组件
```

#### 步骤2: 组件配置
```csharp
// 在 Inspector 中设置：
✅ Data Processor: 拖入你的 UnityDrillingDataProcessor
✅ Target Camera: 拖入主相机（或留空自动使用）
✅ Enable Mouse Interaction: ✓
✅ Color Scheme: 选择 Depth（深度着色）
```

#### 步骤3: 显示选项配置
```csharp
Display Options:
✅ Show Trajectory Line: ✓
✅ Show Trajectory Points: ✓
✅ Show Depth Labels: ✓
✅ Show Coordinate Axis: ✓
✅ Line Width: 0.1
✅ Point Size: 0.5
✅ Max Points To Display: 1000
```

## 使用方法 💡

### 方法1: 通过Inspector控制

#### 右键菜单操作
```
右键 TrajectoryVisualizerUnified 组件：
• "加载并可视化数据" - 从数据处理器加载真实数据
• "创建测试数据" - 生成螺旋测试轨迹
• "切换轨迹线显示" - 开关轨迹线
• "切换轨迹点显示" - 开关轨迹点
• "切换坐标轴显示" - 开关坐标轴
• "切换深度标签显示" - 开关深度标签
• "切换颜色方案" - 循环切换颜色方案
```

### 方法2: 通过代码控制

#### 基础代码示例
```csharp
// 获取可视化器
var visualizer = FindObjectOfType<TrajectoryVisualizerUnified>();

// 设置数据
List<TrajectoryPoint> points = dataProcessor.GetTrajectoryPoints();
visualizer.SetTrajectoryData(points);

// 切换显示选项
visualizer.ToggleTrajectoryLine();
visualizer.ToggleTrajectoryPoints();

// 改变颜色方案
visualizer.SetColorScheme(ColorSchemeType.Inclination);

// 重置视角
visualizer.ResetView();
```

#### 高级代码示例
```csharp
// 自定义显示选项
var options = new TrajectoryDisplayOptions
{
    showTrajectoryLine = true,
    showTrajectoryPoints = false,  // 大数据集时关闭点显示
    lineWidth = 0.2f,
    maxPointsToDisplay = 500,      // 限制显示点数提高性能
    labelInterval = 10
};
visualizer.SetDisplayOptions(options);

// 创建自定义轨迹数据
List<TrajectoryPoint> customPoints = new List<TrajectoryPoint>();
for (int i = 0; i < 100; i++)
{
    var point = new TrajectoryPoint
    {
        eastDisplacement = i * 0.5f,
        northDisplacement = Mathf.Sin(i * 0.1f) * 5f,
        verticalDepth = i * 0.3f,
        inclination = Random.Range(0f, 45f),
        azimuth = Random.Range(0f, 360f)
    };
    customPoints.Add(point);
}
visualizer.SetTrajectoryData(customPoints);
```

### 方法3: 使用UI控制面板

#### 添加UI控制面板
```
1. 在Canvas下创建UI面板
2. 添加 TrajectoryControlPanel 组件
3. 将可视化器拖入 Target Visualizer 字段
4. 配置按钮和滑块引用
```

#### UI控件配置
```
按钮控件:
• trajectoryLineToggle - 轨迹线开关
• trajectoryPointsToggle - 轨迹点开关
• resetViewButton - 重置视角
• createTestDataButton - 创建测试数据

滑块控件:
• lineWidthSlider - 线宽控制
• pointSizeSlider - 点大小控制
• rotationSpeedSlider - 旋转速度
• autoRotateSpeedSlider - 自动旋转速度

文本显示:
• statusText - 状态信息
• pointCountText - 点数统计
```

## 鼠标交互 🖱️

### 默认交互操作
```
🖱️ 左键拖拽: 旋转视角
🖱️ 滚轮: 缩放距离
⌨️ 自动旋转: 通过代码或UI开启
```

### 自定义交互
```csharp
// 禁用鼠标交互
visualizer.enableMouseInteraction = false;

// 调整交互速度
visualizer.rotationSpeed = 200f;  // 旋转速度
visualizer.zoomSpeed = 20f;       // 缩放速度

// 启用自动旋转
visualizer.autoRotate = true;
visualizer.autoRotateSpeed = 45f; // 度/秒
```

## 性能优化 ⚡

### 大数据集优化
```csharp
// 方法1: 限制显示点数
var options = TrajectoryDisplayOptions.CreateHighPerformance();
options.maxPointsToDisplay = 500;
options.showTrajectoryPoints = false;  // 只显示线条
visualizer.SetDisplayOptions(options);

// 方法2: 减少标签数量
options.labelInterval = 20;  // 每20个点显示一个标签
options.showDepthLabels = false;  // 完全关闭标签

// 方法3: 简化材质
visualizer.lineMaterial = simpleMaterial;  // 使用简单材质
```

### 内存优化
```csharp
// 启用对象池（默认已启用）
var renderer = visualizer.GetComponent<TrajectoryRenderer>();
renderer.useObjectPooling = true;
renderer.maxPoolSize = 1000;

// 定期清理
if (Application.isPlaying)
{
    System.GC.Collect();  // 手动垃圾回收
}
```

## 颜色方案 🎨

### 内置颜色方案
```csharp
// 深度着色（蓝色→红色）
visualizer.SetColorScheme(ColorSchemeType.Depth);

// 倾角着色（绿色→黄色）
visualizer.SetColorScheme(ColorSchemeType.Inclination);

// 方位角着色（彩虹色）
visualizer.SetColorScheme(ColorSchemeType.Azimuth);
```

### 自定义颜色
```csharp
// 使用颜色设置
var colorSettings = new TrajectoryColorScheme.ColorSettings
{
    startColor = Color.blue,
    endColor = Color.red,
    saturation = 0.8f,
    brightness = 1f
};

// 生成自定义颜色
var colors = TrajectoryColorScheme.GenerateColors(points, 
    ColorSchemeType.Custom, colorSettings);
```

## 常见问题解决 🔧

### 问题1: 轨迹不显示
```
✅ 检查数据处理器是否设置且有数据
✅ 确认显示选项已启用
✅ 检查相机位置是否合适
✅ 运行 "创建测试数据" 验证功能
```

### 问题2: 性能低下
```
✅ 减少 maxPointsToDisplay 值
✅ 关闭轨迹点显示，只显示线条
✅ 降低 labelInterval 值
✅ 使用 CreateHighPerformance() 选项
```

### 问题3: 交互不流畅
```
✅ 降低 rotationSpeed 值
✅ 检查相机是否有其他脚本冲突
✅ 确保目标帧率合适
```

### 问题4: 颜色显示异常
```
✅ 检查材质设置
✅ 确认颜色方案选择
✅ 验证数据范围是否合理
```

## 高级功能 🎯

### 1. 动态数据更新
```csharp
// 实时更新轨迹数据
void Update()
{
    if (hasNewData)
    {
        var newPoints = dataProcessor.GetTrajectoryPoints();
        visualizer.SetTrajectoryData(newPoints);
        hasNewData = false;
    }
}
```

### 2. 多轨迹显示
```csharp
// 创建多个可视化器显示不同轨迹
var visualizer1 = CreateVisualizer("Trajectory1");
var visualizer2 = CreateVisualizer("Trajectory2");

visualizer1.SetTrajectoryData(trajectory1Points);
visualizer2.SetTrajectoryData(trajectory2Points);

// 使用不同颜色方案区分
visualizer1.SetColorScheme(ColorSchemeType.Depth);
visualizer2.SetColorScheme(ColorSchemeType.Inclination);
```

### 3. 数据导出和保存
```csharp
// 获取当前状态信息
string info = visualizer.GetStatusInfo();
Debug.Log(info);

// 获取渲染统计
var renderer = visualizer.GetComponent<TrajectoryRenderer>();
string stats = renderer.GetRenderingStats();
```

## 调试技巧 🔍

### 1. Scene视图调试
```
在Scene视图中可以看到：
• 黄色线框: 轨迹边界
• 红色球体: 轨迹中心点
• Gizmos显示统计信息
```

### 2. 控制台输出
```csharp
// 启用详细日志
Debug.Log(visualizer.GetStatusInfo());

// 监控性能
Debug.Log($"当前FPS: {1f/Time.deltaTime:F1}");
```

### 3. Inspector监控
```
实时监控Inspector中的值：
• IsVisualizationActive: 可视化状态
• 当前点数统计
• 显示选项状态
```

这个指南涵盖了从基础使用到高级功能的完整流程，按照这个指南，您可以快速在Unity中实现专业的轨迹可视化效果！