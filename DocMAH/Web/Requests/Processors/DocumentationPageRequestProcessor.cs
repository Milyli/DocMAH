using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using DocMAH.Configuration;
using DocMAH.Properties;

namespace DocMAH.Web.Requests.Processors
{
	public class DocumentationPageRequestProcessor : IRequestProcessor
	{
		#region Constructors
		
		public DocumentationPageRequestProcessor(
			HttpContextBase httpContext, 
			IContentConfiguration contentConfiguration, 
			IDocumentationConfiguration documentationConfiguration, 
			IMinifier minifier)
		{
			_httpContext = httpContext;
			_contentConfiguration = contentConfiguration;
			_documentationConfiguration = documentationConfiguration;
			_minifier = minifier;
		}

		#endregion

		#region Private Fields

		private readonly HttpContextBase _httpContext;
		private readonly IContentConfiguration _contentConfiguration;
		private readonly IDocumentationConfiguration _documentationConfiguration;
		private readonly IMinifier _minifier;

		#endregion

		#region Private Methods

		/// <summary>
		/// By default (unconfigured jsUrl), creates a script link for the CDN URL provided.
		/// If configured, returns an empty string assuming that the first JavaScript link, 
		/// which does not use this method, has created a link to a bundle with all files.
		/// </summary>
		/// <param name="cdnUrl"></param>
		/// <returns></returns>
		private string CreateBundledOrDefaultScriptLink(string cdnUrl)
		{
			if (string.IsNullOrEmpty(_contentConfiguration.JsUrl))
				return string.Format(LinkFormats.JavaScript, cdnUrl);
			else
				return string.Empty;
		}

		#endregion

		#region IRequestProcessor Members

		public ResponseState Process(string data)
		{
			var documentationHtml = _minifier.Minify(Resources.Html_Documentation, Resources.Html_Documentation_min);

			documentationHtml = documentationHtml.Replace("[TITLE]", _documentationConfiguration.PageTitle);

			// TODO: This will be removed when the return link is changed to a close link.
			var request = _httpContext.Request;
			var returnLink = new UriBuilder(request.Url.Scheme, request.Url.Host, request.Url.Port, request.ApplicationPath);
			documentationHtml = documentationHtml.Replace("[RETURNLINK]", returnLink.ToString());

			var cssUrl = _contentConfiguration.CssUrl;
			if (string.IsNullOrEmpty(cssUrl))
				cssUrl = CdnUrls.cssJsTree;
			documentationHtml = documentationHtml.Replace("[JSTREECSS]", string.Format(LinkFormats.Css, cssUrl));

			var customCssLink = string.Empty;
			var customCssUrl = _documentationConfiguration.CustomCss;
			if (!string.IsNullOrEmpty(customCssUrl))
				customCssLink = string.Format(LinkFormats.Css, customCssUrl);
			documentationHtml = documentationHtml.Replace("[CUSTOMCSS]", customCssLink);

			var jQueryUrl = _contentConfiguration.JsUrl;
			if (string.IsNullOrEmpty(jQueryUrl))
				jQueryUrl = CdnUrls.jsJQuery;
			documentationHtml = documentationHtml.Replace("[JQUERYURL]", string.Format(LinkFormats.JavaScript, jQueryUrl));

			documentationHtml = documentationHtml.Replace("[JQUERYUIURL]", CreateBundledOrDefaultScriptLink(CdnUrls.jsJQueryUi));

			documentationHtml = documentationHtml.Replace("[JSTREEURL]", CreateBundledOrDefaultScriptLink(CdnUrls.jsJsTree));

			documentationHtml = documentationHtml.Replace("[firstTimeViewHTML]",
				_minifier.Minify(Resources.Html_FirstTimeView, Resources.Html_FirstTimeView_min)
			);

			// Set return values.
			return new ResponseState
			{
				Content = documentationHtml,
				ContentType = ContentTypes.Html,
			};
		}

		#endregion
	}
}
