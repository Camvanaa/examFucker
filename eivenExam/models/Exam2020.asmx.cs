using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace Eiven.EXE.Web.Models
{
    /// <summary>
    /// Exam2020 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class Exam2020 : System.Web.Services.WebService
    {
        [WebMethod]
        public void Log(int uid, string text)
        {
            string ip = this.Context.Request.UserHostAddress;
            if (ip == null) ip = "";
            Db.Execute("INSERT INTO Exam2020Log (UID, IP, What) Values (" + uid + ", '" + ip.Replace("'", "''") + "', '" + text.Replace("'", "''") + "')");
            Db.Execute("UPDATE Exam2020Stu SET LastUpdate=GetDate() WHERE ID=" + uid);
        }

        private string GetUserExamKey(int uid)
        {
            if (userExamKeys != null && userExamKeys.ContainsKey(uid))
                return userExamKeys[uid];
            //if (Session != null)
            //{
            //    object ekobj = Session["ExamKey"];
            //    if (ekobj == null || ekobj.ToString() == "")
            //    {
            //        string ek = Db.GetString("SELECT ExamKey FROM Exam2020Stu WHERE id=" + uid);
            //        Session["ExamKey"] = ek;
            //        return ek;
            //    }
            //    else
            //        return ekobj.ToString();
            //}
            else
            {
                if (userExamKeys == null)
                {
                    Log(uid, "UEK IS NULL");
                    userExamKeys = new SortedList<int, string>();
                }
                return userExamKeys[uid] = Db.GetString("SELECT ExamKey FROM Exam2020Stu WHERE id=" + uid);
            }

        }

        private void SetUserExamKey(int uid, string ek)
        {
            if (userExamKeys == null)
                userExamKeys = new SortedList<int, string>();

            userExamKeys[uid] = ek;
        }

        static SortedList<int, string> userExamKeys;

        bool ConvertToBool(object val)
        {
            if (val == null) return false;
            if (val is bool) return (bool)val;
            if (val is int) return (int)val != 0;

            if (val is DBNull) return false;

            string s = val.ToString().ToLower().Trim();

            if (s == "") return false;
            if (s == "true" || s == "1" || s == "on" || s == "是" || s == "真")
                return true;
            int r;
            if (int.TryParse(s, out r) && r != 0)
                return true;

            return false;
        }

        [WebMethod]
        public DataTable Login(string username, string pwd)
        {
            if (username == null) username = "";


            DataRow row = Db.GetDataRow("SELECT *, 1 as Login, '' As Err FROM Exam2020Stu WHERE Code1='" + username.ToString() + "'");
            if (row != null)
                if (row["Password"].ToString() == pwd)
                {
                    string examKey = row["ExamKey"].ToString();

                    if (DateTime.Now > Db.GetDateTime("SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + examKey.Replace("'", "''") + "' AND Name='StopTime'"))
                    {
                        row["Login"] = false;
                        row["Err"] = "非考试期间，无法登录。";
                        Log(0, "用户“" + username + "”登录失败，非考试期间，无法登录。");
                        int uid = Convert.ToInt(row["ID"]);
                        SetUserExamKey(uid, examKey);
                    }
                    else if (ConvertToBool(row["Submitted"]))
                    {
                        row["Login"] = false;
                        row["Err"] = "试卷已经提交，无法登录。";
                        Log(0, "用户“" + username + "”登录失败，试卷已经提交，无法登录。");
                        int uid = Convert.ToInt(row["ID"]);
                        SetUserExamKey(uid, examKey);
                    }
                    else
                    {
                        row["Password"] = "*";
                        int uid = Convert.ToInt(row["ID"]);
                        Log(uid, "用户“" + username + "”登录成功。");
                        SetUserExamKey(uid, examKey);
                    }
                }
                else
                {
                    row["Login"] = false;
                    row["Password"] = "*";
                    row["Err"] = "用户名或者密码错误";
                    row["Login"] = 0;
                    Log(0, "用户“" + username + "”登录失败，密码错误。");
                }
            else
            {
                row = Db.GetDataRow("SELECT '' AS Code1, '' AS Code2, '' AS Name, '' AS Password, 0 as Login, '用户名不存在' As Err");
                Log(0, "用户“" + username + "”登录失败，用户名不存在。");
            }

            if (row.Table != null)
                row.Table.AcceptChanges();

            return row.Table;
        }

        [WebMethod]
        public DataTable GetExamTime(int uid)
        {
            string ek = GetUserExamKey(uid);
            DataTable dt = Db.GetDataTable(@"SELECT 
                (SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + ek + @"' AND Name='PreparingTime') AS PreparingTime,
                (SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + ek + @"' AND Name='StartTime')     AS StartTime,
                (SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + ek + @"' AND Name='EndingTime')    AS EndingTime,
                (SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + ek + @"' AND Name='EndTime')       AS EndTime,
                (SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + ek + @"' AND Name='StopTime')      AS StopTime,
                (SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + ek + @"' AND Name='QuestionCount') AS QuestionCount,
                GETDATE() AS Now ");
            Log(uid, "获取考试时间成功。");
            return dt;
        }

        [WebMethod]
        public string[] GetExamFiles(int uid)
        {
            List<string> files = new List<string>();
            DirectoryInfo di = new DirectoryInfo(GetFilder(GetFolderType.Paper, uid));

            foreach (FileInfo fi in di.GetFiles())
                files.Add(fi.Name);

            Log(uid, "获取试卷列表成功。");

            return files.ToArray();
        }

        [WebMethod]
        public byte[] GetExamFile(int uid, string filename)
        {
            string path = GetFilder(GetFolderType.Paper, uid);

            path += "\\" + filename;

            if (File.Exists(path))
            {
                byte[] bs = File.ReadAllBytes(path);

                Log(uid, "获取试卷文件“" + filename + "”成功。");
                return bs;
            }
            else
            {
                Log(uid, "获取试卷文件“" + filename + "”失败，该文件不存在。");
                return null;
            }
        }

        [WebMethod]
        public void SpyScreen(int uid, byte[] screen, int sIndex)
        {
            try
            {
                string path = GetFilder(GetFolderType.Spy, uid, sIndex);

                Directory.CreateDirectory(path);

                string recordPath = path + "\\" + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + ".jpg";

                File.WriteAllBytes(recordPath, screen);

                Db.Execute("UPDATE Exam2020Stu SET ScreenNext = ScreenNow WHERE ID=" + uid);
                Db.Execute("UPDATE Exam2020Stu SET ScreenNow  ='" + recordPath.Replace("'", "''") + "', LastUpdate=GetDate() WHERE ID=" + uid);
            }
            catch (System.Exception ex)
            {
                Log(uid, "保存客户端截屏错误：" + ex.Message);
            }
        }

        [WebMethod]
        public void Submit(int uid)
        {
            Db.Execute("UPDATE Exam2020Stu SET Submitted = 1 WHERE ID=" + uid);
        }

        bool IsSubmitted(int uid)
        {
            return Db.GetBool("SELECT Submitted FROM Exam2020Stu WHERE ID=" + uid);
        }

        //[WebMethod]
        //public void SpyScreenArea(int x, int y, byte[] screen, string clientName)
        //{
        //    try
        //    {
        //        string rootpath = "F:\\";

        //        string recordPath = rootpath + "\\RD\\" + clientName + "\\" + y + "." + x + "\\";

        //        Directory.CreateDirectory(recordPath);

        //        string recordFilename = recordPath + DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff") + ".jpg";

        //        File.WriteAllBytes(recordFilename, screen);

        //        // Db.Execute("UPDATE Exam2020Stu SET ScreenNext = ScreenNow WHERE ID=" + uid);
        //        // Db.Execute("UPDATE Exam2020Stu SET ScreenNow  ='" + recordPath.Replace("'", "''") + "', LastUpdate=GetDate() WHERE ID=" + uid);

        //        string[] files = Directory.GetFiles(recordPath);
        //        Array.Sort(files);
        //        for (int i = 0; i < files.Length - 1; ++i)
        //        {
        //            File.Delete(files[i]);
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        //Log(uid, "保存客户端截屏错误：" + ex.Message);

        //    }
        //}

        //public void SpyMouse(int x, int y, string clientName)
        //{
        //    Application[clientName + "_Mouse"] = new Point(x, y);
        //}

        [WebMethod]
        public void SpyProcesses(int uid, string[] processes)
        {
            try
            {
                string path = GetFilder(GetFolderType.Spy, uid);

                Directory.CreateDirectory(path);

                File.AppendAllText(path + "Processes.txt",
                    "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "] " +
                    string.Join(", ", processes) + System.Environment.NewLine);
            }
            catch (System.Exception ex)
            {
                Log(uid, "保存客户端进程列表错误：" + ex.Message);
            }
        }

        [WebMethod]
        public string ChangePassword(int uid, string oldPwd, string newPwd)
        {
            string op = Db.GetString("SELECT Password FROM Exam2020Stu WHERE ID=" + uid);
            if (op == oldPwd)
            {
                Db.Execute("UPDATE Exam2020Stu SET Password='" + newPwd.Replace("'", "''") + "' WHERE ID=" + uid);
                Log(uid, "修改密码成功。");
                return "OK";
            }
            else
            {
                Log(uid, "修改密码失败：原密码错误，无法修改密码。");
                return "原密码错误，无法修改密码。";
            }

        }

        [WebMethod]
        public DataTable GetDangerousProcesses(int uid)
        {
            DataTable dt = Db.GetDataTable("SELECT * FROM Exam2020DP Where Forbidden=1");
            Log(uid, "获取危险进程列表成功。");

            return dt;
        }

        enum GetFolderType
        {
            Paper,
            Answer,
            Spy
        }

        private enum ConfigTypeStr
        {
            PaperPath,
            AnswerPath,
        }

        private string GetExamConfigStr(ConfigTypeStr t, int uid)
        {
            string ek = GetUserExamKey(uid);
            return Db.GetString(@"SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + ek + "' AND Name='" + t.ToString() + "'");
        }


        private enum ConfigTypeStrPublic
        {
            MinVersion
        }

        private string GetExamConfigStr(ConfigTypeStrPublic t)
        {
            return Db.GetString(@"SELECT [Value] FROM Exam2020Config WHERE Name='" + t.ToString() + "'");
        }

        private enum ConfigTypeDT
        {
            PreparingTime,
            StartTime,
            EndingTime,
            EndTime,
            StopTime
        }

        private DateTime GetExamConfigDT(ConfigTypeDT t, int uid)
        {
            string ek = GetUserExamKey(uid);
            return Db.GetDateTime(@"SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + ek + "' AND Name='" + t.ToString() + "'");
        }

        private enum ConfigTypeInt
        {
            QuestionCount,
        }

        private int GetExamConfigInt(ConfigTypeInt t, int uid)
        {
            string ek = GetUserExamKey(uid);
            return Db.GetInt(@"SELECT [Value] FROM Exam2020Config WHERE ExamKey='" + ek + "' AND Name='" + t.ToString() + "'");
        }

        private string GetFilder(GetFolderType type, int uid, int aid = -1)
        {
            if (type == GetFolderType.Paper)
            {
                return GetExamConfigStr(ConfigTypeStr.PaperPath, uid);
            }
            else
            {
                string path = GetExamConfigStr(ConfigTypeStr.AnswerPath, uid);

                path += "\\" + type.ToString();

                string userCode = Db.GetString("Select Code2 FROM Exam2020Stu WHERE ID=" + uid);
                string userName = Db.GetString("Select Name FROM Exam2020Stu WHERE ID=" + uid);
                string userRoom = Db.GetString("Select Room FROM Exam2020Stu WHERE ID=" + uid);

                if (userRoom != null && userRoom != "")
                    path += "\\" + userRoom;

                path += "\\" + userCode + " " + userName;

                if (type == GetFolderType.Spy)
                    if (aid < 0)
                        return path + "\\Processes\\";
                    else
                        return path + "\\Screen" + aid.ToString("00") + "\\";
                else
                    return path + "\\A" + aid.ToString("00") + "\\";
            }
        }

        [WebMethod]
        public string UploadAnswer(int uid, int aid, string filename, byte[] file)
        {
            string ek = GetUserExamKey(uid);
            string path = "";
            try
            {
                //时间检查
                if (DateTime.Now > GetExamConfigDT(ConfigTypeDT.StopTime, uid))
                    throw new Exception("考试已结束，无法再上传文件。");

                if (IsSubmitted(uid))
                    throw new Exception("试卷已经提交，无法再上传文件。");

                //后缀检查
                string ext = Path.GetExtension(filename).ToLower().Replace(".", "");

                if (ext == "dsw" || ext == "dsp" || ext == "vcproj" || ext == "vcprojx" ||
                    ext == "sln" || ext == "slu")
                    throw new Exception("本次考试不接收项目文件，请上传cpp程序文件或word文档。");

                path = GetFilder(GetFolderType.Answer, uid, aid);

                Directory.CreateDirectory(path);
                File.WriteAllBytes(path + filename, file);

                Log(uid, "上传文件“" + path + "”成功。");
                Db.Execute("UPDATE Exam2020Stu SET F" + aid + "=1 WHERE ID=" + uid);
                return "OK";
            }
            catch (Exception ex)
            {
                Log(uid, "上传文件“" + path + "”失败，" + ex.Message);
                return "上传文件错误：" + ex.Message;
            }
        }

        [WebMethod]
        public byte[] GetFileData(string path)
        {
            return File.ReadAllBytes(path);
        }

        [WebMethod]
        public void RemoveFile(string path)
        {
            File.Delete(path);
        }

        [WebMethod]
        public DataTable GetUploadFiles(int uid)
        {
            DataTable dt = new DataTable("UploadeFiles");
            dt.Columns.Add(new DataColumn("AID", typeof(int)));
            dt.Columns.Add(new DataColumn("Filename", typeof(string)));
            dt.Columns.Add(new DataColumn("Size", typeof(int)));
            dt.Columns.Add(new DataColumn("Datetime", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("FullPath", typeof(string)));
            try
            {
                for (int aid = 1; aid <= 6; ++aid)
                {
                    string path = GetFilder(GetFolderType.Answer, uid, aid);

                    if (Directory.Exists(path))
                        foreach (FileInfo fi in new DirectoryInfo(path).GetFiles())
                        {
                            dt.Rows.Add(aid, fi.Name, fi.Length, fi.LastWriteTime, fi.FullName);
                        }
                }

                Log(uid, "获取已上传文件列表成功。");
                return dt;
            }
            catch (System.Exception ex)
            {
                dt.Columns.Add("Error", typeof(string));
                dt.Rows.Add(0, "", 0, DBNull.Value, ex.Message);

                Log(uid, "获取已上传文件列表失败：" + ex.Message);
                return dt;
            }
        }

        [WebMethod]
        public string CheckVersion(string ver)
        {
            //return "OK";
            string minVer = GetExamConfigStr(ConfigTypeStrPublic.MinVersion);

            if (string.Compare(ver, minVer) >= 0)
            {
                Log(0, "版本对比成功，客户端版本：" + ver + "，要求版本：" + minVer + "。");
                return "OK";
            }
            else
            {
                Log(0, "版本对比失败，客户端版本：" + ver + "，要求版本：" + minVer + "。");
                return "请下载最新版本再登录。最新版本要求为：" + minVer;
            }
        }
    }
}
