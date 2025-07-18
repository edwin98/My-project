@echo off
echo =================================================
echo      钻井数据处理与空间FPI插值系统
echo =================================================
echo.

:: 设置控制台编码为UTF-8
chcp 65001 > nul

:: 检查.NET运行时
echo 检查.NET运行时环境...
dotnet --version > nul 2>&1
if %errorlevel% neq 0 (
    echo 错误: 未找到 .NET 运行时，请先安装 .NET 6.0 或更高版本
    pause
    exit /b 1
)

:: 构建项目
echo 构建数据处理器...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo 错误: 构建失败
    pause
    exit /b 1
)

:: 设置输入输出路径
set INPUT_FILE=TestData\J16原始.xlsx
set OUTPUT_DIR=Output\WithSpatialInterpolation
set CONFIG_SPATIAL=true

:: 检查输入文件
if not exist "%INPUT_FILE%" (
    echo 警告: 输入文件 %INPUT_FILE% 不存在
    echo 将使用示例模式运行...
    set INPUT_FILE=
)

:: 创建输出目录
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

echo.
echo 运行参数:
echo - 输入文件: %INPUT_FILE%
echo - 输出目录: %OUTPUT_DIR%
echo - 空间插值: 启用
echo.

:: 运行数据处理（带空间插值）
echo 开始数据处理...
dotnet run --project . --configuration Release -- ^
    --input "%INPUT_FILE%" ^
    --output "%OUTPUT_DIR%" ^
    --spatial-interpolation true ^
    --spatial-resolution 0.5 ^
    --influence-radius 5.0 ^
    --min-neighbors 3 ^
    --boundary-expansion 2.0

if %errorlevel% equ 0 (
    echo.
    echo =================================================
    echo           数据处理完成！
    echo =================================================
    echo.
    echo 输出文件位置:
    echo - 基础处理结果: %OUTPUT_DIR%\JSON\
    echo - 轨迹数据: %OUTPUT_DIR%\3D\
    echo - 空间插值结果: %OUTPUT_DIR%\SpatialInterpolation\
    echo.
    echo 主要输出文件:
    echo [轨迹数据]
    echo - %OUTPUT_DIR%\3D\trajectory_data.json
    echo.
    echo [FPI分析]  
    echo - %OUTPUT_DIR%\JSON\fpi_analysis.json
    echo.
    echo [空间插值]
    echo - %OUTPUT_DIR%\SpatialInterpolation\spatial_fpi_interpolation.json
    echo - %OUTPUT_DIR%\SpatialInterpolation\spatial_fpi_visualization.json
    echo - %OUTPUT_DIR%\SpatialInterpolation\interpolation_summary.txt
    echo.
    
    :: 询问是否打开输出目录
    choice /M "是否打开输出目录查看结果"
    if %errorlevel% equ 1 (
        start "" "%OUTPUT_DIR%"
    )
) else (
    echo.
    echo =================================================
    echo           数据处理失败！
    echo =================================================
    echo 请检查错误信息并重试
)

echo.
pause 