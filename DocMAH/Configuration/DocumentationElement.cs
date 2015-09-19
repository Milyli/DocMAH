using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Configuration
{
	public class DocumentationElement : ConfigurationElement, IDocumentationConfiguration
	{
		[ConfigurationProperty("pageTitle", DefaultValue = "Documentation")]
		public virtual string PageTitle
		{
			get { return (string)this["pageTitle"]; }
			set { this["pageTitle"] = value; }
		}

		[ConfigurationProperty("customCss", DefaultValue = null)]
		public virtual string CustomCss
		{
			get { return (string)this["customCss"]; }
			set { this["customCss"] = value; }
		}

		[ConfigurationProperty("disabled", DefaultValue = false)]
		public virtual bool Disabled
		{
			get { return (bool)this["disabled"]; }
			set { this["disabled"] = value; }
		}
	}
}
