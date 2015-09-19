using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace DocMAH.Configuration
{
	public class HidePopupButtonElement : ConfigurationElement, IHidePopupButtonConfiguration
	{
		[ConfigurationProperty("isHidden", DefaultValue = false)]
		public bool IsHidden
		{
			get { return (bool)this["isHidden"]; }
			set { this["isHidden"] = value; }
		}

		[ConfigurationProperty("text", DefaultValue = "Got it")]
		public string Text
		{
			get { return (string)this["text"]; }
			set { this["text"] = value; }
		}

		[ConfigurationProperty("description", DefaultValue = "Do not show this help next time")]
		public string Description
		{
			get { return (string)this["description"]; }
			set { this["description"] = value; }
		}
	}
}
