using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Configuration
{
	public class DocmahConfigurationSection : ConfigurationSection, IDocmahConfiguration
	{
		#region Public Properties

		public static DocmahConfigurationSection Current
		{
			get
			{
				return (DocmahConfigurationSection)ConfigurationManager.GetSection("docmah");
			}
		}
		
		[ConfigurationProperty("connectionStringName", DefaultValue = null)]
		public string ConnectionStringName
		{
			get { return (string)this["connectionStringName"]; }
			set { this["connectionStringName"] = value; }
		}

		[ConfigurationProperty("jsUrl", DefaultValue = null)]
		public string JsUrl
		{
			get { return (string)this["jsUrl"]; }
			set { this["jsUrl"] = value; }
		}

		[ConfigurationProperty("cssUrl", DefaultValue = null)]
		public string CssUrl
		{
			get { return (string)this["cssUrl"]; }
			set { this["cssUrl"] = value; }
		}

		public IDocumentationConfiguration DocumentationConfiguration
		{
			get { return DocumentationElement; }
			set { DocumentationElement = (DocumentationElement)value; }
		}

		[ConfigurationProperty("documentation")]
		public DocumentationElement DocumentationElement
		{
			get { return (DocumentationElement)this["documentation"]; }
			set { this["documentation"] = value; }
		}

		public IEditHelpConfiguration EditHelpConfiguration
		{
			get { return EditHelpElement; }
			set { EditHelpElement = (EditHelpElement) value; }
		}

		[ConfigurationProperty("editHelp")]
		public EditHelpElement EditHelpElement
		{
			get { return (EditHelpElement)this["editHelp"]; }
			set { this["editHelp"] = value; }
		}

		public IPopupViewerConfiguration PopupViewerConfiguration
		{
			get { return PopupViewerElement; }
			set { PopupViewerElement = (PopupViewerElement)value; }
		}

		[ConfigurationProperty("popupViewer")]
		public PopupViewerElement PopupViewerElement
		{
			get { return (PopupViewerElement)this["popupViewer"]; }
			set { this["popupViewer"] = value; }
		}

		#endregion
	}
}
