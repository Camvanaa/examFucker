using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.UI;

/// <summary>
/// Summary description for Common
/// </summary>
namespace Eiven.EXE.Web.Models
{
    public static class Common
    {
        public const string SESSION_NAME_CURRENT_USER = "_CURRENT_USER_";
        public const string SESSION_NAME_LOGIN_CHECK = "_CURRENT_LOGIN_CHECK_";
        public const string SESSION_NAME_URL_BEFORE_LOGIN = "_CURRENT_URL_BEFORE_LOGIN_";

        public static int[] GetIPIntArr(string ip)
        {
            return ToIntArr(ip.Trim().Split('.'));
        }

        public static int[] ToIntArr(string[] sa)
        {
            List<int> l = new List<int>();
            foreach (string s in sa)
                l.Add(MyConvert.ToInt(s));
            return l.ToArray();
        }
        public static string ToJsStringScript(string s)
        {
            if (s == null) return null;
            return s
               .Replace("\\", "\\x5C")
               .Replace("\"", "\\x22")
               .Replace("\'", "\\x27")
               .Replace("\n", "\\x0A")
               .Replace("\r", "\\x0D");
        }

        //public static bool InSchool(string ip)
        //{
        //    return FindIP(ip, 'S');
        //}

        //public static bool InChina(string ip)
        //{
        //    return FindIP(ip, 'F');
        //}

        //public static bool FindIP(string ipAddress, char type)
        //{
        //    int[] ip = GetIPIntArr(ipAddress);
        //    if (ip.Length != 4) return false;

        //    string sql = string.Format("SELECT Count(*) FROM IP Where " +
        //        "({0} BETWEEN s1 and e1) AND " +
        //        "({1} BETWEEN s2 and e2) AND " +
        //        "({2} BETWEEN s3 and e3) AND " +
        //        "({3} BETWEEN s4 and e4) AND " +
        //        "type='{4}'", ip[0], ip[1], ip[2], ip[3], type);

        //    return Db.GetInt(sql) > 0;
        //}

        //public static int CIIPVisitCount 
        //{
        //    get
        //    {
        //        return Db.GetInt(ConnnectionType.News, "Select [Value] From visitCount Where Name='CIIP'");
        //    }
        //    set
        //    {
        //        Db.Execute(ConnnectionType.News, "Update visitcount set [value]=" + value + " Where Name='CIIP'");
        //    }
        //}

        static Random rnd = new Random();
        public static int Rnd()
        {
            return rnd.Next();
        }

        private static decimal GetDateIndex(DateTime date)
        {
            return (decimal)-date.Ticks;
        }

        private static decimal GetAlphabetIndex(string name)
        {
            decimal k = 0;
            byte[] bytes = System.Text.Encoding.GetEncoding("GB2312").GetBytes(name);
            for (int i = 0; i < 12; i++)
            {
                k *= 256;
                if (i < bytes.Length)
                    k += bytes[i];
            }
            return k;
        }

        public static bool isImage(object path)
        {
            string str = MyConvert.ToString(path).ToLower();
            // if (!str.Contains(".")) return true;
            if (str.EndsWith(".bmp")) return true;
            if (str.EndsWith(".jpg")) return true;
            if (str.EndsWith(".jpeg")) return true;
            if (str.EndsWith(".png")) return true;
            if (str.EndsWith(".gif")) return true;
            if (str.EndsWith(".tiff")) return true;
            if (str.EndsWith(".tif")) return true;
            return false;
        }

        public static bool isStaticImage(object path)
        {
            string str = MyConvert.ToString(path).ToLower();
            if (str.EndsWith(".bmp")) return true;
            if (str.EndsWith(".jpg")) return true;
            if (str.EndsWith(".jpeg")) return true;
            if (str.EndsWith(".png")) return true;
            if (str.EndsWith(".tiff")) return true;
            if (str.EndsWith(".tif")) return true;
            return false;
        }

        public static bool isVideo(object path)
        {
            string str = MyConvert.ToString(path).ToLower();
            if (str.EndsWith(".mp4")) return true;
            if (str.EndsWith(".mov")) return true;
            if (str.EndsWith(".mpg")) return true;
            if (str.EndsWith(".mpeg")) return true;
            if (str.EndsWith(".avi")) return true;
            if (str.EndsWith(".flv")) return true;
            return false;
        }


        private static SortedList<string, string> Json(string info)
        {
            SortedList<string, string> list = new SortedList<string, string>();
            string[] lines = info.Trim('{', '}').Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                int pos = line.IndexOf(':');
                if (pos >= 0)
                {
                    string key = line.Substring(0, pos).Trim('\"');
                    string value = line.Substring(pos + 1).Trim('\"');
                    list[key] = value;
                }
            }
            return list;
        }

        //public static Json Json(this Page page, string url)
        //{
        //    System.Net.WebClient wc = new System.Net.WebClient();
        //    SortedList<string, string> values = Json(wc.DownloadString(url));
        //    wc.Dispose();
        //    return new Json(values);
        //}

        public static Json Json(this Page page, string remotePage, params string[] parameters)
        {
            string url = remotePage;
            for (int i = 0; i < parameters.Length; i += 2)
            {
                if (i == 0) url = url + '?'; else url += '&';
                url += parameters[i] + '=' + page.Server.UrlEncode(parameters[i + 1]);
            }
            // page.Response.Write(url + "<br/>");
            System.Net.WebClient wc = new System.Net.WebClient();
            byte[] bytes = wc.DownloadData(url);
            string results = System.Text.Encoding.UTF8.GetString(bytes);
            // page.Response.Write(results + "<br/>");
            SortedList<string, string> values = Json(results);
            //foreach (KeyValuePair<string, string> p in values)
            //    page.Response.Write(p.Key + " = " + p.Value + "<br/>");
            wc.Dispose();
            return new Json(values);
        }

        //private static decimal GetChineseCharIndex(string name)
        //{
        //    List<byte> bl = new List<byte>();
        //    foreach (Char c in name)
        //    {
        //        int cc = Db.GetInt(ConnnectionType.News,
        //            string.Format(
        //            "IF (Select Count(*) From CharCount Where Char='{0}') > 0 " +
        //                "Select [Count] From CharCount Where Char='{0}' ELSE " +
        //                "INSERT INTO CharCount Values('{0}', 0)", 
        //            c.ToString().Replace("'", "''")));
        //        bl.Add((byte)cc);
        //    }

        //    decimal k = 0;
        //    byte[] bytes = bl.ToArray();
        //    for (int i = 0; i < 10; i++)
        //    {
        //        k *= 100;
        //        if (i < bytes.Length)
        //            k += bytes[i];
        //    }
        //    return k;
        //}

        //public static void AlterArticleSortIndex(int articleId)
        //{
        //    int pid = Db.GetInt(ConnnectionType.News, "Select SubDepID from Articles where id=" + articleId);
        //    string sortType = Db.GetString(ConnnectionType.News, "Select SortMode From Subdeps Where id=" + pid);

        //    decimal newIndex = 0;
        //    switch (sortType)
        //    {
        //        case "日期":
        //            newIndex = GetDateIndex(Db.GetDateTime(ConnnectionType.News, "Select PostTime From Articles Where id=" + articleId));
        //            break;
        //        case "字母":
        //            newIndex = GetAlphabetIndex(Db.GetString(ConnnectionType.News, "Select Title From Articles Where id=" + articleId));
        //            break;
        //        case "笔画":
        //            newIndex = GetChineseCharIndex(Db.GetString(ConnnectionType.News, "Select Title From Articles Where id=" + articleId));
        //            break;
        //        default:
        //            return;
        //    }

        //    Db.Execute(ConnnectionType.News,
        //        "UPDATE Articles Set [Index]=" + (-newIndex) + " Where id=" + articleId);
        //}

        //public static void AlterFolderSortIndex(int depid)
        //{
        //    int pid = Db.GetInt(ConnnectionType.News, "Select PID from SubDeps where id=" + depid);
        //    string sortType = Db.GetString(ConnnectionType.News, "Select SortMode From Subdeps Where id=" + pid);

        //    decimal newIndex = 0;
        //    switch (sortType)
        //    {
        //        case "字母":
        //            newIndex = GetAlphabetIndex(Db.GetString(ConnnectionType.News, "Select Name From SubDeps Where id=" + depid));
        //            break;
        //        case "笔画":
        //            newIndex = GetChineseCharIndex(Db.GetString(ConnnectionType.News, "Select Name From SubDeps Where id=" + depid));
        //            break;
        //        default:
        //            goto modifyNext;
        //    }

        //    Db.Execute(ConnnectionType.News,
        //        "UPDATE SubDeps Set [Index]=" + newIndex + " Where id=" + depid);

        //    modifyNext:
        //    using (DataTable t = Db.GetDataTable(ConnnectionType.News,
        //        "Select id from articles where subdepid=" + depid))
        //    {

        //        foreach (DataRow row in t.Rows)
        //        {
        //            int aid = MyConvert.ToInt(row["id"]);
        //            AlterArticleSortIndex(aid);
        //        }
        //    }

        //    using (DataTable t = Db.GetDataTable(ConnnectionType.News,
        //         "Select id from SubDeps where pid=" + depid))
        //    {
        //        foreach (DataRow row in t.Rows)
        //        {
        //            int aid = MyConvert.ToInt(row["id"]);
        //            AlterFolderSortIndex(aid);
        //        }
        //    }
        //}
    }
}