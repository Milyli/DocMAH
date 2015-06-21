using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Configuration
{
	public interface IContentConfiguration
	{
		string ConnectionStringName { get; set; }
		string JsUrl { get; set; }
		string CssUrl { get; set; }
	}
}
