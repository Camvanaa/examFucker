using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Eiven.EXE.Web.Models
{
    public partial class Screen_p : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string s = "<script>\n";
            string path = Server.MapPath("RD") + "\\" + Request["c"] + "\\";
            for (int y = 0; y < 4; ++y)
                for (int x = 0; x < 4; ++x)
                {
                    string folder = path + y + "." + x + "\\";
                    if (Directory.Exists(folder))
                    {
                        string[] files = Directory.GetFiles(folder);
                        Array.Sort(files);

                        string p1 = null, p2 = null;
                        if (files.Length > 1)
                            p1 = p2 = files[files.Length - 1];
                        if (files.Length > 2)
                            p2 = files[files.Length - 2];

                        if (p1 != null)
                        {
                            p1 = "RD\\" + Request["c"] + "\\" + y + "." + x + "\\" +
                                Path.GetFileName(p1);
                            p2 = "RD\\" + Request["c"] + "\\" + y + "." + x + "\\" +
                                Path.GetFileName(p2);

                            s += "parent.setImg(\"s" + y + x + "\", \"" + p1.Replace("\\", "/") + "\", \"" + p2.Replace("\\", "/") + "\");\n";

                        }
                    }

                }

            object objf = Application[Request["c"] + "_Mouse"];
            if (objf != null && objf is PointF)
            {
                PointF p = (PointF)objf;
                s += "parent.setCur(" + p.X + ", " + p.Y + ");\n"; 
            }
            s += "</script>";
            sRun.InnerHtml = s;
        }
    }
}