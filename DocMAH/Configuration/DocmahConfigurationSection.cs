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

		[ConfigurationProperty("documentation")]
		public DocumentationElement DocumentationConfiguration
		{
			get { return (DocumentationElement)this["documentation"]; }
			set { this["documentation"] = value; }
		}

		[ConfigurationProperty("editHelp")]
		public EditHelpElement EditHelpConfiguration
		{
			get { return (EditHelpElement)this["editHelp"]; }
			set { this["editHelp"] = value; }
		}

		#endregion
	}
}
