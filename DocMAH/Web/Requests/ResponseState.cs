using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace DocMAH.Web.Requests
{
	public class ResponseState
	{
		#region Constructors

		public ResponseState()
		{
			Content = "Success";
			ContentType = ContentTypes.Html;
			Disposition = string.Empty;
			StatusCode = HttpStatusCode.OK;
		}

		#endregion

		#region Public Properties

		public string Content { get; set; }
				
		public string ContentType { get; set; }

		public string Disposition { get; set; }

		public HttpStatusCode StatusCode { get; set; }
		
		#endregion
	}
}
