using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Data;

/// <summary>
/// Summary description for PageBase
/// </summary>

namespace Eiven.EXE.Web.Models
{
    public class PageBase : Page
    {
        public PageBase()
        {
            this.PreRenderComplete += new EventHandler(PageBase_PreRenderComplete);
            this.Init += new EventHandler(PageBase_Init);
            this.Load += new EventHandler(PageBase_Load);
        }

        protected bool checkLogin = true;

        void PageBase_Load(object sender, EventArgs e)
        {
            //if (checkLogin)
             //   NeedLogin();
        }

        void PageBase_Init(object sender, EventArgs e)
        {
            if (this.Header != null)
                pageTitle = this.Title;
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
        }

        public bool SupportPNG
        {
            get
            {
                if (Request.Browser.Browser == "IE" && Request.Browser.Version.Length >= 1 && Request.Browser.Version[0] < '7')
                    return false;
                else
                    return true;
            }
        }

        bool preRenderCompleted = false;
        void PageBase_PreRenderComplete(object sender, EventArgs e)
        {
            if (this.Header != null)
            {
                if (this.Header.Title == "" || this.Header.Title == null)
                    this.Header.Title = "特种设备管理系统";
                else
                    this.Header.Title += " - 特种设备管理系统";
            }
            preRenderCompleted = true;
        }

        string pageTitle;
        public string PageTitle
        {
            get
            {
                if (pageTitle == null && !preRenderCompleted) return this.Title;
                return pageTitle;
            }
            set
            {
                pageTitle = value;
                this.Title = this.Header.Title = value;
            }
        }

        public string IP
        {
            get
            {
                return Request.ServerVariables["REMOTE_ADDR"];
            }
        }

        public new WebUser User
        {
            get
            {
                WebUser u = (WebUser)Session[Common.SESSION_NAME_CURRENT_USER];
                return u;
            }
        }

        //public void NeedAdmin()
        //{
        //    if (!User.IsAdmin)
        //        Response.Redirect("~/Default.aspx");
        //}

        public void NeedLogin()
        {
            if (User == null || !User.Login)
                Response.Redirect("~/JumpToHome.aspx");
        }

        public int EvalI(string field)
        {
            return MyConvert.ToInt(Eval(field));
        }

        public bool EvalB(string field)
        {
            return MyConvert.ToBool(Eval(field));
        }

        public double EvalF(string field)
        {
            return MyConvert.ToDouble(Eval(field));
        }

        public string EvalS(string field)
        {
            return MyConvert.ToString(Eval(field));
        }

        public DateTime EvalD(string field)
        {
            return MyConvert.ToDateTime(Eval(field));
        }

        public void CloseDialogAndRefresh()
        {
            Response.Write("<script language='javascript'>");
            Response.Write("	parent.CloseDialogAndRefresh();");
            Response.Write("</script>");
            Response.End();
        }

        public void Alert(string p)
        {
            Response.Write("<script language='javascript'>");
            Response.Write("	alert('" + p + "');");
            Response.Write("</script>");
            Response.End();
        }
    }
}