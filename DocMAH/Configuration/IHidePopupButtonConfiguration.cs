using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Configuration
{
	public interface IHidePopupButtonConfiguration
	{
		string Description { get; set; }

		bool IsHidden { get; set; }

		string Text { get; set; }
	}
}
