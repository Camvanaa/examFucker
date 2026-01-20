using System;
using System.Collections.Generic;
using System.Web;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using System.Xml;

/// <summary>
/// Summary description for Db
/// </summary>
/// 
namespace Eiven.EXE.Web.Models
{
    public static class Db
    {
        enum SqlServerVersion
        {
            _2000 = 2000,
            _2005 = 2005,
            _2008 = 2008,
            _2012 = 2012,
        }

        static SqlServerVersion version = SqlServerVersion._2000;

        public static string SysColumnsTableName
        {
            get
            {
                switch (version)
                {
                    case SqlServerVersion._2000:
                        return "sysColumns";
                    default:
                        return "sys.Columns";
                }
            }
        }
        public static string SysObjectsTableName
        {
            get
            {
                switch (version)
                {
                    case SqlServerVersion._2000:
                        return "sysObjects";
                    default:
                        return "sys.Objects";
                }
            }
        }
        public static string SysObjectIDFieldName
        {
            get
            {
                switch (version)
                {
                    case SqlServerVersion._2000:
                        return "id";
                    default:
                        return "object_id";
                }
            }
        }
        public static string SysColumnTypeFieldName
        {
            get
            {
                switch (version)
                {
                    case SqlServerVersion._2000:
                        return "xtype";
                    default:
                        return "system_type_id";
                }
            }
        }



        public static string GetConnectionKeyName(ConnnectionType type)
        {
            return type.ToString();
        }

        public static SqlConnection GetConnection()
        {
            string key = GetConnectionKeyName(ConnnectionType.EXE);
            string connStr = ConfigurationManager.ConnectionStrings[key].ConnectionString;

            SqlConnection conn = new SqlConnection(connStr);
            conn.Open();
            return conn;
        }

        public static object GetValue(string sql)
        {
            SqlConnection conn = GetConnection();
            SqlCommand cmd = new SqlCommand(sql, conn);
            object obj = cmd.ExecuteScalar();
            cmd.Dispose();
            conn.Close();
            conn.Dispose();
            return obj;
        }

        public static int Execute(string sql)
        {
            SqlConnection conn = GetConnection();
            SqlCommand cmd = new SqlCommand(sql, conn);
            int obj = cmd.ExecuteNonQuery();
            cmd.Dispose();
            conn.Close();
            conn.Dispose();
            return obj;
        }

        public static DataRow GetDataRow(string sql)
        {
            SqlConnection conn = GetConnection();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(ds);
            adapter.Dispose();
            conn.Dispose();
            if (ds.Tables.Count <= 0) return null;
            if (ds.Tables[0].Rows.Count <= 0) return null;
            return ds.Tables[0].Rows[0];
        }

        public static DataTable GetDataTable(string sql)
        {
            SqlConnection conn = GetConnection();
            DataSet ds = new DataSet();
            SqlDataAdapter adapter = new SqlDataAdapter(sql, conn);
            adapter.Fill(ds);
            adapter.Dispose();
            conn.Dispose();
            if (ds.Tables.Count <= 0) return null;
            return ds.Tables[0];
        }

        public static SortedList<T1, T2> GetList<T1, T2>(string sql)
        {
            SortedList<T1, T2> objects = new SortedList<T1, T2>();
            foreach (DataRow row in GetDataTable(sql).Rows)
            {
                try
                {
                    T1 v1 = (T1)System.Convert.ChangeType(row[0], typeof(T1));
                    T2 v2 = (T2)System.Convert.ChangeType(row[1], typeof(T2));
                    objects[v1] = v2;
                }
                catch (System.Exception) { }
            }
            return objects;
        }

        public static List<T> GetList<T>(string sql)
        {
            List<T> objects = new List<T>();
            foreach (DataRow row in GetDataTable(sql).Rows)
            {
                try
                {
                    T value = (T)System.Convert.ChangeType(row[0], typeof(T));
                    objects.Add(value);
                }
                catch (System.Exception) { }
            }
            return objects;
        }

        public static List<object> GetList(string sql)
        {
            List<object> objects = new List<object>();
            foreach (DataRow row in GetDataTable(sql).Rows)
            {
                objects.Add(row[0]);
            }
            return objects;
        }


        public static List<string> GetStringList(string sql)
        {
            List<string> objects = new List<string>();
            foreach (DataRow row in GetDataTable(sql).Rows)
            {
                objects.Add(MyConvert.ToString(row[0]));
            }
            return objects;
        }

        public static List<int> GetIntList(string sql)
        {
            List<int> objects = new List<int>();
            foreach (DataRow row in GetDataTable(sql).Rows)
            {
                objects.Add(MyConvert.ToInt(row[0]));
            }
            return objects;
        }

        public static string GetString(string sql)
        {
            return MyConvert.ToString(GetValue(sql));
        }

        //public static DateTime GetDateTime(string sql)
        //{
        //    return MyConvert.ToDateTime(sql);
        //}

        public static int GetInt(string sql)
        {
            return MyConvert.ToInt(GetValue(sql));
        }

        public static double GetDouble(string sql)
        {
            return MyConvert.ToDouble(GetValue(sql));
        }

        public static bool GetBool(string sql)
        {
            return MyConvert.ToBool(GetValue(sql));
        }

        public static long GetLong(string sql)
        {
            return MyConvert.ToLong(GetValue(sql));
        }

        public static DateTime GetDateTime(string sql)
        {
            return MyConvert.ToDateTime(GetValue(sql));
        }

        public static string GetSQLParam(object p, Type dataType = null)
        {
            SqlDataType t = SqlDataType.Auto;
            if (p != null && dataType == null) dataType = p.GetType();
            if (dataType == typeof(string)) t = SqlDataType.String;
            else if (dataType == typeof(int)) t = SqlDataType.Integer;
            else if (dataType == typeof(uint)) t = SqlDataType.Integer;
            else if (dataType == typeof(short)) t = SqlDataType.Integer;
            else if (dataType == typeof(ushort)) t = SqlDataType.Integer;
            else if (dataType == typeof(long)) t = SqlDataType.Integer;
            else if (dataType == typeof(ulong)) t = SqlDataType.Integer;
            else if (dataType == typeof(byte)) t = SqlDataType.Integer;
            else if (dataType == typeof(sbyte)) t = SqlDataType.Integer;
            else if (dataType == typeof(float)) t = SqlDataType.Float;
            else if (dataType == typeof(double)) t = SqlDataType.Float;
            else if (dataType == typeof(DateTime)) t = SqlDataType.DateTime;
            else if (dataType == typeof(bool)) t = SqlDataType.Bool;
            else if (dataType == typeof(decimal)) t = SqlDataType.Float;

            return GetSQLParam(p, t);
        }

        public static string GetSQLParam(object parameter,
            SqlDataType dataType, string columnName = null)
        {
            if (dataType == SqlDataType.Auto && columnName != null)
            {
                if (columnName.EndsWith("Date") ||
                    columnName.EndsWith("Time") ||
                    columnName.EndsWith("时间") ||
                    columnName.EndsWith("日期"))
                    dataType = SqlDataType.DateTime;
                if (columnName.EndsWith("ID"))
                    dataType = SqlDataType.Integer;
            }

            switch (dataType)
            {
                case SqlDataType.Integer:
                    return MyConvert.ToLong(parameter).ToString();
                case SqlDataType.Bool:
                    return MyConvert.ToBool(parameter) ? "1" : "0";
                case SqlDataType.Float:
                    return MyConvert.ToDouble(parameter).ToString();
                case SqlDataType.DateTime:
                    return "'" + MyConvert.ToDateTime(parameter).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                default:
                    return "'" + MyConvert.ToString(parameter).Replace("'", "''") + "'";
            }
        }



    }

    public enum SqlDataType
    {
        String,
        Integer,
        Float,
        Auto,
        DateTime,
        Bool
    }

    public class FolderInfo
    {
        private int iD;

        public int ID
        {
            get { return iD; }
            set { iD = value; }
        }
        private string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
    }

    public enum ConnnectionType
    {
        News,
        Users,
        EXE,
    }
}