using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace DocMAH.Configuration
{
	public class ClosePopupButtonElement : ConfigurationElement, IClosePopupButtonConfiguration
	{
		[ConfigurationProperty("isHidden", DefaultValue = false)]
		public bool IsHidden {
			get { return (bool)this["isHidden"]; }
			set { this["isHidden"] = value; }
		}

		[ConfigurationProperty("text", DefaultValue = "Remind me later")]
		public string Text
		{
			get { return (string)this["text"]; }
			set { this["text"] = value; }
		}

		[ConfigurationProperty("description", DefaultValue = "Show this help when I come back to the page")]
		public string Description
		{
			get { return (string)this["description"]; }
			set { this["description"] = value; }
		}
	}
}
