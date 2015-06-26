using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Properties;
using DocMAH.Web.Requests;
using DocMAH.Web.Requests.Processors;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	
	[TestFixture]
	public class CssRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Processes a CSS request.")]
		public void Process_Success()
		{
			// Arrange
			var testCss = "Test CSS";

			var minifier = Mocks.Create<IMinifier>();
			minifier.Setup(m => m.Minify(Resources.DocMAHStyles, Resources.DocMAHStyles_min)).Returns(testCss);

			var processor = new CssRequestProcessor(minifier.Object);

			// Act
			var result = processor.Process(null);

			// Assert
			Assert.That(result, Is.Not.Null, "A valid ResponseState should be returned.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Css), "The response should contain CSS.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The response should be successful.");
			Assert.That(result.Content, Is.EqualTo(testCss), "The response should contain the content provided by the minifier.");
		}

		#endregion
	}
}
