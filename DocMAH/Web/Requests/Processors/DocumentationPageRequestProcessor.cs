using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DocMAH.Configuration;
using DocMAH.Properties;
using DocMAH.Web.Html;

namespace DocMAH.Web.Requests.Processors
{
	internal class DocumentationPageRequestProcessor : IRequestProcessor
	{
		#region Constructors
		
		internal DocumentationPageRequestProcessor(IHtmlBuilder htmlBuilder)
		{
			_htmlBuilder = htmlBuilder;
		}

		#endregion

		#region Private Fields

		private readonly IHtmlBuilder _htmlBuilder;

		#endregion

		#region Private Methods

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			// Set return values.
			return new ResponseState
			{
				Content = _htmlBuilder.CreateDocumentationPageHtml(),
				ContentType = ContentTypes.Html,
			};
		}

		#endregion
	}
}
