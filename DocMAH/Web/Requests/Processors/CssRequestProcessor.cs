using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using DocMAH.Properties;

namespace DocMAH.Web.Requests.Processors
{
	public class CssRequestProcessor : IRequestProcessor
	{
		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			return new ResponseState
			{
				Content = ResourcesExtensions.Minify(Resources.DocMAHStyles, Resources.DocMAHStyles_min),
				ContentType = ContentTypes.Css,		
			};
		}

		public string RequestType
		{
			get { return RequestTypes.Css; }
		}

		public bool RequiresEditAuthorization
		{
			get { return false; }
		}

		#endregion
	}
}
