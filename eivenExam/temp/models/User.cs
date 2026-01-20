using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

/// <summary>
/// Summary description for User
/// </summary>
namespace Eiven.EXE.Web.Models
{
    public sealed class WebUser
    {
        public WebUser()
        {
        }

        public string Name { get; set; }
        public string TrueName { get; set; }
        public string Email { get; set; }
        public bool Login { get; set; }
    }

    public enum PermissionType
    {
        CreateFolder,
        ModifyFolder,
        DeleteFolder,
        CreateDocument,
        ModifyDocument,
        DeleteDocument,
    }

}