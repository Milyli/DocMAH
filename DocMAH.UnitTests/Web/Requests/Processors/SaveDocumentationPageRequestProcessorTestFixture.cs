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
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	[TestFixture]
	public class SaveDocumentationPageRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Attempts to change the order of the page and save it.")]
		[ExpectedException(typeof(InvalidOperationException), MatchType = MessageMatch.Contains, ExpectedMessage = "Changing page order")]
		public void Process_ChangeOrder()
		{
			// Arrange
			var dataStorePage = Models.CreateDocumentationPage(id: 75326, parentPageId: 12943, order: 5);
			var clientPage = Models.CreateDocumentationPage(id: dataStorePage.Id, parentPageId: dataStorePage.ParentPageId, order: 4);

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientPage);

			var documentationPageRepository = Mocks.Create<IDocumentationPageRepository>();
			documentationPageRepository.Setup(r => r.Read(clientPage.Id)).Returns(dataStorePage);

			var processor = new SaveDocumentationPageRequestProcessor(documentationPageRepository.Object);

			// Act
			processor.Process(requestData);

			// Assert
			// InvalidOperationException should be thrown.
		}

		[Test]
		[Description("Attempts to change the parent page of the page and save it.")]
		[ExpectedException(typeof(InvalidOperationException), MatchType = MessageMatch.Contains, ExpectedMessage = "Changing page order")]
		public void Process_ChangeParentPage()
		{
			// Arrange
			var dataStorePage = Models.CreateDocumentationPage(id: 75326, parentPageId: 12943, order: 5);
			var clientPage = Models.CreateDocumentationPage(id: dataStorePage.Id, parentPageId: 543, order: dataStorePage.Order);

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientPage);
			
			var documentationPageRepository = Mocks.Create<IDocumentationPageRepository>();
			documentationPageRepository.Setup(r => r.Read(clientPage.Id)).Returns(dataStorePage);

			var processor = new SaveDocumentationPageRequestProcessor(documentationPageRepository.Object);

			// Act
			processor.Process(requestData);

			// Assert
			// InvalidOperationException should be thrown.
		}

		[Test]
		[Description("saves a help page and returns the updated model.")]
		public void Process_ExistingPage()
		{
			// Arrange
			var dataStorePage = Models.CreateDocumentationPage(id: 84926);

			var clientPage = new DocumentationPage
			{
				Id = dataStorePage.Id,			// Same id as data store page because this is an existing page.
				Title = dataStorePage.Title,
				Content = dataStorePage.Content,
				Order = dataStorePage.Order,
				ParentPageId = dataStorePage.ParentPageId,
			};

			var documentationPageRepository = Mocks.Create<IDocumentationPageRepository>();
			documentationPageRepository.Setup(r => r.Read(clientPage.Id)).Returns(dataStorePage);
			documentationPageRepository.Setup(r => r.Update(It.Is<DocumentationPage>(p => p.Id == clientPage.Id)));

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientPage);
			var processor = new SaveDocumentationPageRequestProcessor(documentationPageRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result, Is.Not.Null, "A response state instance should be returned.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "The response content should contain JSON.");
			var resultPage = serializer.Deserialize<DocumentationPage>(result.Content);
			Assert.That(resultPage.Id, Is.EqualTo(clientPage.Id), "The page id should not change.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Saves a new page to the data store.")]
		public void Process_NewPage()
		{
			// Arrange
			var parentPage = Models.CreateDocumentationPage(id: 74362);
			var lowerSiblingPage = Models.CreateDocumentationPage(id: 63254, parentPageId: parentPage.Id, order: 0);
			var higherSiblingPage = Models.CreateDocumentationPage(id: 1342, parentPageId: parentPage.Id, order: 1);
			var highestSiblingPage = Models.CreateDocumentationPage(id: 8724, parentPageId: parentPage.Id, order: 2);
			var siblingPages = new List<DocumentationPage> { lowerSiblingPage, higherSiblingPage, highestSiblingPage };

			var clientPage = Models.CreateDocumentationPage(parentPageId: parentPage.Id, order: 1);
			
			var documentationPageRepository = Mocks.Create<IDocumentationPageRepository>();
			documentationPageRepository.Setup(r => r.ReadByParentId(clientPage.ParentPageId)).Returns(siblingPages);
			documentationPageRepository.Setup(r => r.Update(It.Is<DocumentationPage>(p => p.Id == higherSiblingPage.Id && p.Order == 2)));
			documentationPageRepository.Setup(r => r.Update(It.Is<DocumentationPage>(p => p.Id == highestSiblingPage.Id && p.Order == 3)));
			documentationPageRepository.Setup(r => r.Create(It.Is<DocumentationPage>(p => p.Title == clientPage.Title)));

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientPage);
			var processor = new SaveDocumentationPageRequestProcessor(documentationPageRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Mocks.VerifyAll();
			Assert.That(result, Is.Not.Null, "Response state instance expected.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "JSON result expected.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Valid response code expected.");
			var resultPage = serializer.Deserialize<DocumentationPage>(result.Content);
			Assert.That(resultPage.Title, Is.EqualTo(clientPage.Title), "The title of the result page should match the client page.");
		}

		#endregion
	}
}
