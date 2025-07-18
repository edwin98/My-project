@echo off
chcp 65001 >nul
echo 钻井数据处理器测试脚本
echo ============================

echo 检查.NET环境...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo 错误：未检测到.NET SDK
    echo 请从 https://dotnet.microsoft.com/download 下载并安装.NET 6.0或更高版本
    pause
    exit /b 1
)

echo .NET环境检查通过

echo.
echo 检查测试数据...
if not exist "TestData" (
    echo 错误：TestData目录不存在
    pause
    exit /b 1
)

echo 测试数据目录存在，列出文件：
dir TestData

echo.
echo 还原依赖包...
dotnet restore
if %errorlevel% neq 0 (
    echo 错误：依赖包还原失败
    pause
    exit /b 1
)

echo.
echo 编译项目...
dotnet build
if %errorlevel% neq 0 (
    echo 错误：项目编译失败
    pause
    exit /b 1
)

echo.
echo 运行数据处理器...
dotnet run

echo.
echo 检查输出文件...
if exist "Output\CSV" (
    echo ✓ CSV目录存在
    dir "Output\CSV"
)

if exist "Output\JSON" (
    echo ✓ JSON目录存在
    dir "Output\JSON"
)

echo.
echo 测试完成！
pause 