# Unity钻井三维可视化系统使用指南

## 系统概述

Unity钻井三维可视化系统是一个完整的钻井数据处理和可视化解决方案，集成了以下核心功能：

- **钻井轨迹三维可视化** - 使用LineRenderer绘制轨迹线和轨迹点
- **FPI值空间可视化** - 多种模式显示FPI空间插值数据
- **坐标轴显示** - 清晰的三维坐标参考系
- **交互式相机控制** - 鼠标拖拽旋转和滚轮缩放
- **数据处理器集成** - 连接外部DrillingDataProcessor处理Excel数据
- **UI控制面板** - 实时调整可视化参数

## 快速开始

### 1. 自动场景设置

最简单的方式是使用自动场景设置：

```csharp
// 在空场景中创建一个GameObject，添加DrillingVisualizationSceneSetup组件
// 在Inspector中勾选 "Auto Setup On Start"
// 运行场景，系统会自动创建所有必要的组件
```

或者手动执行：
```csharp
// 在DrillingVisualizationSceneSetup组件上右键选择"设置钻井可视化场景"
```

### 2. 手动组件添加

如果需要手动设置，按以下顺序添加组件：

1. **创建主可视化对象**
   ```
   创建空GameObject "DrillingVisualization"
   添加 IntegratedDrillingVisualizationController 组件
   ```

2. **创建数据处理器桥接器**
   ```
   创建空GameObject "DataProcessorBridge"
   添加 DrillingDataProcessorBridge 组件
   ```

3. **设置UI控制面板**
   ```
   创建Canvas (如果场景中没有)
   创建控制面板GameObject
   添加 DrillingVisualizationControlPanel 组件
   ```

## 核心组件说明

### IntegratedDrillingVisualizationController

主要的可视化控制器，负责：
- 轨迹线和轨迹点的渲染
- FPI值的空间可视化
- 坐标轴的显示
- 相机控制
- 颜色方案管理

**主要方法：**
```csharp
LoadAndProcessData()              // 加载和处理数据
ToggleTrajectoryLine()           // 切换轨迹线显示
ToggleTrajectoryPoints()         // 切换轨迹点显示
ToggleFPIVisualization()         // 切换FPI可视化
ToggleCoordinateAxes()           // 切换坐标轴显示
ResetCameraView()                // 重置相机视角
SetTrajectoryColorScheme(int)    // 设置轨迹颜色方案
SetFPIVisualizationMode(int)     // 设置FPI可视化模式
```

### DrillingDataProcessorBridge

数据处理器桥接器，负责：
- 调用外部DrillingDataProcessor.exe
- 数据文件的管理和传输
- 处理状态的监控

**主要方法：**
```csharp
StartProcessing()                // 开始数据处理
LoadExistingData()              // 加载现有数据
GetAvailableOutputFiles()       // 获取可用输出文件
```

### DrillingVisualizationControlPanel

UI控制面板，提供：
- 可视化开关控制
- 参数调节滑块
- 颜色方案选择
- 处理状态显示

## 数据格式和文件结构

### 输入数据

1. **Excel文件格式** (J16原始.xlsx)
   - A列：原序号
   - B列：日期
   - C列：时间
   - D列：扭矩
   - E列：钻压
   - F列：转速
   - G列：温度
   - H列：倾角
   - I列：方位角
   - J列：重力和
   - K列：磁强
   - L列：磁倾
   - M列：电压

2. **黄色标记行**
   - 在Excel中用黄色背景标记的行，用于深度分段

### 输出数据

1. **轨迹数据** (trajectory_data.json)
   ```json
   {
     "points": [
       {
         "x": 0.0, "y": 0.0, "z": 0.0,
         "depth": 0.0,
         "inclination": 0.0,
         "azimuth": 0.0,
         "fpi": 1000000.0
       }
     ]
   }
   ```

2. **空间FPI数据** (spatial_fpi_visualization.json)
   ```json
   {
     "points": [
       {
         "position": {"x": 0.0, "y": 0.0, "z": 0.0},
         "fpi": 1000000.0
       }
     ]
   }
   ```

## 可视化模式说明

### 轨迹颜色方案

1. **深度着色** (Depth) - 按深度从蓝色(浅)到红色(深)渐变
2. **倾角着色** (Inclination) - 按倾角从绿色到黄色渐变
3. **方位角着色** (Azimuth) - 按方位角使用HSV色环
4. **FPI着色** (FPI) - 按FPI值使用热度图着色

### FPI可视化模式

1. **点云模式** (Points) - 简单的球体点云，性能最佳
2. **球体模式** (Spheres) - 大小可变的球体，根据FPI值调整大小
3. **立方体模式** (Cubes) - 立方体表示，适合网格化数据
4. **合并网格模式** (Merged) - 优化性能的合并网格（当前为简化实现）

### FPI颜色方案

1. **热度图** (Heat) - 蓝色(低) → 青色 → 绿色 → 黄色 → 橙色 → 红色(高)
2. **彩虹色** (Rainbow) - 标准HSV彩虹渐变
3. **灰度** (Grayscale) - 黑白渐变
4. **蓝红渐变** (BlueRed) - 简单的蓝到红渐变

## 相机控制

### 鼠标操作
- **左键拖拽** - 旋转视角
- **滚轮** - 缩放距离
- **右键** - （预留，可扩展为平移）

### 程序控制
```csharp
// 重置视角
visualizationController.ResetCameraView();

// 启用/禁用自动旋转
visualizationController.ToggleAutoRotate();
```

## 数据处理流程

### 1. 准备数据
```
1. 确保Excel文件格式正确
2. 用黄色背景标记深度分段行
3. 将文件放在 DrillingDataProcessor/TestData/ 目录下
```

### 2. 配置处理器
```csharp
// 在DrillingDataProcessorBridge中配置路径
dataProcessorExecutablePath = "DrillingDataProcessor/bin/Release/net6.0/DrillingDataProcessor.exe";
inputExcelPath = "DrillingDataProcessor/TestData/J16原始.xlsx";
outputDirectory = "DrillingDataProcessor/Output";
```

### 3. 运行处理
```csharp
// 程序控制
dataProcessorBridge.StartProcessing();

// 或使用UI控制面板的"处理数据"按钮
```

### 4. 加载可视化
```csharp
// 处理完成后自动加载，或手动加载
visualizationController.LoadAndProcessData();
```

## 性能优化

### LOD (Level of Detail) 控制

1. **FPI点LOD**
   ```csharp
   // 在IntegratedDrillingVisualizationController中设置
   maxFPIPoints = 3000; // 最大显示点数
   ```

2. **相机距离LOD**
   ```csharp
   // 在SpatialFPIVisualizer中启用
   enableLOD = true;
   lodDistance1 = 50f;
   lodDistance2 = 100f;
   ```

### 渲染优化

1. **批处理**
   ```csharp
   // 启用GPU实例化（规划中）
   useGPUInstancing = true;
   ```

2. **视锥剔除**
   ```csharp
   // 启用视锥剔除
   enableFrustumCulling = true;
   ```

## 常见问题解决

### 1. 轨迹不显示
**可能原因：**
- 数据文件不存在或格式错误
- 相机位置不合适
- 可视化组件未正确配置

**解决方法：**
```csharp
// 检查数据加载
visualizationController.LoadAndProcessData();

// 重置相机视角
visualizationController.ResetCameraView();

// 检查显示开关
visualizationController.ToggleTrajectoryLine();
visualizationController.ToggleTrajectoryPoints();
```

### 2. 数据处理失败
**可能原因：**
- DrillingDataProcessor.exe路径错误
- Excel文件格式不正确
- 权限问题

**解决方法：**
```csharp
// 检查可执行文件路径
string fullPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", dataProcessorExecutablePath));
Debug.Log($"处理器路径: {fullPath}");
Debug.Log($"文件存在: {File.Exists(fullPath)}");

// 检查输入文件
string inputPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", inputExcelPath));
Debug.Log($"输入文件: {inputPath}");
Debug.Log($"文件存在: {File.Exists(inputPath)}");
```

### 3. 性能问题
**解决方法：**
```csharp
// 减少显示点数
maxFPIPoints = 1000;

// 启用LOD
enableLOD = true;

// 关闭不必要的可视化
visualizationController.ToggleFPIVisualization(); // 关闭FPI可视化
visualizationController.ToggleTrajectoryPoints(); // 只显示轨迹线
```

### 4. UI控制面板不响应
**解决方法：**
```csharp
// 检查组件连接
controlPanel.SaveSettings();
controlPanel.LoadSettings();

// 重新初始化
Destroy(controlPanel);
// 重新添加组件
```

## 扩展开发

### 添加新的颜色方案
```csharp
// 在IntegratedDrillingVisualizationController中扩展GetTrajectoryColor方法
case TrajectoryColorScheme.Custom:
    // 添加自定义颜色逻辑
    return CalculateCustomColor(point, index);
```

### 添加新的可视化模式
```csharp
// 在FPIVisualizationMode枚举中添加新模式
public enum FPIVisualizationMode
{
    Points, Spheres, Cubes, Merged,
    NewCustomMode // 新模式
}

// 在CreateFPIVisualization方法中添加处理逻辑
case FPIVisualizationMode.NewCustomMode:
    CreateCustomVisualization(fpiObj, filteredFPIData);
    break;
```

### 添加新的交互功能
```csharp
// 继承现有交互组件
public class CustomPointInteraction : MonoBehaviour
{
    void OnMouseDown()
    {
        // 自定义交互逻辑
    }
}
```

## 系统测试

### 运行自动测试
```csharp
// 在场景中添加DrillingVisualizationTest组件
// 在Inspector中勾选"Run Tests On Start"
// 或手动执行：
testComponent.RunAllTestsNow();
```

### 快速验证
```csharp
// 右键菜单选择"快速验证系统"
testComponent.QuickValidateSystem();
```

### 生成测试报告
```csharp
// 右键菜单选择"创建测试报告"
testComponent.CreateTestReport();
```

## 版本更新和维护

### 备份设置
```csharp
// UI设置会自动保存到PlayerPrefs
// 手动保存：
controlPanel.SaveSettings();
```

### 清理和重置
```csharp
// 清理场景
sceneSetup.CleanupScene();

// 重新设置
sceneSetup.SetupScene();
```

## 技术支持

如遇到问题，请按以下步骤排查：

1. **运行系统测试** - 确定问题范围
2. **检查日志** - 查看Unity Console的错误信息
3. **验证文件路径** - 确保所有文件路径正确
4. **重新初始化** - 尝试重新设置场景

更多技术细节请参考源代码注释和算法描述文档。