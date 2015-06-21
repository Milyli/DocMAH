using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Configuration
{
	public interface IDocumentationConfiguration
	{
		string PageTitle { get; set; }
		string CustomCss { get; set; }
	}
}
