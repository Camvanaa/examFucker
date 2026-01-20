import requests
import xml.etree.ElementTree as ET

# ================= 配置区域 =================
TARGET_URL = "http://120.55.241.236:999/Models/Exam2020.asmx"
HEADERS = {
    "Content-Type": "text/xml; charset=utf-8",
    "User-Agent": "Eiven.Exam2021 Client"
}

def check_uploaded_files():
    print("=== 查询已上传文件列表 (定位真实路径) ===")

    # 调用 GetUploadFiles 接口
    # 我们可以通过这个接口看到文件的 FullPath，从而知道文件到底传哪儿去了
    soap_payload = """<?xml version="1.0" encoding="utf-8"?>
    <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
      <soap:Body>
        <GetUploadFiles xmlns="http://tempuri.org/">
          <uid>9207</uid>
        </GetUploadFiles>
      </soap:Body>
    </soap:Envelope>"""

    try:
        headers = HEADERS.copy()
        headers["SOAPAction"] = "http://tempuri.org/GetUploadFiles"

        response = requests.post(TARGET_URL, data=soap_payload.encode('utf-8'), headers=headers, timeout=10)

        if response.status_code == 200:
            # 打印完整响应以供调试
            print("\n[DEBUG] Server Response Body:")
            print(response.text)

            # 解析 XML
            try:
                root = ET.fromstring(response.text)
                # DataTables 通常被序列化在 diffgram 中
                if "FullPath" in response.text:
                    print("\n[Found Files]")
                    # 简单粗暴地按行分割或查找
                    lines = response.text.split("</FullPath>")
                    for line in lines[:-1]:
                        path_part = line.split("<FullPath>")[-1]
                        filename = line.split("<Filename>")[-1].split("</Filename>")[0] if "<Filename>" in line else "Unknown"
                        print(f"    File: {filename}")
                        print(f"    Path: {path_part}")
                        print("-" * 30)
                else:
                    print("[-] 未在响应中找到 FullPath 字段。")
            except Exception as parse_error:
                print(f"[!] XML Parsing Error: {parse_error}")

        else:
            print(f"[-] 请求失败: {response.status_code}")
            print("\n[DEBUG] Error Response:")
            print(response.text)

    except Exception as e:
        print(f"[!] 异常: {e}")

if __name__ == "__main__":
    check_uploaded_files()
