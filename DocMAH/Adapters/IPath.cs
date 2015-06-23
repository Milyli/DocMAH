using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocMAH.Adapters
{
	public interface IPath
	{
		/// <summary>
		/// Adapts System.Web.HostingEnvironment.MapPath(string virtualPath).
		/// This is the same as Server.MapPath but is available without an HttpContext.
		/// Used when unit tests require the ability to mock this method.
		/// </summary>
		/// <param name="virtualPath"></param>
		/// <returns></returns>
		string MapPath(string virtualPath);
	}
}
