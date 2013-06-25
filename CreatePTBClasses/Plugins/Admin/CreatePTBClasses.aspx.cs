using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using EPiServer;
using EPiServer.DataAbstraction;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections;

namespace Antecknat.Web.CMS.Plugins.Admin
{
    [GuiPlugIn(Area = PlugInArea.AdminMenu, DisplayName = "Create PTB classes from page types", RequiredAccess = EPiServer.Security.AccessLevel.Administer, Url = "~/Plugins/Admin/CreatePTBClasses.aspx")]
    public partial class CreatePTBClasses : System.Web.UI.Page
    {
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            ButtonSubmit.Click += new EventHandler(ButtonSubmit_Click);
        }
     
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            if (!IsPostBack)
            {
                CheckBoxListPageTypes.DataSource = PageType.List();
                CheckBoxListPageTypes.DataTextField = "Name";
                CheckBoxListPageTypes.DataValueField = "ID";
                CheckBoxListPageTypes.DataBind();
            }

        }
        
        void ButtonSubmit_Click(object sender, EventArgs e)
        {
            StringBuilder ret = new StringBuilder();
            Page.Validate();
            if (Page.IsValid)
            {
                PlaceHolderInput.Visible = false;
                CreatePageTypeFiles(ret);
                CreateTabs(ret);                

                LiterlOutput.Text = ret.ToString();
            }
        }

        StringBuilder CreatePageTypeFiles(StringBuilder ret)
        {
            foreach (ListItem li in CheckBoxListPageTypes.Items)
            {
                if (!li.Selected)
                {
                    PageType pageType = PageType.Load(int.Parse(li.Value));
                    string dir = OutputDirectory(TextBoxOutput.Text, pageType.Name);
                    string className = GetClassName(pageType.Name);
                    string classCont = GetClassContainer(pageType.Name);
                    ret.AppendFormat("Name: {0}, ClassName: {1}, Directory: {2}<br />", pageType.Name, GetClassName(pageType.Name), dir);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(OutputDirectory(TextBoxOutput.Text, pageType.Name));
                    }
                    using (StreamWriter sw = File.CreateText(dir + "\\" + className + ".cs"))
                    {
                        sw.WriteLine(TextBoxUsing.Text);
                        sw.WriteLine(string.Format(@"using {12}.Tabs;

namespace {0}
{{
    [PageType(""{1}"",
        Name = ""{2}"",
        Filename = ""{3}"",
        DefaultChildSortOrder = EPiServer.Filters.FilterSortOrder.{4},
        Description = ""{5}"",
        SortOrder = {6},
        DefaultVisibleInMenu = {7},        
        AvailablePageTypes = new Type[] {{ {11} }})]
    
    public class {8} : {9}
    {{
        {10}
    }}
}}
", TextBoxNamespace.Text + (string.IsNullOrEmpty(classCont) ? string.Empty : "." + classCont),
pageType.GUID,
pageType.Name,
pageType.FileName,
pageType.DefaultChildOrderRule.ToString(),
pageType.Description.Replace("\"", "'"),
pageType.SortOrder,
pageType.DefaultVisibleInMenu.ToString().ToLower(),
className,
TextBoxExtend.Text,
GetProperties(pageType, ret),
GetAvailablePageTypes(pageType),
TextBoxNamespace.Text
));
                        sw.Flush();
                    }
                }
                else
                {
                    ret.AppendFormat("Ignoring {0}<br />", li.Text);
                }
            }
            return ret;
        }

        private void CreateTabs(StringBuilder ret)
        {
            string dir = OutputDirectory(TextBoxOutput.Text, string.Empty);
            dir += "Tabs";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            foreach (TabDefinition tab in TabDefinition.List())
            {
                using (StreamWriter sw = File.CreateText(dir + "/" + ReplaceIllegalChars(tab.Name) + "Tab.cs"))
                {
                    sw.WriteLine(string.Format(@"
using PageTypeBuilder;

namespace {0}.Tabs
{{
    public class {1}Tab : Tab
    {{
        public override string Name
        {{
            get {{ return ""{2}""; }}
        }}

        public override EPiServer.Security.AccessLevel RequiredAccess
        {{
            get {{ return EPiServer.Security.AccessLevel.{3}; }}
        }}

        public override int SortIndex
        {{
            get {{ return {4}; }}
        }}
    }}
}}
", TextBoxNamespace.Text, ReplaceIllegalChars(tab.Name), tab.Name, tab.RequiredAccess.ToString(), tab.SortIndex ));

                    sw.Flush();
                }
            }
        }

        private object GetProperties(PageType pageType, StringBuilder output)
        {
            StringBuilder ret = new StringBuilder();
            foreach (PageDefinition pdf in pageType.Definitions)
            {
                if (pdf.Name.Contains("-"))
                {
                    output.AppendFormat(" Ignoring property: " + pdf.Name);
                }
                else
                {
                    ret.AppendFormat(@"
        [PageTypeProperty(EditCaption = ""{0}"",
            HelpText = ""{1}"",
            SortOrder = {2},
            UniqueValuePerLanguage = {3},
            Required = {4},
            Searchable = {9},
            DefaultValue = ""{10}"",
            DefaultValueType = EPiServer.DataAbstraction.DefaultValueType.{12},
            Tab = typeof({5}){6}{11})]
        public virtual {7} {8} {{ get; set; }}

", pdf.EditCaption.Replace("\"", "'"),
          pdf.HelpText.Replace("\"", "'"),
          pdf.FieldOrder,
          pdf.LanguageSpecific.ToString().ToLower(),
          pdf.Required.ToString().ToLower(),
          ReplaceIllegalChars(pdf.Tab.Name) + "Tab",
          string.IsNullOrEmpty(pdf.Type.TypeName) ? "" : string.Format(@",
            Type = typeof({0})", pdf.Type.TypeName),
          GetDataType(pdf.Type.DataType),
          pdf.Name,
          pdf.Searchable.ToString().ToLower(),
          pdf.DefaultValue,
          string.Empty,
          pdf.DefaultValueType.ToString());
                }
            }

            return ret.ToString();
        }

      

        private string GetDataType(EPiServer.Core.PropertyDataType propertyDataType)
        {
            switch (propertyDataType)
            {
                case EPiServer.Core.PropertyDataType.Boolean:
                    return "bool";
                case EPiServer.Core.PropertyDataType.Category:
                    return "CategoryCollection";
                case EPiServer.Core.PropertyDataType.Date:
                    return "DateTime";
                case EPiServer.Core.PropertyDataType.FloatNumber:
                    return "double";
                case EPiServer.Core.PropertyDataType.LinkCollection:
                    return "LinkItemCollection";
                case EPiServer.Core.PropertyDataType.LongString:
                    return "string";
                case EPiServer.Core.PropertyDataType.Number:
                    return "int";
                case EPiServer.Core.PropertyDataType.PageReference:
                    return "PageReference";
                case EPiServer.Core.PropertyDataType.PageType:
                    return "int";
                case EPiServer.Core.PropertyDataType.String:
                    return "string";
                default:
                    return "object";
            }
        }

        string OutputDirectory(string output, string name)
        {
            return Server.MapPath("/" + TextBoxOutput.Text.Trim() + "/" + GetClassContainer(name));
        }

        string GetClassContainer(string name)
        {
            var pattern = @"\[(.*?)\]";
            var match = Regex.Match(name, pattern);
            return match.Value.TrimStart('[').TrimEnd(']');
        }
        
        string GetClassName(string name)
        {
            var pattern = @"\[(.*?)\]";
            name = Regex.Replace(name, pattern, "");
            name = ReplaceIllegalChars(name);
            return TextBoxPrefixBefore.Text.Trim() + name + TextBoxPrefixAfter.Text.Trim();
        }

        string ReplaceIllegalChars(string inputString)
        {            
            Regex regexFindInvalidUrlChars = new Regex(@"[^A-Za-z0-9\-_~]{1}", RegexOptions.Compiled);
            Hashtable _urlCharacterMap = EPiServer.Web.UrlSegment.GetURLCharacterMap();

            StringBuilder builder = new StringBuilder(inputString);
            MatchCollection matchs = regexFindInvalidUrlChars.Matches(inputString);
            for (int i = 0; i < matchs.Count; i++)
            {
                object obj2 = _urlCharacterMap[builder[matchs[i].Index]];
                if (obj2 != null)
                {
                    builder[matchs[i].Index] = (char)obj2;
                }
                else
                {
                    builder[matchs[i].Index] = '?';
                }
            }
            builder.Replace("?", "");
            builder.Replace(" ", "");
            builder.Replace("-", "");
            return builder.ToString();
        }

        private string GetAvailablePageTypes(PageType pageType)
        {
            StringBuilder ret = new StringBuilder();
            foreach (string s in pageType.AllowedPageTypeNames)
            {
                string cont = GetClassContainer(s);
                string typeName = string.IsNullOrEmpty(cont) ? GetClassName(s) : cont + "." + GetClassName(s);
                ret.AppendFormat(@" typeof({0}), ", typeName);
            }
            if (ret.Length > 0) {
                return ret.ToString().Trim().TrimEnd(',');
            }
            return ret.ToString();
        }
        
    }
}
