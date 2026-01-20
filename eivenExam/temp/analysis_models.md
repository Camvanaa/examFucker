# Models 目录代码安全分析报告

通过分析 `models` 目录下的源代码（主要是 `Exam2020.asmx.cs`），发现存在多个高危安全漏洞。

## 1. 任意文件读取 (Arbitrary File Read)
**位置**: `Exam2020.asmx.cs` -> `GetFileData` 方法 (Line 436)
**代码**:
```csharp
[WebMethod]
public byte[] GetFileData(string path)
{
    return File.ReadAllBytes(path);
}
```
**分析**: 该方法直接接受用户提供的 `path` 参数并读取文件内容，没有任何路径验证或权限检查。攻击者可以读取服务器上的任意文件（如 `web.config`），从而获取数据库凭证等敏感信息。
**Proof of Concept**: `poc.py` 已经利用了此漏洞。

## 2. 任意文件删除 (Arbitrary File Delete)
**位置**: `Exam2020.asmx.cs` -> `RemoveFile` 方法 (Line 442)
**代码**:
```csharp
[WebMethod]
public void RemoveFile(string path)
{
    File.Delete(path);
}
```
**分析**: 类似于读取漏洞，该方法允许攻击者删除服务器上的任意文件，可能导致拒绝服务 (DoS) 或破坏系统。

## 3. 文件上传漏洞 (File Upload Vulnerability)
**位置**: `Exam2020.asmx.cs` -> `UploadAnswer` 方法 (Line 399)
**代码**:
```csharp
[WebMethod]
public string UploadAnswer(int uid, int aid, string filename, byte[] file)
{
    // ...
    // 后缀检查 (黑名单)
    string ext = Path.GetExtension(filename).ToLower().Replace(".", "");
    if (ext == "dsw" || ext == "dsp" || ...) // 只禁止了项目文件
        throw new Exception(...);

    path = GetFilder(GetFolderType.Answer, uid, aid);
    Directory.CreateDirectory(path);
    File.WriteAllBytes(path + filename, file); // 路径拼接
    // ...
}
```
**分析**:
1.  **黑名单绕过**: 仅禁止了部分 VS 项目文件后缀，未禁止 `.aspx`, `.ashx`, `.config`, `.exe` 等可执行文件后缀。
2.  **路径遍历**: `File.WriteAllBytes` 使用 `path + filename`。如果 `filename` 包含路径遍历字符（如 `..\..\shell.aspx`），可能允许文件写入到预期目录之外。
    *   注意：代码逻辑中 `path` 来自 `GetFilder`（通常是 `F:\Exam2020\Answer\...`）。如果尝试使用绝对路径（如 `e:\web\shell.aspx`）作为 `filename`，在 `File.WriteAllBytes` 中进行拼接可能会导致路径无效异常（取决于 .NET 版本和操作系统行为）。但在某些情况下（或者如果 `path` 为空），可能成功。
    *   更可靠的利用方式是利用目录遍历将 WebShell 上传到 Web 根目录（如果 Web 根目录与 `path` 在同一驱动器且可达）。

## 4. SQL 注入 (SQL Injection)
**位置**: `Exam2020.asmx.cs` -> `Login` 方法 (Line 90)
**代码**:
```csharp
[WebMethod]
public DataTable Login(string username, string pwd)
{
    // ...
    DataRow row = Db.GetDataRow("SELECT *, 1 as Login, '' As Err FROM Exam2020Stu WHERE Code1='" + username.ToString() + "'");
    // ...
}
```
**分析**: `username` 参数直接拼接到 SQL 查询字符串中，且没有经过任何过滤或参数化查询处理。
**危害**:
*   **认证绕过**: 使用 `admin' --` 可无需密码登录。
*   **数据泄露**: 通过 `UNION SELECT` 查询其他表。
*   **远程代码执行 (RCE)**: 由于数据库连接使用 `sa` 用户（从 `web.config` 可知），攻击者可以通过注入开启 `xp_cmdshell` 并执行系统命令，这是获取远程 Shell 最直接、最可靠的方法。
*   **文件写入**: 也可以利用 `xp_cmdshell` 或 `sp_makewebtask` 写入 WebShell。

## 总结与建议
该系统存在严重的安全隐患。
*   **推荐利用路径**: 首选 SQL 注入漏洞 (`Login` 方法) 获取 RCE，因为 `sa` 权限极高且不受文件路径拼接限制。
*   **备选利用路径**: 文件上传 (`UploadAnswer`) 配合路径遍历上传 WebShell，但受限于目录结构和驱动器限制。
