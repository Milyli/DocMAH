using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace DocMAH.Configuration
{
	public class PopupViewerElement : ConfigurationElement, IPopupViewerConfiguration
	{
		public IHidePopupButtonConfiguration HidePopupButtonConfiguration
		{
			get { return HidePopupButtonElement; }
			set { HidePopupButtonElement = (HidePopupButtonElement)value; }
		}

		[ConfigurationProperty("hidePopupButton")]
		public HidePopupButtonElement HidePopupButtonElement
		{
			get { return (HidePopupButtonElement)this["hidePopupButton"]; }
			set { this["hidePopupButton"] = value; }
		}

		public IClosePopupButtonConfiguration ClosePopupButtonConfiguration
		{
			get { return ClosePopupButtonElement; }
			set { ClosePopupButtonElement = (ClosePopupButtonElement)value; }
		}

		[ConfigurationProperty("closePopupButton")]
		public ClosePopupButtonElement ClosePopupButtonElement
		{
			get { return (ClosePopupButtonElement)this["closePopupButton"]; }
			set { this["closePopupButton"] = value; }
		}
	}
}
