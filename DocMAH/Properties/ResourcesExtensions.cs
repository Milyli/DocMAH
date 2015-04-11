using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DocMAH.Properties
{
	internal static class ResourcesExtensions
	{
		internal static string Minify(string full, string minified)
		{
			return 
				Debugger.IsAttached 
				|| (HttpContext.Current != null &&  HttpContext.Current.IsDebuggingEnabled)
			? full : minified;
		}
	}
}
