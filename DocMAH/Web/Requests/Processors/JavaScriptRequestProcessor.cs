using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocMAH.Properties;

namespace DocMAH.Web.Requests.Processors
{
	public class JavaScriptRequestProcessor : IRequestProcessor
	{
		#region Constructors

		public JavaScriptRequestProcessor(IMinifier minifier)
		{
			_minifier = minifier;
		}

		#endregion

		#region Private Fields

		private readonly IMinifier _minifier;

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			return new ResponseState
			{
				Content = _minifier.Minify(Resources.DocMAHJavaScript, Resources.DocMAHJavaScript_min),
				ContentType = ContentTypes.JavaScript,
			};
		}

		#endregion
	}
}
