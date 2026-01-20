<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Screen.aspx.cs" Inherits="Eiven.EXE.Web.Models.Screen" %>

<html>
<head runat="server">
    <meta http-equiv="Content-Type" content="text/html; charset=utf-8" />
    <title></title>
</head>
<body style="margin: 0 0 0 0; overflow: hidden;">
    <table style="width: 100%; height: 100%;background-color:dimgray; border-spacing:0 0; padding:0 0 0 0"
        border="0" cellspacing="0" cellpadding="0">
        <tr>
            <td style="width:25%;height:25%"><img id="s00" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s01" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s02" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s03" src="" style="width:100%;height:100%;" /></td>
        </tr>
        <tr>
            <td style="width:25%;height:25%"><img id="s10" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s11" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s12" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s13" src="" style="width:100%;height:100%;" /></td>
        </tr>
        <tr>
            <td style="width:25%;height:25%"><img id="s20" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s21" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s22" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s23" src="" style="width:100%;height:100%;" /></td>
        </tr>
        <tr>
            <td style="width:25%;height:25%"><img id="s30" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s31" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s32" src="" style="width:100%;height:100%;" /></td>
            <td style="width:25%;height:25%"><img id="s33" src="" style="width:100%;height:100%;" /></td>
        </tr>
    </table>
    <img src="cursor.png" id="cur" style="position:absolute;left:-100px; top:-100px" />
    <script>

        function setImg(id, path, lpath)
        {
            document.getElementById(id).style.backgroundImage = "url('" + lpath + "')";
            document.getElementById(id).src = path;
        }

        function setCur(x, y)
        {
            cur.style.left = window.innerWidth * x + "px";
            cur.style.top = window.innerHeight * y + "px";
        }

        function sendKeyDown(e)
        {
            var event = e || window.event; // e:非IE浏览器使用，window.event是IE浏览器使用
           // console.log(event.shiftKey, event.altKey, event.ctrlKey, event.key, event.keyCode);
            alert(event.key);
        }

        function sendKeyUp(e) {
            var event = e || window.event; // e:非IE浏览器使用，window.event是IE浏览器使用
            // console.log(event.shiftKey, event.altKey, event.ctrlKey, event.key, event.keyCode);
            alert(event.key);
        }

        document.addEventListener('keydown', sendKeyDown, false)
        document.addEventListener('keyup', sendKeyUp, false)


    </script>
    <iframe style="display:none" src="Screen_p.aspx?c=<% =Request["c"] %>">

    </iframe>
</body>
</html>
