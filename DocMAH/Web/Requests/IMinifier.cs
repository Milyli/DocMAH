using System;
using System.Collections.Generic;
using System.Linq;

namespace DocMAH.Web.Requests
{
	public interface IMinifier
	{
		string Minify(string full, string minified);
	}
}
