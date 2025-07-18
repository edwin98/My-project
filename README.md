# 钻井数据处理与三维可视化系统

## 项目概述

这是一个完整的钻井数据处理与三维轨迹可视化系统，包含数据处理器和Unity三维可视化组件。系统能够读取Excel格式的钻井数据，进行智能分析处理，并在Unity中提供交互式三维轨迹可视化。

## 主要功能

### 🔧 数据处理功能
- **Excel数据读取**: 支持XLSX格式钻井数据导入
- **智能数据过滤**: 根据设定参数过滤无效数据点
- **标记行检测**: 自动识别Excel中的黄色标记行
- **深度插值计算**: 标记行间线性深度插值
- **FPI/UCS计算**: 贯入指数和煤岩强度计算
- **轨迹计算**: 钻孔三维轨迹和位移分析
- **多格式导出**: CSV数据和JSON图表格式输出

### 🎮 Unity可视化功能
- **三维轨迹显示**: 实时三维轨迹线和轨迹点可视化
- **交互式控制**: 鼠标拖拽旋转、滚轮缩放、中键平移
- **多种颜色方案**: 按深度、倾角、方位角着色
- **UI控制面板**: 完整的可视化参数控制界面
- **实时数据更新**: 支持Excel文件自动监控和更新
- **点击交互**: 点击轨迹点查看详细信息

## 快速开始

### 🚀 Unity三维可视化

1. **自动场景设置**（推荐）
   ```csharp
   // 在空场景中创建GameObject并添加组件
   GameObject setupObject = new GameObject("AutoSetup");
   setupObject.AddComponent<Trajectory3DSceneSetup>();
   
   // 运行场景，系统会自动创建所有必要组件
   ```

2. **准备数据**
   - 将Excel数据文件放置在项目中
   - 在`UnityDrillingDataProcessor`组件中配置文件路径

3. **开始可视化**
   - 运行场景
   - 使用UI控制面板控制可视化参数
   - 通过鼠标交互操作视角

### 💻 独立数据处理

```bash
# 进入数据处理器目录
cd DrillingDataProcessor

# 安装依赖
dotnet restore

# 编译项目
dotnet build

# 运行处理
dotnet run
```

## 系统要求

### Unity版本
- **Unity 2022.3 LTS** 或更高版本
- **内置渲染管线** (Built-in Render Pipeline)
- **平台支持**: Windows, macOS, Linux

### 开发环境
- **.NET 6.0 SDK** 或更高版本
- **Visual Studio 2022** 或 **VS Code**

### 硬件要求
- **内存**: 4GB+ RAM（推荐8GB+）
- **显卡**: 支持DirectX 11的独立显卡
- **存储**: 2GB+ 可用空间

## 项目结构

```
📁 项目根目录/
├── 📁 Assets/Script/                     # Unity脚本
│   ├── 📄 UnityDrillingDataProcessor.cs  # 数据处理器Unity集成
│   ├── 📄 Trajectory3DVisualizer.cs      # 三维可视化器
│   ├── 📄 Trajectory3DController.cs      # UI控制器
│   ├── 📄 Trajectory3DSceneSetup.cs      # 自动场景设置
│   └── 📄 Trajectory3DTest.cs            # 测试脚本
├── 📁 DrillingDataProcessor/              # 独立数据处理器
│   ├── 📄 README.md                      # 数据处理器说明
│   ├── 📄 Program.cs                     # 控制台程序入口
│   ├── 📁 Core/                          # 核心处理逻辑
│   ├── 📁 Models/                        # 数据模型
│   └── 📁 Output/                        # 处理结果输出
├── 📄 README.md                          # 本文档
├── 📄 Unity三维轨迹可视化集成指南.md      # Unity详细使用指南
└── 📄 项目完成总结.md                    # 项目开发总结
```

## 文档导航

| 文档 | 描述 | 目标用户 |
|------|------|----------|
| [**Unity三维轨迹可视化集成指南**](Unity三维轨迹可视化集成指南.md) | Unity中展示轨迹的完整指南 | Unity开发者 |
| [**数据处理器说明**](DrillingDataProcessor/README.md) | 独立数据处理器使用说明 | 数据分析人员 |
| [**项目完成总结**](项目完成总结.md) | 项目开发历程和技术总结 | 项目管理者 |

## 核心组件

### 1. 数据处理器 (`UnityDrillingDataProcessor`)
```csharp
// 获取数据处理器并处理数据
var processor = FindObjectOfType<UnityDrillingDataProcessor>();
processor.ProcessDrillingDataAndGenerateCharts();

// 获取处理后的轨迹点
List<TrajectoryPoint> points = processor.GetTrajectoryPoints();
```

### 2. 三维可视化器 (`Trajectory3DVisualizer`)
```csharp
// 获取可视化器并更新显示
var visualizer = FindObjectOfType<Trajectory3DVisualizer>();
visualizer.UpdateFromProcessor();

// 控制显示选项
visualizer.showTrajectoryLine = true;
visualizer.showTrajectoryPoints = true;
visualizer.colorScheme = Trajectory3DVisualizer.ColorScheme.Depth;
```

### 3. UI控制器 (`Trajectory3DController`)
```csharp
// 获取控制器并设置引用
var controller = FindObjectOfType<Trajectory3DController>();
controller.SetDataProcessor(processor);
controller.SetVisualizer(visualizer);
```

## 使用示例

### 基本轨迹显示
```csharp
public class TrajectoryExample : MonoBehaviour
{
    void Start()
    {
        // 1. 创建数据处理器
        var processor = gameObject.AddComponent<UnityDrillingDataProcessor>();
        processor.inputExcelPath = "Assets/Data/drilling_data.xlsx";
        
        // 2. 创建可视化器
        var visualizer = gameObject.AddComponent<Trajectory3DVisualizer>();
        visualizer.dataProcessor = processor;
        
        // 3. 处理数据并显示
        processor.ProcessDrillingDataAndGenerateCharts();
        visualizer.UpdateFromProcessor();
    }
}
```

### 交互控制
```csharp
void Update()
{
    // 键盘快捷键
    if (Input.GetKeyDown(KeyCode.R)) visualizer.ResetView();
    if (Input.GetKeyDown(KeyCode.Space)) visualizer.ToggleAutoRotate();
    if (Input.GetKeyDown(KeyCode.L)) visualizer.ToggleTrajectoryLine();
}
```

## 数据格式

### Excel输入格式
| 列名 | 类型 | 说明 |
|------|------|------|
| 日期 | 文本 | YYYY-MM-DD |
| 时间 | 文本 | HH:mm:ss |
| 扭矩 | 数值 | 钻井扭矩 |
| 钻压 | 数值 | 钻井压力 |
| 转速 | 数值 | 旋转速度 |
| 倾角 | 数值 | 倾斜角度 |
| 方位角 | 数值 | 方位角度 |
| 重力和 | 数值 | 重力值 |
| 深度 | 数值 | 钻井深度 |

### 轨迹点数据结构
```csharp
public class TrajectoryPoint
{
    public DateTime markTimestamp;      // 标记时间
    public float inclination;           // 倾角
    public float azimuth;              // 方位角
    public float eastDisplacement;     // 东向位移
    public float northDisplacement;    // 北向位移
    public float verticalDepth;        // 垂直深度
    public float rodLength;            // 杆长
    // ... 其他属性
}
```

## 性能优化建议

### 大数据集处理
- 🔸 **减少轨迹点显示**: 对于>1000个点的数据集，建议关闭轨迹点显示
- 🔸 **调整线宽**: 减小线宽可提高渲染性能
- 🔸 **关闭标签**: 大数据集时关闭深度标签显示
- 🔸 **分批加载**: 使用协程分批处理大文件

### 内存管理
- 🔸 **及时清理**: 数据更新时自动清理旧的可视化对象
- 🔸 **资源复用**: 复用材质和几何体对象
- 🔸 **缓存优化**: 启用深度分组缓存机制

## 故障排除

### 常见问题

**Q: 轨迹不显示？**
- ✅ 检查Excel文件路径是否正确
- ✅ 确认数据处理是否成功完成
- ✅ 验证相机位置和角度设置

**Q: 性能较慢？**
- ✅ 减少轨迹点数量或关闭轨迹点显示
- ✅ 调整线宽和点大小参数
- ✅ 关闭不需要的可视化元素

**Q: 交互不响应？**
- ✅ 确认交互控制已启用
- ✅ 检查UI EventSystem是否存在
- ✅ 验证相机组件配置

## 开发路线图

### 近期计划
- [ ] **VR支持**: 适配VR设备进行沉浸式查看
- [ ] **多轨迹对比**: 同时显示多个钻孔轨迹
- [ ] **数据筛选器**: 按时间、深度等条件过滤数据
- [ ] **动画回放**: 钻井过程的时间轴动画

### 长期规划
- [ ] **Web版本**: WebGL版本在线查看
- [ ] **移动端优化**: iOS/Android平台适配
- [ ] **云端处理**: 大数据云端处理方案
- [ ] **AI分析**: 集成机器学习分析功能

## 许可证

本项目遵循 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 贡献指南

欢迎提交Issue和Pull Request来改进项目。请确保：
- 代码符合项目编码规范
- 添加必要的测试用例
- 更新相关文档

## 联系我们

- 📧 **邮箱**: [项目邮箱]
- 📞 **电话**: [联系电话]
- 🌐 **网站**: [项目网站]

---

**最后更新**: 2024年12月
**版本**: v2.0 