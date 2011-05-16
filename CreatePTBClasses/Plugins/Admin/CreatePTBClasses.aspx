<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CreatePTBClasses.aspx.cs" Inherits="Antecknat.Web.CMS.Plugins.Admin.CreatePTBClasses" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
    <title></title>
    <style>
        body { font-family: Verdana; font-size: 11px; }
        h3 { padding: 0; margin: 10px 0 3px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <h1>Create PageTypeBuilder classes from a current project</h1>
        <asp:PlaceHolder ID="PlaceHolderInput" runat="server">
            <div>
                <h3>Prefix before class name:</h3></h3>
                <asp:TextBox ID="TextBoxPrefixBefore" runat="server"></asp:TextBox>
            </div>
            
            <div>
                <h3>Prefix after class name:</h3>
                <asp:TextBox ID="TextBoxPrefixAfter" runat="server">PageType</asp:TextBox>
            </div>
            
            <div>
                <h3>PageType base class:</h3>
                <asp:TextBox ID="TextBoxExtend" runat="server">TypedPageData</asp:TextBox>
            </div>
            
            <div>
                <h3>Namespace:</h3>
                <asp:TextBox ID="TextBoxNamespace" runat="server"></asp:TextBox>
                <asp:RequiredFieldValidator ID="ReqFielsNamespace" ControlToValidate="TextBoxNamespace" runat="server"></asp:RequiredFieldValidator>
            </div>
            
            <div>
                <h3>Output directory (relative to Server.MapPath):</h3>
                <asp:TextBox ID="TextBoxOutput" runat="server">PageTypes</asp:TextBox>
            </div>
            
            <div>
                <h3>Exclude following page types:</h3>
                <asp:CheckBoxList ID="CheckBoxListPageTypes" RepeatDirection="Horizontal" RepeatColumns="3" runat="server"></asp:CheckBoxList>
            </div>
            
            <div>
                <h3>Using:</h3>
                <asp:TextBox ID="TextBoxUsing" TextMode="MultiLine" Width="400" Height="200" runat="server">using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using PageTypeBuilder;
using EPiServer;
using EPiServer.Core;</asp:TextBox>
            </div>
            <div>
                <asp:Button ID="ButtonSubmit" Text="Create classes" runat="server" />
            </div>
        </asp:PlaceHolder>
        
        <asp:Literal ID="LiterlOutput" runat="server"></asp:Literal>
        
    </div>
    </form>
</body>
</html>
