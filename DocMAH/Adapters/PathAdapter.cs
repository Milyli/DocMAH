using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace DocMAH.Adapters
{
	public class PathAdapter : IPath
	{
		#region IPath Members

		public string MapPath(string virtualPath)
		{
			return HostingEnvironment.MapPath(virtualPath);
		}

		#endregion
	}
}
