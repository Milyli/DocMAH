using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using DocMAH.Configuration;
using DocMAH.Data;
using DocMAH.Models;
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;

namespace DocMAH.Web.Html
{
	internal class HtmlBuilder : IHtmlBuilder
	{
		#region Constructors

		internal HtmlBuilder(
			HttpContextBase httpContext,
			IBulletRepository bulletRepository,
			IContentConfiguration contentConfiguration,
			IDocumentationConfiguration documentationConfiguration,
			IEditAuthorizer editAuthorizer,
			IMinifier minifier,
			IPageRepository pageRepository,
			IUserPageSettingsRepository userPageSettingsRepository)
		{
			_httpContext = httpContext;
			_bulletRepository = bulletRepository;
			_contentConfiguration = contentConfiguration;
			_documentationConfiguration = documentationConfiguration;
			_editAuthorizer = editAuthorizer;
			_minifier = minifier;
			_pageRepository = pageRepository;
			_userPageSettingsRepository = userPageSettingsRepository;
		}

		#endregion

		#region Private Fields

		private readonly HttpContextBase _httpContext;
		private readonly IBulletRepository _bulletRepository;
		private readonly IContentConfiguration _contentConfiguration;
		private readonly IDocumentationConfiguration _documentationConfiguration;
		private readonly IEditAuthorizer _editAuthorizer;
		private readonly IMinifier _minifier;
		private readonly IPageRepository _pageRepository;
		private readonly IUserPageSettingsRepository _userPageSettingsRepository;

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

		#region Public Methods

		public string CreateDocumentationPageHtml(){
			var result = _minifier.Minify(HtmlContent.Documentation, HtmlContent.Documentation_min);

			result = result.Replace("[TITLE]", _documentationConfiguration.PageTitle);

			// TODO: This will be removed when the return link is changed to a close link.
			var request = _httpContext.Request;
			var returnLink = new UriBuilder(request.Url.Scheme, request.Url.Host, request.Url.Port, request.ApplicationPath);
			result = result.Replace("[RETURNLINK]", returnLink.ToString());

			var cssUrl = _contentConfiguration.CssUrl;
			if (string.IsNullOrEmpty(cssUrl))
				cssUrl = CdnUrls.cssJsTree;
			result = result.Replace("[JSTREECSS]", string.Format(LinkFormats.Css, cssUrl));

			var customCssLink = string.Empty;
			var customCssUrl = _documentationConfiguration.CustomCss;
			if (!string.IsNullOrEmpty(customCssUrl))
				customCssLink = string.Format(LinkFormats.Css, customCssUrl);
			result = result.Replace("[CUSTOMCSS]", customCssLink);

			var jQueryUrl = _contentConfiguration.JsUrl;
			if (string.IsNullOrEmpty(jQueryUrl))
				jQueryUrl = CdnUrls.jsJQuery;
			result = result.Replace("[JQUERYURL]", string.Format(LinkFormats.JavaScript, jQueryUrl));

			result = result.Replace("[JQUERYUIURL]", CreateBundledOrDefaultScriptLink(CdnUrls.jsJQueryUi));

			result = result.Replace("[JSTREEURL]", CreateBundledOrDefaultScriptLink(CdnUrls.jsJsTree));

			result = result.Replace("[firstTimeViewHTML]",
				_minifier.Minify(HtmlContent.FirstTimeView, HtmlContent.FirstTimeView_min)
			);

			return result;
		}

		public string CreateFirstTimeHelpHtml()
		{
			var requestUrl = HttpContext.Current.Request.Url.AbsolutePath;
			var page = _pageRepository.ReadByUrl(requestUrl.Replace('*', '%'));
			UserPageSettings userPageSettings = null;

			if (null != page)
			{
				page.Bullets = _bulletRepository.ReadByPageId(page.Id);
				if (_httpContext.Request.IsAuthenticated)
				{
					var userName = _httpContext.User.Identity.Name;
					userPageSettings = _userPageSettingsRepository.Read(userName, page.Id);
				}
			}

			var result = string.Empty;

			// The HTML is reused on the documentation page.
			// When injecting into other requests, the initialization scripts must be included.
			result += _minifier.Minify(HtmlContent.FirstTimeView, HtmlContent.FirstTimeView_min);

			// TODO: Iron out javascript reference injection for first time help in base site pages.
			// Attach jQueryUi CDN locations if not configured.
			// Leaving out jQuery for the time being as it's likely included.
			var javaScriptDependencies = _contentConfiguration.JsUrl;
			if (string.IsNullOrEmpty(javaScriptDependencies))
			{
				result += string.Format("<script src='{0}' type='application/javascript'></script>", CdnUrls.jsJQuery);
				result += string.Format("<script src='{0}' type='application/javascript'></script>", CdnUrls.jsJQueryUi);
			}

			result += _minifier.Minify(HtmlContent.FirstTimeViewInjectedScripts, HtmlContent.FirstTimeViewInjectedScripts_min);
			
			if (_editAuthorizer.Authorize())
			{
				result += _minifier.Minify(HtmlContent.FirstTimeEdit, HtmlContent.FirstTimeEdit_min);
			}

			// TODO: Cache the non-dynamic portion of the first time HTML for faster loading.

			var serializer = new JavaScriptSerializer();
			var pageJson = serializer.Serialize(page);
			result = result.Replace("[PAGEJSON]", pageJson);

			var userPageSettingsJson = serializer.Serialize(userPageSettings);
			result = result.Replace("[USERPAGESETTINGSJSON]", userPageSettingsJson);

			var applicationSettings = new ApplicationSettings { CanEdit = _editAuthorizer.Authorize() };
			var applicationSettingsJson = serializer.Serialize(applicationSettings);
			result = result.Replace("[APPLICATIONSETTINGSJSON]", applicationSettingsJson);

			return result;

		}

		public string CreateFirstTimeHelpCssLink()
		{
			return HtmlContent.CssLink;
		}

		#endregion
	}
}
