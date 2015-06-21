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
		#region Constructors

		public CssRequestProcessor(IMinifier minifier)
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
				Content = _minifier.Minify(Resources.DocMAHStyles, Resources.DocMAHStyles_min),
				ContentType = ContentTypes.Css,		
			};
		}

		public bool RequiresEditAuthorization
		{
			get { return false; }
		}

		#endregion
	}
}
