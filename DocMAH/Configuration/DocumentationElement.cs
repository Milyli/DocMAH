using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Configuration
{
	public class DocumentationElement : ConfigurationElement
	{
		[ConfigurationProperty("pageTitle", DefaultValue = "Documentation")]
		public string PageTitle
		{
			get { return (string)this["pageTitle"]; }
			set { this["pageTitle"] = value; }
		}

		[ConfigurationProperty("customCss", DefaultValue = null)]
		public string CustomCss
		{
			get { return (string)this["customCss"]; }
			set { this["customCss"] = value; }
		}
	}
}
