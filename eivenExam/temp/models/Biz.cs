using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Eiven.EXE.Web.Models
{
	public static class Biz
	{
		public const string wxAppID = "wx277cf22cbb16fbe4";
		public const string wxSecret = "35b85148a7ae9889b46a3f7ef9962c2e";

		public enum SESSIONKEY
		{
			__EXE_CE_USER_NAME,
			__EXE_CE_USER_TRUENAME,
			__EXE_CE_USER_CODE,
			__EXE_CE_USER_LOGIN,
			__EXE_CE_USER_ID,
			__EXE_CE_USER_TYPE,
			__EXE_CE_USER_CLASSCODE,
			__EXE_CE_USER_TITLE,
		}

		public static bool FromWXBrowser(this Page page)
		{
			return page.Request.UserAgent.ToLower().Contains("micromessenger");
		}

		public static WXInfo GetWXInfo(this Page page)
		{
			string p = page.Request["p"];


			DataRow row = Db.GetDataRow("SELECT fxwxPlatforms.wxPlatform as wxPlatform, wxAppID, wxSecret FROM fxwxPlatforms, fxProjects WHERE fxwxPlatforms.wxPlatform=fxProjects.WXPlatform AND fxProjects.Code=" + Db.GetSQLParam(p));
			if (row == null) return WXInfo.Empty;
			WXInfo wi = new WXInfo();
			wi.wxPlatform = MyConvert.ToString(row["wxPlatform"]);
			wi.wxAppID = MyConvert.ToString(row["wxAppID"]);
			wi.wxSecret = MyConvert.ToString(row["wxSecret"]);


			return wi;
		}

		public struct WXInfo
		{
			public string wxAppID;
			public string wxSecret;
			public string wxPlatform;

			public static WXInfo Empty;
		}

		public static string GetSchool(this Page page)
		{
			string pk = page.Request["p"];
			string sc = Db.GetString("SELECT SchoolCode from fxProjects WHERE Code=" + Db.GetSQLParam(pk));
			return Db.GetString("SELECT Name from fxSchool WHERE Code=" + Db.GetSQLParam(sc));
		}

		public static string GetProject(this Page page)
		{
			string pk = page.Request["p"];
			return Db.GetString("SELECT Name from fxProjects WHERE Code=" + Db.GetSQLParam(pk));
		}

		public static bool CheckIDNumber(string idn, out DateTime birthDate, out bool sex, out string errReason)
		{
			idn = idn.ToUpper();
			birthDate = new DateTime(1900, 1, 1);

			sex = false;

			if (idn.Length != 18)
			{
				errReason = "身份证号码不是18位";
				return false;
			}
			int[] times = { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 };
			char[] check = { '1', '0', 'X', '9', '8', '7', '6', '5', '4', '3', '2' };
			int sum = 0;
			for (int i = 0; i < 17; ++i)
			{
				char c = idn[i];
				if (c < '0') { errReason = "身份证号码中出现了不该出现的字母或符号"; return false; }
				if (c > '9') { errReason = "身份证号码中出现了不该出现的字母或符号"; return false; }
				int n = (int)(c - '0');
				sum += n * times[i];
			}
			sum %= 11;

			if ((idn[17] < '0' || idn[17] > '9') && idn[17] != 'X')
			{ errReason = "身份证号码中出现了不该出现的字母或符号"; return false; }

			string y = idn.Substring(6, 4);
			string m = idn.Substring(10, 2);
			string d = idn.Substring(12, 2);

			sex = ((idn[16] - '0') % 2 == 1) ? false : true;

			if (!DateTime.TryParse(y + "-" + m + "-" + d, out birthDate))
			{
				errReason = "身份证号码中的出生日期有误";
				return false;
			}

			if (check[sum] != idn[17])
			{
				errReason = "身份证号码输入有误，请检查每一个数字或字母";
				return false;
			}

			errReason = "";
			return true;
		}

		public static string GetTitle(this Page page)
		{
			string title = GetProject(page) + " - " + GetSchool(page);
			return title.Replace("<br/>", "");
		}



		public enum UserType
		{
			STU,
			TEA,
			GUE,
		}

		public static void SetUserName(this Page page, string value)
		{
			page.Session[SESSIONKEY.__EXE_CE_USER_NAME.ToString()] = value;
		}

		public static string GetUserName(this Page page)
		{
			return Convert.ToString(page.Session[SESSIONKEY.__EXE_CE_USER_NAME.ToString()]);
		}

		public static void SetUserTrueName(this Page page, string value)
		{
			page.Session[SESSIONKEY.__EXE_CE_USER_TRUENAME.ToString()] = value;
		}

		public static string GetUserTrueName(this Page page)
		{
			return Convert.ToString(page.Session[SESSIONKEY.__EXE_CE_USER_TRUENAME.ToString()]);
		}

		public static string GetUserTitle(this Page page)
		{
			return Convert.ToString(page.Session[SESSIONKEY.__EXE_CE_USER_TITLE.ToString()]);
		}

		public static void SetUserTitle(this Page page, string title)
		{
			page.Session[SESSIONKEY.__EXE_CE_USER_TITLE.ToString()] = title;
		}

		public static void SetUserCode(this Page page, string value)
		{
			page.Session[SESSIONKEY.__EXE_CE_USER_CODE.ToString()] = value;
		}

		public static string GetUserHeadImageUrl(this Page page)
		{
			return Db.GetString("SELECT wxHeadImgUrl FROM Users Where Code=" + Db.GetSQLParam(Biz.GetUserCode(page))).Replace("\\/", "/");
		}

		public static void SetUserHeadImageUrl(this Page page, string url)
		{
			Db.Execute("UPDATE Users SET wxHeadImgUrl=" + Db.GetSQLParam(url) + " Where Code=" + Db.GetSQLParam(Biz.GetUserCode(page)));
		}

		public static string GetUserCode(this Page page)
		{
			return Convert.ToString(page.Session[SESSIONKEY.__EXE_CE_USER_CODE.ToString()]);
		}


		public static void SetUserClassCode(this Page page, string value)
		{
			page.Session[SESSIONKEY.__EXE_CE_USER_CLASSCODE.ToString()] = value;
		}

		public static string GetUserClassCode(this Page page)
		{
			return Convert.ToString(page.Session[SESSIONKEY.__EXE_CE_USER_CLASSCODE.ToString()]);
		}

		public static void SetUserLogin(this Page page, bool value)
		{
			page.Session[SESSIONKEY.__EXE_CE_USER_LOGIN.ToString()] = value;
		}

		public static bool GetUserLogin(this Page page)
		{
			return Convert.ToBool(page.Session[SESSIONKEY.__EXE_CE_USER_LOGIN.ToString()]);
		}


		public static void SetUserID(this Page page, int value)
		{
			page.Session[SESSIONKEY.__EXE_CE_USER_ID.ToString()] = value;
		}

		public static int GetUserID(this Page page)
		{
			return Convert.ToInt(page.Session[SESSIONKEY.__EXE_CE_USER_ID.ToString()]);
		}


		public static void SetUserType(this Page page, UserType value)
		{
			page.Session[SESSIONKEY.__EXE_CE_USER_TYPE.ToString()] = value;
		}

		public static UserType GetUserType(this Page page)
		{
			return Convert.ToEnum<UserType>(page.Session[SESSIONKEY.__EXE_CE_USER_TYPE.ToString()]);
		}

		public static void UserLogin(this Page page, int userId)
		{
			DataRow row = Db.GetDataRow("SELECT Code, UserName, TrueName, UserType, Title, ClassCode FROM Users WHERE ID=" + userId);

			page.SetUserCode(Convert.ToString(row["Code"]));
			page.SetUserName(Convert.ToString(row["UserName"]));
			page.SetUserTrueName(Convert.ToString(row["TrueName"]));
			page.SetUserType(Convert.ToEnum<UserType>(row["UserType"]));
			page.SetUserClassCode(Convert.ToString(row["ClassCode"]));
			page.SetUserTitle(Convert.ToString(row["Title"]));
			page.SetUserLogin(true);
			page.SetUserID(userId);

			//Response.Cookies.Add(new HttpCookie("__NSDFX_GROUP", MyConvert.ToString(Session["__NSDFX_GROUP"])));
			//Response.Cookies.Add(new HttpCookie("__NSDFX_GNAME", MyConvert.ToString(Session["__NSDFX_GNAME"])));

			//Response.Cookies["__NSDFX_GROUP"].Expires = DateTime.Now.AddYears(1);
			//Response.Cookies["__NSDFX_GNAME"].Expires = DateTime.Now.AddYears(1);

		}

		public enum DefaultAction
		{
			Activity,
			FreshmanRegister,
			Sign,
			Evaluation,
			Questionnaire,
			Evaluate,
		}

		public static DefaultAction GetDefaultAction(this Page page)
		{
			string p = page.Request["p"];


			string s = Db.GetString("SELECT defaultAction FROM fxProjects WHERE fxProjects.Code=" + Db.GetSQLParam(p));

			return MyConvert.ToEnum<Biz.DefaultAction>(s, DefaultAction.Activity);
		}

		public static string GetDefaultPage(this Page page)
		{

			if (string.Compare(page.Request["p"], "STEM.museum", true) == 0)
			{
				string code = page.GetUserCode();
				string proj = page.Request["p"];
				Db.Execute("DELETE FROM fxProjectUsers Where ProjectCode=" + Db.GetSQLParam(proj) + " AND UserCode=" + Db.GetSQLParam(code));
				Db.Execute("INSERT INTO fxProjectUsers (ProjectCode, UserCode) Values (" + Db.GetSQLParam(proj) + ", " + Db.GetSQLParam(code) + ")");
				return "ActivitySelect_Item.aspx?p=" + page.Request["p"];
			}

			switch (GetDefaultAction(page))
			{
				case DefaultAction.FreshmanRegister:
					return "Freshman_Register.aspx?p=" + page.Request["p"];
				case DefaultAction.Evaluation:
					return "E_Classroom.aspx?p=" + page.Request["p"];
				default:
					if (GetUserType(page) == UserType.GUE)
						return "Best.aspx?p=" + page.Request["p"];
					else
						return "Project.aspx?p=" + page.Request["p"];
			}
		}

		public static void GoToDefaultPage(this Page page)
		{
			page.Response.Redirect(GetDefaultPage(page));
		}

		public static bool HasPermission(this Page page, string permission)
		{
			return Db.GetBool("SELECT count(*) FROM dbo.fxUserPermissions " +
				"Where Permission=" + Db.GetSQLParam(permission) + " AND " +
				"UserCode=" + Db.GetSQLParam(GetUserCode(page)) + " AND " +
				"ProjectCode=" + Db.GetSQLParam(page.Request["p"]));
		}


	}
}