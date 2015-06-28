using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Data;
using DocMAH.Models;
using DocMAH.Web.Requests.Processors;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	
	[TestFixture]
	public class DeletePageRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Processes a delete page request successfully.")]
		public void Process_Success()
		{
			// Arrange
			var page = Models.CreateDocumentationPage(id: 66387, order: 1, parentPageId: 23198);
			var sibling0 = Models.CreateDocumentationPage(id: 98231, order: 0, parentPageId: page.ParentPageId);
			var sibling2 = Models.CreateDocumentationPage(id: 66123, order: 2, parentPageId: page.ParentPageId);
			var child0 = Models.CreateDocumentationPage(id: 4392, order: 0, parentPageId: page.Id);
			var child1 = Models.CreateDocumentationPage(id: 9342, order: 1, parentPageId: page.Id);

			var siblings = new List<DocumentationPage> { sibling0, page, sibling2 }; 
			var children = new List<DocumentationPage> { child0, child1 };

			var pageRepository = Mocks.Create<IDocumentationPageRepository>();
			pageRepository.Setup(r => r.Read(page.Id)).Returns(page);
			pageRepository.Setup(r => r.ReadByParentId(page.ParentPageId)).Returns(siblings); // read to update orders.
			pageRepository.Setup(r => r.ReadByParentId(page.Id)).Returns(children);	// read to update orders and parent id.
			pageRepository.Setup(r => r.Update(It.Is<DocumentationPage>(p => p.Id == child0.Id && p.ParentPageId == page.ParentPageId && p.Order == 1)));
			pageRepository.Setup(r => r.Update(It.Is<DocumentationPage>(p => p.Id == child1.Id && p.ParentPageId == page.ParentPageId && p.Order == 2)));
			pageRepository.Setup(r => r.Update(It.Is<DocumentationPage>(p => p.Id == sibling2.Id && p.ParentPageId == page.ParentPageId && p.Order == 3)));
			pageRepository.Setup(r => r.Delete(page.Id));

			var bulletRepository = Mocks.Create<IBulletRepository>();
			bulletRepository.Setup(r => r.DeleteByPageId(page.Id));

			var userPageSettingsRepository = Mocks.Create<IUserPageSettingsRepository>();
			userPageSettingsRepository.Setup(r => r.DeleteByPageId(page.Id));

			var processor = new DeletePageRequestProcessor(bulletRepository.Object, pageRepository.Object, userPageSettingsRepository.Object);

			// Act
			var result = processor.Process(page.Id.ToString());

			// Assert
			Assert.That(result, Is.Not.Null, "A Valid ResponseState should be returned.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The request should return a valid HTTP status.");
		}

		#endregion
	}
}
