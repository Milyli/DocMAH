using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocMAH.Configuration
{
	public class EditHelpElement : ConfigurationElement, IEditHelpConfiguration
	{
		[ConfigurationProperty("requireAuthentication", DefaultValue = true)]
		public virtual bool RequireAuthentication
		{
			get { return (bool)this["requireAuthentication"]; }
			set { this["requireAuthentication"] = value; }
		}

		[ConfigurationProperty("requireLocalConnection", DefaultValue = true)]
		public virtual bool RequireLocalConnection
		{
			get { return (bool)this["requireLocalConnection"]; }
			set { this["requireLocalConnection"] = value; }
		}

		[ConfigurationProperty("disabled", DefaultValue = false)]
		public virtual bool IsDisabled
		{
			get { return (bool)this["disabled"]; }
			set { this["disabled"] = value; }
		}
	}
}
