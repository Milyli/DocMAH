using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Configuration
{
	public interface IPopupViewerConfiguration
	{
		IClosePopupButtonConfiguration ClosePopupButtonConfiguration { get; set; }
		IHidePopupButtonConfiguration HidePopupButtonConfiguration { get; set; }
	}
}
