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
	public class MovePageRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Moves the page to a higher order in the same parent item.")]
		public void Process_MoveHigherInSameParent()
		{
			// Arrange
			var parentPage = Models.CreatePage(id: 83472);
			var firstSibling = Models.CreatePage(id: 89231, parentPageId: parentPage.Id, order: 0);
			var targetPage = Models.CreatePage(id: 43294, parentPageId: parentPage.Id, order: 1);
			var secondSibling = Models.CreatePage(id: 1428, parentPageId: parentPage.Id, order: 2);
			var thirdSibling = Models.CreatePage(id: 65473, parentPageId: parentPage.Id, order: 3);
			var fourthSibling = Models.CreatePage(id: 33242, parentPageId: parentPage.Id, order: 4);
			var siblings = new List<Page> { firstSibling, targetPage, secondSibling, thirdSibling, fourthSibling };

			var pageRepository = Mocks.Create<IPageRepository>();
			pageRepository.Setup(r => r.Read(targetPage.Id)).Returns(targetPage);
			pageRepository.Setup(r => r.ReadByParentId(parentPage.Id)).Returns(siblings);
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == secondSibling.Id && p.ParentPageId == parentPage.Id && p.Order == 1)));
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == thirdSibling.Id && p.ParentPageId == parentPage.Id && p.Order == 2)));
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == targetPage.Id && p.ParentPageId == parentPage.Id && p.Order == 3)));

			var serializer = new JavaScriptSerializer();
			var moveRequest = new MoveTocRequest { PageId = targetPage.Id, NewParentId = parentPage.Id, NewPosition = 3 };
			var requestData = serializer.Serialize(moveRequest);

			var processor = new MovePageRequestProcessor(pageRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result, Is.Not.Null, "A valid response state instance should be returned.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The result status code should be OK.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Html), "The result content type should be HTML.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Moves the page to a lower order in the same parent item.")]
		public void Process_MoveLowerInSameParent()
		{
			// Arrange
			var parentPage = Models.CreatePage(id: 37453);
			var firstSibling = Models.CreatePage(id: 87623, parentPageId: parentPage.Id, order: 0);
			var secondSibling = Models.CreatePage(id: 12387, parentPageId: parentPage.Id, order: 1);
			var thirdSibling = Models.CreatePage(id: 54356, parentPageId: parentPage.Id, order: 2);
			var targetPage = Models.CreatePage(id: 76527, parentPageId: parentPage.Id, order: 3);
			var fourthSibling = Models.CreatePage(id: 33452, parentPageId: parentPage.Id, order: 4);
			var siblings = new List<Page> { firstSibling, secondSibling, thirdSibling, targetPage, fourthSibling };

			var pageRepository = Mocks.Create<IPageRepository>();
			pageRepository.Setup(r => r.Read(targetPage.Id)).Returns(targetPage);
			pageRepository.Setup(r => r.ReadByParentId(parentPage.Id)).Returns(siblings);
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == targetPage.Id && p.ParentPageId == parentPage.Id && p.Order == 1)));
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == secondSibling.Id && p.ParentPageId == parentPage.Id && p.Order == 2)));
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == thirdSibling.Id && p.ParentPageId == parentPage.Id && p.Order == 3)));

			var serializer = new JavaScriptSerializer();
			var moveRequest = new MoveTocRequest { PageId = targetPage.Id, NewParentId = parentPage.Id, NewPosition = 1 };
			var requestData = serializer.Serialize(moveRequest);

			var processor = new MovePageRequestProcessor(pageRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result, Is.Not.Null, "A valid response state instance should be returned.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The result status code should be OK.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Html), "The result content type should be HTML.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Moves a page to a new parent.")]
		public void Process_MoveToNewParent()
		{
			// Arrange
			var oldParentPage = Models.CreatePage(id: 46382);
			var newParentPage = Models.CreatePage(id: 29556);

			var oldLowerSibling = Models.CreatePage(id: 98732, parentPageId: oldParentPage.Id, order: 0);
			var targetPage = Models.CreatePage(id: 43900, parentPageId: oldParentPage.Id, order: 1);
			var oldHigherSibling = Models.CreatePage(id: 43729, parentPageId: oldParentPage.Id, order: 2);
			var oldSiblings = new List<Page> { oldLowerSibling, targetPage, oldHigherSibling };

			var newLowerSibling = Models.CreatePage(id: 12943, parentPageId: newParentPage.Id, order: 0);
			var newHigherSibling = Models.CreatePage(id: 84539, parentPageId: newParentPage.Id, order: 1);
			var newSiblings = new List<Page> { newLowerSibling, newHigherSibling };

			var pageRepository = Mocks.Create<IPageRepository>();
			pageRepository.Setup(r => r.Read(targetPage.Id)).Returns(targetPage);
			pageRepository.Setup(r => r.ReadByParentId(oldParentPage.Id)).Returns(oldSiblings);
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == oldHigherSibling.Id && p.Order == 1)));
			pageRepository.Setup(r => r.ReadByParentId(newParentPage.Id)).Returns(newSiblings);
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == targetPage.Id && p.ParentPageId == newParentPage.Id && p.Order == 1)));
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == newHigherSibling.Id && p.Order == 2)));

			var serializer = new JavaScriptSerializer();
			var moveRequest = new MoveTocRequest { PageId = targetPage.Id, NewParentId = newParentPage.Id, NewPosition = 1 };
			var requestData = serializer.Serialize(moveRequest);

			var processor = new MovePageRequestProcessor(pageRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result, Is.Not.Null, "A valid response state instance should be returned.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The result status code should be OK.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Html), "The result content type should be HTML.");
			Mocks.VerifyAll();
		}

		#endregion
	}
}
