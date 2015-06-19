using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocMAH.Properties;

namespace DocMAH.Web.Requests.Processors
{
	public class JavaScriptRequestProcessor : IRequestProcessor
	{
		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			return new ResponseState
			{
				Content = ResourcesExtensions.Minify(Resources.DocMAHJavaScript, Resources.DocMAHJavaScript_min),
				ContentType = ContentTypes.JavaScript,
			};
		}

		public string RequestType
		{
			get { return RequestTypes.JavaScript; }
		}

		public bool RequiresEditAuthorization
		{
			get { return false; }
		}

		#endregion
	}
}
