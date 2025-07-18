@echo off
echo Unity Burst 编译错误修复脚本
echo ================================
echo.

echo 正在检查当前目录...
if not exist "Library" (
    echo 错误：未找到 Library 文件夹，请确保在 Unity 项目根目录运行此脚本
    pause
    exit /b 1
)

echo 正在备份当前的 ScriptAssemblies 文件夹...
if exist "Library\ScriptAssemblies_backup" (
    echo 删除旧的备份文件夹...
    rmdir /s /q "Library\ScriptAssemblies_backup"
)
if exist "Library\ScriptAssemblies" (
    echo 创建备份...
    xcopy "Library\ScriptAssemblies" "Library\ScriptAssemblies_backup" /e /i /h
)

echo.
echo 正在清理编译缓存...
if exist "Library\ScriptAssemblies" (
    echo 删除 ScriptAssemblies 文件夹...
    rmdir /s /q "Library\ScriptAssemblies"
)

if exist "Library\Bee" (
    echo 删除 Bee 文件夹...
    rmdir /s /q "Library\Bee"
)

echo.
echo 正在检查可能有问题的编辑器脚本...

if exist "Assets\Plugins\Editor\HighlightingSystemEditor.cs" (
    echo 发现 HighlightingSystemEditor.cs，建议临时禁用...
    echo 请手动将其重命名为 HighlightingSystemEditor.cs.bak
)

if exist "Assets\WebGLSupport\Editor\Postprocessor.cs" (
    echo 发现 Postprocessor.cs，脚本看起来正常
)

echo.
echo 清理完成！
echo.
echo 下一步操作：
echo 1. 重新打开 Unity 编辑器
echo 2. 等待项目重新编译
echo 3. 检查 Unity Console 是否还有错误
echo.
echo 如果问题仍然存在，请尝试：
echo - 禁用 Burst 编译（Jobs > Burst > Enable Compilation）
echo - 更新第三方插件到兼容 Unity 6000 的版本
echo - 检查 Unity Console 中的具体错误信息
echo.

pause 