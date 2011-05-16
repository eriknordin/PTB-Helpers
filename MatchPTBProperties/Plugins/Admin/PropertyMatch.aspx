<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="PropertyMatch.aspx.cs" Inherits="Antecknat.Web.CMS.Plugins.Admin.PropertyMatch" %>


<asp:Content ID="Content1" ContentPlaceHolderID="HeaderContentRegion" runat="server">
</asp:Content>
<asp:Content ID="Content3" ContentPlaceHolderID="MainRegion" runat="server">
<style>
    .override td { padding: 0 5px; }
</style>
    <p>Show only missmatchs: <asp:CheckBox id="CheckBoxOnlyMissmatch" autopostback="true" checked="true" runat="server"></asp:CheckBox></p>
    <table class="override">
        <asp:Repeater ID="RepeaterPageTypes" runat="server">
            <ItemTemplate>
            <thead>
                <tr><td colspan="5"><b>PageType: <a href="<%= EPiServer.UriSupport.ResolveUrlFromUIBySettings("Admin")  %>/EditPageType.aspx?typeId=<%# DataBinder.Eval(Container.DataItem, "ID") %>" target="_blank"><%# DataBinder.Eval(Container.DataItem, "Name") %></a></td></tr>
                
                    
                        <tr>
                            <td>Propertyname</td>
                            <td>EPi-type</td>
                            <td>PTB-type</td>
                            <td>Message</td>
                            <td>Action</td>
                        </tr>
                    </thead>
                    <tbody>
                    
                    <asp:Repeater ID="RepeaterProperties" runat="server">
                        <ItemTemplate>                            
                            <tr>
                                <td><%# DataBinder.Eval(Container.DataItem, "Name") %></td>
                                <td><%# GetEPiType(Container.DataItem) %></td>
                                <td><asp:Literal ID="LiteralType" runat="server"></asp:Literal></td>
                                <td><asp:Literal ID="LiteralMissmatch" runat="server"></asp:Literal></td>
                                <td align="center"><asp:LinkButton ID="LinkButtonRemove" runat="server"></asp:LinkButton><asp:HyperLink id="HyperLinkAction" runat="server" /></td>
                            </tr>
                        </ItemTemplate>
                    </asp:Repeater>       
                    <tr><td>&nbsp;</td></tr>     
                </tbody>
            
            </ItemTemplate>
        </asp:Repeater>
        </table>
</asp:Content>
