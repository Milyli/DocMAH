using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DocMAH.Web.Requests.Processors
{
	public class NotFoundRequestProcessor : IRequestProcessor
	{
		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			return new ResponseState
			{
				Content = "<html><body>404 - Not Found</body></html>",
				StatusCode = HttpStatusCode.NotFound,
			};
		}

		public string RequestType
		{
			get { return RequestTypes.NotFound; }
		}

		public bool RequiresEditAuthorization
		{
			get { return false; }
		}

		#endregion
	}
}
