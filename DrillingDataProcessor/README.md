# 钻井数据处理器 - 控制台版本

这是从Unity项目中提取的纯C#数据处理器，用于处理钻井Excel数据并生成分析结果。

## 功能特性

- **Excel数据读取**: 支持读取XLSX格式的钻井数据
- **数据过滤**: 根据配置参数过滤无效数据
- **标记行检测**: 智能检测Excel中的标记行（黄色标记）
- **深度插值**: 在标记行之间进行线性深度插值
- **FPI/UCS计算**: 计算贯入指数和煤岩强度
- **轨迹计算**: 计算钻孔轨迹位移和坐标
- **图表数据生成**: 生成JSON格式的图表数据
- **结果导出**: 导出CSV和JSON格式的处理结果

## 项目结构

```
DrillingDataProcessor/
├── DrillingDataProcessor.csproj    # 项目文件
├── Program.cs                      # 主程序入口
├── README.md                       # 说明文档
├── Models/
│   └── DrillingModels.cs          # 数据模型定义
├── Core/
│   └── DrillingDataProcessorCore.cs # 核心处理逻辑
├── TestData/                       # 测试数据目录
│   └── J16原始.xlsx               # 示例Excel文件
└── Output/                         # 输出目录
    ├── CSV/                       # CSV结果文件
    └── JSON/                      # JSON图表数据
```

## 安装和运行

### 前提条件

- .NET 6.0 或更高版本
- Visual Studio 2022 或 VS Code

### 步骤

1. **克隆或复制项目文件**

2. **还原NuGet包**
   ```bash
   dotnet restore
   ```

3. **准备测试数据**
   - 将Excel文件复制到 `TestData/J16原始.xlsx`
   - 确保Excel文件格式符合要求（见下方格式说明）

4. **编译项目**
   ```bash
   dotnet build
   ```

5. **运行程序**
   ```bash
   dotnet run
   ```

## Excel文件格式要求

Excel文件应包含以下列（按顺序）：

| 列号 | 列名     | 数据类型 | 说明           |
|------|----------|----------|----------------|
| A    | 原序号   | 整数     | 原始数据序号   |
| B    | 日期     | 文本     | 格式：YYYY-MM-DD |
| C    | 时间     | 文本     | 格式：HH:mm:ss |
| D    | 扭矩     | 数值     | 钻井扭矩       |
| E    | 钻压     | 数值     | 钻井压力       |
| F    | 转速     | 数值     | 旋转速度       |
| G    | 温度     | 数值     | 温度值         |
| H    | 倾角     | 数值     | 倾斜角度       |
| I    | 方位角   | 数值     | 方位角度       |
| J    | 重力和   | 数值     | 重力值         |
| K    | 磁强     | 数值     | 磁场强度       |
| L    | 磁倾     | 数值     | 磁倾角         |
| M    | 电压     | 数值     | 电压值         |

## 配置参数

可以在 `Program.cs` 的 `ConfigureProcessor` 方法中修改处理参数：

### 文件路径
- `InputExcelPath`: 输入Excel文件路径
- `OutputPath`: CSV输出目录
- `JsonOutputPath`: JSON输出目录

### 处理参数
- `DepthStep`: 深度步长（默认1.5m）
- `DepthInterval`: 深度间隔（默认0.2m）
- `MagneticDeclination`: 磁偏角（默认0°）
- `SurveyLineTrueAzimuth`: 测线真方位角（默认90°）

### 过滤条件
- `MinRotationSpeed`: 最小转速（默认10）
- `MinDrillPressure`: 最小钻压（默认200）
- `MinTorque`: 最小扭矩（默认200）
- `GravityMin/Max`: 重力值范围（默认0.98-1.02）

### 标记行检测
- `UseAdvancedMarkDetection`: 是否使用高级检测（默认true）
- `MarkGravityTarget`: 标记行重力目标值（默认1.0）
- `MarkGravityTolerance`: 重力值容差（默认0.001）

## 输出文件

程序将生成以下输出文件：

### CSV文件（Output/CSV/）
- `J16原始_depth_FPI_UCS.csv`: 主数据文件，包含深度、FPI、UCS等计算结果
- `trajectory_points.csv`: 轨迹点数据
- `trajectory_calculated.csv`: 轨迹计算结果
- `mark_data.csv`: 标记行数据

### JSON文件（Output/JSON/）
- `fpi_analysis.json`: FPI分析图表数据
- `drilling_parameters.json`: 钻井参数图表数据
- `trajectory_analysis.json`: 轨迹分析图表数据

## 集成回Unity项目

完成控制台测试后，可以将核心处理逻辑集成回Unity项目：

1. **复制核心类**
   - 将 `DrillingDataProcessorCore.cs` 复制到Unity项目的 `Assets/Script/` 目录
   - 将 `DrillingModels.cs` 中的模型类复制到Unity项目

2. **创建Unity包装类**
   ```csharp
   public class UnityDrillingDataProcessor : MonoBehaviour
   {
       private DrillingDataProcessorCore coreProcessor;
       
       void Start()
       {
           coreProcessor = new DrillingDataProcessorCore();
           // 配置参数...
           // 订阅事件...
       }
       
       [ContextMenu("处理数据")]
       public void ProcessData()
       {
           coreProcessor.ProcessDrillingDataAndGenerateCharts();
       }
   }
   ```

3. **处理Unity特定功能**
   - 使用 `Debug.Log` 替代 `Console.WriteLine`
   - 添加 `[Header]` 和 `[ContextMenu]` 属性
   - 集成文件监控功能（如需要）

## 依赖包

- **DotNetCore.NPOI**: Excel文件读取
- **Newtonsoft.Json**: JSON数据序列化

## 许可证

此项目基于原Unity项目提取，请遵循相应的许可协议。

## 支持

如有问题或建议，请联系开发团队。 