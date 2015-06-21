using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Web.Requests;
using DocMAH.Web.Requests.Processors;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	[TestFixture]
	public class JavaScriptRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Returns JavaScript content. ")]
		public void Process_Success()
		{
			// Arrange
			var minifier = Mocks.Create<IMinifier>();
			minifier.Setup(m => m.Minify(It.IsAny<string>(), It.IsAny<string>())).Returns("Content");

			var processor = new JavaScriptRequestProcessor(minifier.Object);

			// Act
			var result = processor.Process(string.Empty);

			// Assert
			Assert.That(result, Is.Not.Null, "Response data expected.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Valid HTTP response code expected.");
			Assert.That(result.Content, Is.EqualTo("Content"), "Minified content expected.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.JavaScript), "JavaScript ContentType expected.");
		}

		#endregion
	}
}
