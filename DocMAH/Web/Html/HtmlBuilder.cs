using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
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
			IDocmahConfiguration docmahConfiguration,
			IEditAuthorizer editAuthorizer,
			IFirstTimeHelpRepository firstTimeHelpRepository,
			ObjectCache memoryCache,
			IMinifier minifier,
			IUserPageSettingsRepository userPageSettingsRepository)
		{
			_httpContext = httpContext;
			_bulletRepository = bulletRepository;
			_docmahConfiguration = docmahConfiguration;
			_editAuthorizer = editAuthorizer;
			_firstTimeHelpRepository = firstTimeHelpRepository;
			_memoryCache = memoryCache;
			_minifier = minifier;
			_userPageSettingsRepository = userPageSettingsRepository;
		}

		#endregion

		#region Private Fields

		internal const string _documentationHtmlCacheKey = "Documentation HTML";
		internal const string _firstTimeHelpHtmlCacheKey = "First Time Help HTML";

		private readonly HttpContextBase _httpContext;
		private readonly IBulletRepository _bulletRepository;
		private readonly IDocmahConfiguration _docmahConfiguration;
		private readonly IEditAuthorizer _editAuthorizer;
		private readonly IFirstTimeHelpRepository _firstTimeHelpRepository;
		private readonly ObjectCache _memoryCache;
		private readonly IMinifier _minifier;
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
			if (string.IsNullOrEmpty(_docmahConfiguration.JsUrl))
				return string.Format(LinkFormats.JavaScript, cdnUrl);
			else
				return string.Empty;
		}

		#endregion

		#region Public Methods

		public string CreateButtonHelp(bool isHidden, string buttonHtml, string text, string description)
		{
			var result = string.Empty;

			if (!isHidden)
			{
				result = buttonHtml.Replace("[TEXT]", text).Replace("[DESCRIPTION]", description);
			}

			return result;
		}

		public string CreateDocumentationPageHtml()
		{
			var result = string.Empty;

			if (_memoryCache.Contains(_documentationHtmlCacheKey))
			{
				result = (string)_memoryCache.Get(_documentationHtmlCacheKey);
			}
			else
			{
				// Get base HTML.
				result = _minifier.Minify(HtmlContent.Documentation, HtmlContent.Documentation_min);

				// Replace the page title with the configured title.
				result = result.Replace("[TITLE]", _docmahConfiguration.DocumentationConfiguration.PageTitle);

				// Replace the JSTree CDN URL with a configured local URL that contains the CSS.
				var cssUrl = _docmahConfiguration.CssUrl;
				if (string.IsNullOrEmpty(cssUrl))
					cssUrl = CdnUrls.cssJsTree;
				result = result.Replace("[JSTREECSS]", string.Format(LinkFormats.Css, cssUrl));

				// Custom CSS token will be replaced with empty string if it is not configured.
				var customCssLink = string.Empty;
				var customCssUrl = _docmahConfiguration.DocumentationConfiguration.CustomCss;
				if (!string.IsNullOrEmpty(customCssUrl))
					customCssLink = string.Format(LinkFormats.Css, customCssUrl);
				result = result.Replace("[CUSTOMCSS]", customCssLink);

				// Replace JS tokens with configured value or CDN defaults.
				var jQueryUrl = _docmahConfiguration.JsUrl;
				if (string.IsNullOrEmpty(jQueryUrl))
					jQueryUrl = CdnUrls.jsJQuery;
				result = result.Replace("[JQUERYURL]", string.Format(LinkFormats.JavaScript, jQueryUrl));

				result = result.Replace("[JQUERYUIURL]", CreateBundledOrDefaultScriptLink(CdnUrls.jsJQueryUi));

				result = result.Replace("[JSTREEURL]", CreateBundledOrDefaultScriptLink(CdnUrls.jsJsTree));

				_memoryCache.Set(_documentationHtmlCacheKey, result, new CacheItemPolicy());
			}

			return result;
		}

		public string CreateFirstTimeHelpHtml()
		{
			var result = string.Empty;

			if (_memoryCache.Contains(_firstTimeHelpHtmlCacheKey))
				result = (string)_memoryCache.Get(_firstTimeHelpHtmlCacheKey);
			else
			{
				// Determine if content should be minified.
				var popupViewContent = _minifier.Minify(HtmlContent.FirstTimeView, HtmlContent.FirstTimeView_min);

				// Modify buttons per configuration settings.
				var hideHelpConfiguration = _docmahConfiguration.PopupViewerConfiguration.HidePopupButtonConfiguration;
				popupViewContent = popupViewContent.Replace("[HIDEHELPBUTTON]", CreateButtonHelp(hideHelpConfiguration.IsHidden, HtmlContent.HideHelpButton, hideHelpConfiguration.Text, hideHelpConfiguration.Description));
				var closeHelpConfiguration = _docmahConfiguration.PopupViewerConfiguration.ClosePopupButtonConfiguration;
				popupViewContent = popupViewContent.Replace("[CLOSEHELPBUTTON]", CreateButtonHelp(closeHelpConfiguration.IsHidden, HtmlContent.CloseHelpButton, closeHelpConfiguration.Text, closeHelpConfiguration.Description));

				// When injecting into other requests, the initialization scripts must be included.
				result += popupViewContent;

				// Attach jQueryUi CDN locations if not configured.
				var javaScriptDependencies = _docmahConfiguration.JsUrl;
				if (string.IsNullOrEmpty(javaScriptDependencies))
				{
					result += string.Format("<script src='{0}' type='application/javascript'></script>", CdnUrls.jsJQuery);
					result += string.Format("<script src='{0}' type='application/javascript'></script>", CdnUrls.jsJQueryUi);
				}

				result += _minifier.Minify(HtmlContent.FirstTimeViewInjectedScripts, HtmlContent.FirstTimeViewInjectedScripts_min);

				_memoryCache.Set(_firstTimeHelpHtmlCacheKey, result, new CacheItemPolicy());
			}

			// Begin reading request specific values.
			if (_editAuthorizer.Authorize())
			{
				result += _minifier.Minify(HtmlContent.FirstTimeEdit, HtmlContent.FirstTimeEdit_min);
			}

			var requestUrl = HttpContext.Current.Request.Url.AbsolutePath;
			var page = _firstTimeHelpRepository.ReadByUrl(requestUrl.Replace('*', '%'));
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

			var serializer = new JavaScriptSerializer();
			var pageJson = serializer.Serialize(page);
			result = result.Replace("[PAGEJSON]", pageJson);

			var userPageSettingsJson = serializer.Serialize(userPageSettings);
			result = result.Replace("[USERPAGESETTINGSJSON]", userPageSettingsJson);

			// TODO: Refactor application settings creation into factory. Replace here and application settings request processor.
			var applicationSettings = new ApplicationSettings { CanEdit = _editAuthorizer.Authorize(), DisableDocumentation = _docmahConfiguration.DocumentationConfiguration.Disabled };
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
