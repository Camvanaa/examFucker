using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Eiven.EXE.Web.Models
{
    public enum HomeMenu
    {
        Home,
        Teacher,
        Student,
        Curcurriculum,
        ContactUs
    }

    public class HomeMenuItem
    {
        public HomeMenu Key { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
    }

    public class HomeMenuAdapter
    {
        List<HomeMenuItem> items = new List<HomeMenuItem>();

        System.Web.UI.WebControls.Table table;

        public HomeMenuAdapter(System.Web.UI.WebControls.Table table)
        {
            Init();

            table.PreRender += Table_PreRender;
            this.table = table;
        }

        private void Table_PreRender(object sender, EventArgs e)
        {
            DrawMenu();
        }

        private void DrawMenu()
        {
            if (table == null) return;
            table.Rows.Clear();
            table.Rows.Add(new TableRow());

            TableRow row = table.Rows[0];
            foreach (HomeMenuItem item in items)
            {
                TableCell tc = new TableCell();
                tc.Width = new Unit(100.0 / items.Count, UnitType.Percentage);
                row.Cells.Add(tc);

                HyperLink link = new HyperLink();
                tc.Controls.Add(link);
                tc.HorizontalAlign = HorizontalAlign.Center;

                link.Text = item.Title;
                link.NavigateUrl = item.Url;
                link.Font.Name = "微软雅黑";
                link.Font.Size = new FontUnit(14, UnitType.Pixel);
                link.Font.Bold = true;
                link.ForeColor = System.Drawing.Color.FromArgb(100, 100, 100);
                link.Font.Underline = false;
                
            }
        }

        private void Init()
        {
            items.Add(new HomeMenuItem() { Key = HomeMenu.Home, Title = "首页", Url = "~/Default.aspx" });
            items.Add(new HomeMenuItem() { Key = HomeMenu.Student, Title = "学生", Url = "~/Students.aspx" });
            items.Add(new HomeMenuItem() { Key = HomeMenu.Teacher, Title = "教师", Url = "~/Teachers.aspx" });
            items.Add(new HomeMenuItem() { Key = HomeMenu.Curcurriculum, Title = "STEM", Url = "~/STEM.aspx" });
            items.Add(new HomeMenuItem() { Key = HomeMenu.ContactUs, Title = "登录", Url = "~/Login.aspx" });

            
        }


    }
}