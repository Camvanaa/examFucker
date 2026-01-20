using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;

namespace Eiven.EXE.Web.Models
{
    public class Json
    {
        SortedList<string, string> list;
        public Json(SortedList<string, string> l)
        {
            list = l;
        }

        public string this[string key]
        {
            get
            {
                string v = "";
                if (list.TryGetValue(key, out v))
                    return v;
                else return "";
            }
        }
    }
}