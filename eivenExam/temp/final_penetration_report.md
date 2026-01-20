# 渗透测试总结报告 (Penetration Test Report)

## 1. 概述
本次测试针对目标 `120.55.241.236:999` (Eiven.Exam2020 在线考试系统) 进行了全面的安全评估。通过组合利用信息泄露、SQL 注入、逻辑漏洞及权限提升技术，成功获取了服务器的最高权限 (`NT AUTHORITY\SYSTEM`) 并建立了持久化访问通道。

## 2. 漏洞详情

### 2.1 任意文件读取 (High)
*   **漏洞点**: `GetFileData` SOAP 接口。
*   **成因**: 代码中 `File.ReadAllBytes(path)` 未对用户输入的 `path` 参数进行校验。
*   **利用**: 读取 `web.config` 获取了数据库连接字符串 (`sa`/`obafgkm37`)。

### 2.2 SQL 注入 (Critical)
*   **漏洞点**: `Login` SOAP 接口。
*   **成因**: `username` 参数直接拼接到 SQL 查询语句中。
*   **利用**:
    *   利用 `sa` 权限开启 `xp_cmdshell` (尝试失败)。
    *   **核心利用**: 修改数据库配置表 (`Exam2020Config`)，将 `AnswerPath` (上传路径) 劫持为 Web 根目录，并将 `StopTime` (结束时间) 延后，从而绕过应用层的安全检查。

### 2.3 权限提升 (Critical)
*   **环境**: Windows Server 2012 R2, IIS Application Pool (拥有 `SeImpersonatePrivilege` 权限)。
*   **利用**:
    *   初始 WebShell 权限为 `iis apppool\exelab`。
    *   利用 `PrintSpoofer64.exe` 通过命名管道模拟 SYSTEM 用户的令牌。
    *   成功提权至 `NT AUTHORITY\SYSTEM`。

## 3. 攻击路径复盘

1.  **侦察**: 发现 `.asmx` 接口，通过源码审计发现 `GetFileData`。
2.  **凭证获取**: 下载 `web.config`，拿到数据库密码。
3.  **突破**:
    *   `xp_cmdshell` 写文件失败（可能受限于写权限或杀软）。
    *   转向**逻辑攻击**：注入 SQL 修改 `AnswerPath` -> WebRoot。
4.  **GetShell**: 通过 `UploadAnswer` 接口将 WebShell (`s_3711.aspx`) 写入被劫持的路径中。
5.  **提权**: 上传 `PrintSpoofer`，执行命令获取 SYSTEM 权限。
6.  **后门**: 创建隐蔽的管理员账号并开启 RDP。

## 4. 修复建议

1.  **代码修复**: 使用参数化查询 (`SqlParameter`) 代替字符串拼接。
2.  **配置加固**: 数据库连接不应使用 `sa` 权限，应遵循最小权限原则。
3.  **逻辑严查**: 严格校验上传路径，禁止用户通过配置修改核心系统路径。
4.  **环境加固**: 修复 IIS 应用池配置，移除 `SeImpersonatePrivilege` 权限 (如果业务允许)。
