﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Configuration;
using DocMAH.Web.Html;
using DocMAH.Web.Requests;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Html
{
	[TestFixture]
	public class HtmlBuilderTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Returns empty content if button is hidden.")]
		public void CreateButtonHelp_Hidden()
		{
			// Arrange
			var isHidden = true;
			var buttonHtml = "<span>[TEXT]</span><span>[DESCRIPTION]</span>";
			var text = "Button Text";
			var description = "Button Description";

			var htmlBuilder = new HtmlBuilder(null, null, null, null, null, null, null, null);

			// Act
			var result = htmlBuilder.CreateButtonHelp(isHidden, buttonHtml, text, description);

			// Assert
			Assert.That(result, Is.EqualTo(string.Empty), "The button content should be hidden.");
		}

		[Test]
		[Description("Returns formatted button content.")]
		public void CreateHelpButton_Visible()
		{
			// Arrange
			var isHidden = false;
			var buttonFormat = "<span>{0}</span><span>{1}</span>";
			var buttonHtml = string.Format(buttonFormat, "[TEXT]", "[DESCRIPTION]");
			var text = "Button Text";
			var description = "Button Description";

			var htmlBuilder = new HtmlBuilder(null, null, null, null, null, null, null, null);

			// Act
			var result = htmlBuilder.CreateButtonHelp(isHidden, buttonHtml, text, description);

			// Assert
			var expected = string.Format(buttonFormat, text, description);
			Assert.That(result, Is.EqualTo(expected), "The button content should be visible and formatted.");
		}

		[Test]
		[Description("Pull cached results for documentation page HTML.")]
		public void CreateDocumentationPageHtml_CachedResponse()
		{
			// Arrange
			var html = "<html><body>This is only a test.</body></html>";

			var memoryCache = Mocks.Create<ObjectCache>();
			memoryCache.Setup(c => c.Contains(HtmlBuilder._documentationHtmlCacheKey, null as string)).Returns(true);
			memoryCache.Setup(c => c.Get(HtmlBuilder._documentationHtmlCacheKey, null as string)).Returns(html);

			var builder = new HtmlBuilder(null, null, null, null, null, memoryCache.Object, null, null);

			// Act
			var result = builder.CreateDocumentationPageHtml();

			// Assert
			Assert.That(result, Is.EqualTo(html), "The result should be the HTML returned from the cache.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Creates the documentation page HTML using configured values, caches the HTML and returns the response.")]
		public void CreateDocumentationPageHtml_FirstResponseConfiguredUrls()
		{
			// Arrange
			var titleToken = "[TITLE]";
			var jsTreeCssToken = "[JSTREECSS]";
			var customCssToken = "[CUSTOMCSS]";
			var jQueryUrlToken = "[JQUERYURL]";
			var jQueryUiUrlToken = "[JQUERYUIURL]";
			var jsTreeUrlToken = "[JSTREEURL]";
			var html = string.Concat(titleToken, jsTreeCssToken, customCssToken, jQueryUrlToken, jQueryUiUrlToken, jsTreeUrlToken);

			var title = "Test Title -";
			var jsTreeCss = "jsTree CSS -";
			var customCss = "custom CSS - ";
			var jQueryUrl = "jQuery URL - ";

			var minifier = Mocks.Create<IMinifier>();
			minifier.Setup(m => m.Minify(HtmlContent.Documentation, HtmlContent.Documentation_min)).Returns(html);

			Configuration.DocumentationConfiguration.Setup(c => c.PageTitle).Returns(title);
			Configuration.DocumentationConfiguration.Setup(c => c.CustomCss).Returns(customCss);
			Configuration.DocmahConfiguration.Setup(c => c.CssUrl).Returns(jsTreeCss);
			Configuration.DocmahConfiguration.Setup(c => c.JsUrl).Returns(jQueryUrl);

			var memoryCache = Mocks.Create<ObjectCache>();
			memoryCache.Setup(c => c.Contains(HtmlBuilder._documentationHtmlCacheKey, null as string)).Returns(false);
			memoryCache.Setup(c => c.Set(HtmlBuilder._documentationHtmlCacheKey, It.IsAny<string>(), It.IsAny<CacheItemPolicy>(), null as string));

			var htmlBuilder = new HtmlBuilder(null, null, Configuration.Object, null, null, memoryCache.Object, minifier.Object, null);

			// Act
			var result = htmlBuilder.CreateDocumentationPageHtml();

			// Assert
			Assert.That(result.Contains(titleToken), Is.False, "The title token should be replaced.");
			Assert.That(result.Contains(jsTreeCssToken), Is.False, "The jsTree CSS token should be replaced.");
			Assert.That(result.Contains(customCssToken), Is.False, "The custom CSS token should be replaced.");
			Assert.That(result.Contains(jQueryUrlToken), Is.False, "The jQuery URL token should be replaced.");
			Assert.That(result.Contains(jQueryUiUrlToken), Is.False, "The jQuery UI URL token should be replaced.");
			Assert.That(result.Contains(jsTreeUrlToken), Is.False, "The jsTree URL token should be replaced.");

			Assert.That(result.Contains(title), Is.True, "The configured title should be inserted.");
			Assert.That(result.Contains(jsTreeCss), Is.True, "The configured jsTree CSS URL should be inserted.");
			Assert.That(result.Contains(customCss), Is.True, "The configured custom CSS URL should be instered.");
			Assert.That(result.Contains(jQueryUrl), Is.True, "The configured jQuery URL should be inserted.");
			Assert.That(result.Contains(CdnUrls.jsJQueryUi), Is.False, "The jQuery UI URL should not be inserted when a custom JS URL is configured.");
			Assert.That(result.Contains(CdnUrls.jsJsTree), Is.False, "The jsTree URL should not be inserted when a custom JS URL is configured.");

			Mocks.VerifyAll();
		}

		#endregion
	}
}
