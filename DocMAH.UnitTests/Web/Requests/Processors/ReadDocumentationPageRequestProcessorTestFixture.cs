using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using DocMAH.Data;
using DocMAH.Models;
using DocMAH.Web.Requests;
using DocMAH.Web.Requests.Processors;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	[TestFixture]
	public class ReadDocumentationPageRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Processes a request for nonexistent page.")]
		public void Process_NotFound()
		{
			// Arrange
			var pageId = 389432;

			var pageRepository = Mocks.Create<IDocumentationPageRepository>();
			pageRepository.Setup(r => r.Read(pageId)).Returns(null as DocumentationPage);

			var processor = new ReadDocumentationPageRequestProcessor(pageRepository.Object);

			// Act
			var result = processor.Process(pageId.ToString());

			// Assert
			Assert.That(result, Is.Not.Null, "A valid ResponseState should be returned.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

			Mocks.VerifyAll();
		}

		[Test]
		[Description("Reads a page and its bullets from the data store.")]
		public void Process_Success()
		{
			// Arrange
			var page = Models.CreateDocumentationPage(id: 84932);

			var pageRepository = Mocks.Create<IDocumentationPageRepository>();
			pageRepository.Setup(r => r.Read(page.Id)).Returns(page);

			var processor = new ReadDocumentationPageRequestProcessor(pageRepository.Object);

			// Act
			var result = processor.Process(page.Id.ToString());

			// Assert
			Assert.That(result, Is.Not.Null, "A valid ResponseState should be returned.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The request should succeed.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "JSON should be returned in the response.");
			var serializer = new JavaScriptSerializer();
			var clientPage = serializer.Deserialize<DocumentationPage>(result.Content);
			Assert.That(clientPage.Id, Is.EqualTo(page.Id), "The page read from the repository should be included in the result.");

			Mocks.VerifyAll();
		}

		#endregion
	}
}
