using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Data;
using DocMAH.Models;
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;
using DocMAH.Web.Requests.Processors;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	[TestFixture]
	public class ReadTableOfContentsRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Returns the table of contents for all help pages.")]
		public void Process_Success()
		{
			// Arrange
			var isAuthorized = true;

			var editAuthorizer = Mocks.Create<IEditAuthorizer>();
			editAuthorizer.Setup(a => a.Authorize()).Returns(isAuthorized);

			var pageRepository = Mocks.Create<IDocumentationPageRepository>();
			pageRepository.Setup(r => r.ReadTableOfContents(isAuthorized)).Returns(new List<DocumentationPage>());

			var processor = new ReadTableOfContentsRequestProcessor(editAuthorizer.Object, pageRepository.Object);

			// Act
			var result = processor.Process(null);

			// Assert
			Assert.That(result, Is.Not.Null, "A valid ResponseState should be returned.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "The response should contain JSON");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The request should succeed.");
		}

		#endregion
	}
}
