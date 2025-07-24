# CLAUDE.md

这个文件为 Claude Code (claude.ai/code) 在此代码库中工作提供指导。

## 项目概述

这是一个Unity钻井数据处理与三维轨迹可视化系统，结合了独立的C#数据处理器和Unity三维可视化组件。系统能够读取Excel格式的钻井数据，进行智能分析处理，并在Unity中提供交互式三维轨迹可视化。

## 项目架构

### 核心组件
1. **独立数据处理器** (`DrillingDataProcessor/`): 纯C#控制台应用，负责Excel数据读取和处理
2. **Unity可视化系统** (`Assets/Scripts/`): Unity组件，负责三维轨迹可视化
3. **Unity集成脚本** (`Assets/Script/`): Unity脚本，连接数据处理器和可视化系统

### 主要脚本文件

#### 推荐使用（统一入口点）
- `Assets/Scripts/UnifiedDrillingVisualizationEntry.cs`: **统一的钻井数据可视化入口点**（推荐使用）
- `Assets/Scripts/VisualizationMigrationHelper.cs`: 可视化组件迁移助手

#### 核心可视化组件
- `Assets/Scripts/TrajectoryVisualizer.cs`: 轨迹可视化器
- `Assets/Scripts/SpatialFPIVisualizer.cs`: 空间FPI可视化器
- `Assets/Scripts/IntegratedDrillingVisualizer.cs`: 集成的钻井可视化器

#### 数据处理组件
- `Assets/Script/UnityDrillingDataProcessor.cs`: Unity数据处理器集成

#### 遗留组件（不推荐使用）
- `Assets/Script/Trajectory3DVisualizer.cs`: 旧版三维轨迹可视化器
- `Assets/Script/Trajectory3DController.cs`: 旧版三维轨迹控制器
- `Assets/Script/Trajectory3DSceneSetup.cs`: 旧版自动场景设置

## 常用命令

### Unity项目操作
- 打开Unity项目: 在Unity Hub中打开项目根目录
- 构建项目: 使用Unity编辑器的Build Settings

### 数据处理器操作
```bash
# 进入数据处理器目录
cd DrillingDataProcessor

# 恢复依赖包
dotnet restore

# 构建项目
dotnet build

# 运行数据处理
dotnet run
```

### Burst编译问题修复
```bash
# 运行Burst编译修复脚本
fix_burst_compilation.bat
```

## 数据流程

### 推荐流程（使用统一入口点）
1. **Excel数据输入**: 钻井数据以Excel格式存储在`Assets/Data/`或`DrillingDataProcessor/TestData/`
2. **数据处理**: 使用`UnityDrillingDataProcessor`或独立处理器处理数据
3. **轨迹计算**: 计算三维轨迹坐标和相关参数
4. **统一可视化**: 使用`UnifiedDrillingVisualizationEntry`进行一站式可视化
5. **交互控制**: 内置鼠标交互和相机控制

### 传统流程（遗留方式）
1. **Excel数据输入**: 钻井数据以Excel格式存储在`Assets/Data/`或`DrillingDataProcessor/TestData/`
2. **数据处理**: 使用`UnityDrillingDataProcessor`或独立处理器处理数据
3. **轨迹计算**: 计算三维轨迹坐标和相关参数
4. **三维可视化**: 使用`Trajectory3DVisualizer`在Unity中显示三维轨迹
5. **交互控制**: 通过`Trajectory3DController`提供UI控制和鼠标交互

## 核心功能

### 数据处理功能
- Excel数据读取和解析
- 智能数据过滤和验证
- 标记行检测和深度插值
- FPI/UCS计算
- 轨迹计算和位移分析
- 多格式数据导出

### 可视化功能
- 三维轨迹线和轨迹点显示
- 多种颜色方案（深度、倾角、方位角）
- 坐标轴和深度标签
- 交互式相机控制
- 实时数据更新

## 开发指南

### 多入口点问题解决方案

#### 问题描述
项目中原本存在多个重复的可视化入口点，导致代码维护困难和功能冲突：
- `Assets/Script/Trajectory3DVisualizer.cs` - 旧版轨迹可视化器
- `Assets/Scripts/TrajectoryVisualizer.cs` - 遗留版本可视化器  
- `Assets/Scripts/Trajectory/TrajectoryVisualizerUnified.cs` - 统一版本可视化器
- `Assets/Scripts/IntegratedDrillingVisualizationController.cs` - 集成控制器

#### 解决方案
使用 `UnifiedDrillingVisualizationEntry.cs` 作为**唯一推荐的入口点**：
- 整合所有可视化功能
- 统一配置界面
- 简化使用流程
- 提供迁移工具

#### 迁移步骤
1. 使用Unity编辑器菜单: `Tools/钻井可视化/组件迁移助手`
2. 扫描场景中的旧组件
3. 执行自动迁移
4. 测试新组件功能
5. 清理禁用的旧组件

### 添加新功能
1. 对于数据处理功能，修改`DrillingDataProcessor/Core/`中的核心逻辑
2. 对于可视化功能，**优先使用`UnifiedDrillingVisualizationEntry`**
3. 对于专门的可视化需求，修改`Assets/Scripts/`中的可视化组件
4. 避免修改`Assets/Script/`中的遗留脚本

### 调试和测试
- 使用Unity Console查看日志信息
- 在脚本中添加`Debug.Log`输出调试信息
- 使用`[ContextMenu]`属性添加测试按钮

### 性能优化
- 大数据集时关闭轨迹点显示
- 使用LOD系统处理复杂场景
- 启用对象池管理轨迹点

## 常见问题解决

### Burst编译错误
运行`fix_burst_compilation.bat`脚本，然后重新打开Unity编辑器。

### 轨迹不显示
1. 检查Excel文件路径是否正确
2. 确认数据处理是否成功
3. 验证相机位置和可视化器设置

### 性能问题
1. 减少轨迹点显示数量
2. 调整线宽和材质参数
3. 关闭不必要的可视化元素

## 文件说明

### 数据文件
- `Assets/Data/`: Unity项目数据文件
- `Assets/StreamingAssets/`: 运行时数据文件
- `DrillingDataProcessor/TestData/`: 测试数据文件

### 配置文件
- `Packages/manifest.json`: Unity包管理器配置
- `DrillingDataProcessor/DrillingDataProcessor.csproj`: C#项目文件

### 文档文件
- `README.md`: 项目主要说明文档
- `Unity三维轨迹可视化集成指南.md`: Unity集成详细指南
- `统一入口点使用指南.md`: **统一入口点详细使用说明**（重要）
- `DrillingDataProcessor/README.md`: 数据处理器说明文档

## 快速开始指南

### 新用户推荐流程
1. **创建可视化对象**
   - 在场景中创建空GameObject，命名为"DrillingVisualization"
   - 添加`UnifiedDrillingVisualizationEntry`组件

2. **配置基本参数**
   - 设置Excel数据路径: `Assets/Data/J16原始.xlsx`
   - 启用自动加载: `autoLoadOnStart = true`
   - 选择显示选项: 轨迹线、轨迹点、坐标轴等

3. **运行测试**
   - 运行场景，系统将自动初始化并显示三维轨迹
   - 使用鼠标拖拽旋转视角，滚轮缩放

4. **高级配置**
   - 设置材质和颜色方案
   - 调整性能参数（LOD控制）
   - 配置相机交互选项

### 从旧版本迁移
1. 打开Unity编辑器菜单: `Tools/钻井可视化/组件迁移助手`
2. 点击"扫描场景中的旧组件"
3. 检查找到的旧组件列表
4. 点击"执行自动迁移"
5. 测试新组件功能正常
6. 点击"清理禁用的旧组件"完成迁移

## 重要提醒

- **推荐使用**: `UnifiedDrillingVisualizationEntry` 作为唯一入口点
- **避免使用**: `Assets/Script/`目录中的旧版组件
- **迁移工具**: 使用`VisualizationMigrationHelper`进行安全迁移
- **详细文档**: 参考`统一入口点使用指南.md`获取完整说明