using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DocMAH.Web.Requests.Processors
{
	public class UnauthorizedRequestProcessor : IRequestProcessor
	{
		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			return new ResponseState
			{
				Content = "<html><body><h2>Unauthorized</h2></body></html>",
				StatusCode = HttpStatusCode.Unauthorized,
			};
		}

		#endregion
	}
}
