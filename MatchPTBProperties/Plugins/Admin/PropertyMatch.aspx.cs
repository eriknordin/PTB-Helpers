using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net;
using EPiServer.DataAbstraction;
using PageTypeBuilder;
using PageTypeBuilder.Reflection;
using PageTypeBuilder.Discovery;
using PageTypeBuilder.Activation;
using System.Reflection;
using PageTypeBuilder.Synchronization;
using PageTypeBuilder.Abstractions;
using EPiServer.PlugIn;
using EPiServer;
namespace Antecknat.Web.CMS.Plugins.Admin
{
    [GuiPlugIn(Area=PlugInArea.AdminMenu, DisplayName="Match properties with PTB", RequiredAccess=EPiServer.Security.AccessLevel.Administer, Url="~/Plugins/Admin/PropertyMatch.aspx")]
    public partial class PropertyMatch : EPiServer.UI.SystemPageBase
    {

        #region Properties 
        private List<PageTypeDefinition> _pageTypes;
        private List<PageTypeDefinition> PageTypes
        {
            get
            {
                return _pageTypes ?? (_pageTypes = (new PageTypeDefinitionLocator()).GetPageTypeDefinitions());
            }
        }
        #endregion

        #region Init
        protected override void OnPreInit(EventArgs e)
        {            
            base.OnPreInit(e);
            if (!EPiServer.Security.PrincipalInfo.HasAdminAccess)
            {
                AccessDenied();
            }
            Page.MasterPageFile =EPiServer.Configuration.Settings.Instance.UIUrl + "MasterPages/EPiServerUI.Master";
        }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);            
            RepeaterPageTypes.ItemDataBound += new RepeaterItemEventHandler(RepeaterPageTypes_ItemDataBound);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ReBind();

        }
        #endregion

        

        #region Helpers
        private void ReBind()
        {
            PageTypeCollection pagetypes = PageType.List();
            RepeaterPageTypes.DataSource = pagetypes;
            RepeaterPageTypes.DataBind();
        }
        #endregion

        #region Events
        void RepeaterPageTypes_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                Repeater rep = e.Item.FindControl("RepeaterProperties") as Repeater;

                PageType pagetype = e.Item.DataItem as PageType;

                rep.ItemDataBound += new RepeaterItemEventHandler(rep_ItemDataBound);

                rep.DataSource = pagetype.Definitions;
                rep.DataBind();
            }
        }

        void rep_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            try
            {
                if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
                {
                    bool missmatch = false;
                    PageDefinition property = e.Item.DataItem as PageDefinition;
                    PageType pagetype = PageType.Load(property.PageTypeID);

                    PageTypePropertyDefinitionLocator locator = new PageTypePropertyDefinitionLocator();

                    var tempType = PageTypes.SingleOrDefault(p => p.GetPageTypeName() == pagetype.Name);
                    if (tempType == null)
                    {
                        Literal ltl = e.Item.FindControl("LiteralMissmatch") as Literal;
                        ltl.Text = "Could not find page type(?)";
                        return;
                    }

                    var props = locator.GetPageTypePropertyDefinitions(pagetype, tempType.Type);

                    var ptbproperty = props.SingleOrDefault(p => p.Name == property.Name);
                    if (ptbproperty == null)
                    {
                        LinkButton remove = e.Item.FindControl("LinkButtonRemove") as LinkButton;
                        remove.Text = "Remove property";
                        remove.CommandArgument = property.ID.ToString();
                        remove.Click += new EventHandler(remove_Click);
                        missmatch = true;
                        Literal ltl = e.Item.FindControl("LiteralMissmatch") as Literal;
                        ltl.Text = "Could not find PTB-property";
                    }
                    else
                    {
                        Literal l = e.Item.FindControl("LiteralType") as Literal;
                        try
                        {
                            Type t = ptbproperty.PageTypePropertyAttribute.Type ?? (new PageDefinitionTypeMapper(new PageDefinitionTypeFactory())).GetDefaultPropertyType(ptbproperty.PropertyType);

                            string typename = (t != null ? t.Name : "Could not get type");

                            l.Text = typename;

                            if (typename != property.Type.DefinitionType.Name)
                            {
                                Literal ltl = e.Item.FindControl("LiteralMissmatch") as Literal;
                                ltl.Text = "Type mismatch";
                                missmatch = true;

                                //http://local.sn.se/EPiUi/CMS/Admin/EditPageTypeField.aspx?typeId=116
                                HyperLink link = e.Item.FindControl("HyperLinkAction") as HyperLink;
                                link.Target = "_blank";
                                link.Text = "Edit property";
                                link.NavigateUrl = UriSupport.ResolveUrlFromUIBySettings("Admin/") + "EditPageTypeField.aspx?typeId=" + property.ID;
                            }
                        }
                        catch { }
                    }


                    if (CheckBoxOnlyMissmatch.Checked && !missmatch)
                    {
                        e.Item.Visible = false;
                    }



                }
            }
            catch { }
        }

        protected string GetEPiType(object o)
        {
            try
            {
                return ((EPiServer.DataAbstraction.PageDefinition)o).Type.DefinitionType.Name;
            }
            catch
            {
                return "Fel";
            }
        }

        void remove_Click(object sender, EventArgs e)
        {
            LinkButton btn = sender as LinkButton;
            int id = int.Parse(btn.CommandArgument);

            PageDefinition pd = PageDefinition.Load(id);

            pd.Delete();
            ReBind();
        }
        #endregion
    }
}
