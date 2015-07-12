﻿using System;
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
		
		internal DocumentationPageRequestProcessor(IDocumentationConfiguration documentationConfiguration, IHtmlBuilder htmlBuilder)
		{
			_documentationConfiguration = documentationConfiguration;
			_htmlBuilder = htmlBuilder;
		}

		#endregion

		#region Private Fields

		private readonly IDocumentationConfiguration _documentationConfiguration;
		private readonly IHtmlBuilder _htmlBuilder;

		#endregion

		#region Private Methods

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			// Return not found result if the documentation page is disabled.
			if (_documentationConfiguration.Disabled)
				return new ResponseState { StatusCode = HttpStatusCode.NotFound };

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
