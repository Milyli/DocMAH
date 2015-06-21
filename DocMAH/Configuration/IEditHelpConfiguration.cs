using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Configuration
{
	public interface IEditHelpConfiguration
	{
		bool RequireAuthentication { get; set; }
		bool RequireLocalConnection { get; set; }
		bool IsDisabled { get; set; }
	}
}
