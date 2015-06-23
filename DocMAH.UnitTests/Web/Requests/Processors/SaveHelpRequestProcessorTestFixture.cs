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
	public class SaveHelpRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Attempts to change the order of the page and save it.")]
		[ExpectedException(typeof(InvalidOperationException), MatchType = MessageMatch.Contains, ExpectedMessage = "Changing page order")]
		public void Process_ChangeOrder()
		{
			// Arrange
			var dataStorePage = Models.CreatePage(id: 75326, parentPageId: 12943, order: 5);
			var clientPage = Models.CreatePage(id: dataStorePage.Id, parentPageId: dataStorePage.ParentPageId, order: 4);

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientPage);

			var pageRepository = Mocks.Create<IPageRepository>();
			pageRepository.Setup(r => r.Read(clientPage.Id)).Returns(dataStorePage);

			var processor = new SaveHelpRequestProcessor(null, pageRepository.Object);

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
			var dataStorePage = Models.CreatePage(id: 75326, parentPageId: 12943, order: 5);
			var clientPage = Models.CreatePage(id: dataStorePage.Id, parentPageId: 543, order: dataStorePage.Order);

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientPage);

			var pageRepository = Mocks.Create<IPageRepository>();
			pageRepository.Setup(r => r.Read(clientPage.Id)).Returns(dataStorePage);

			var processor = new SaveHelpRequestProcessor(null, pageRepository.Object);

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
			var dataStorePage = Models.CreatePage(id: 84926);

			var dataStoreBullets = new List<Bullet>();
			dataStoreBullets.Add(Models.CreateBullet(id: 54829, pageId: dataStorePage.Id));
			dataStoreBullets.Add(Models.CreateBullet(id: 29334, pageId: dataStorePage.Id));
			var updatedBullet = dataStoreBullets[0];
			var deletedBullet = dataStoreBullets[1];

			var clientPage = new Page
			{
				Id = dataStorePage.Id,			// Same id as data store page because this is an existing page.
				Title = dataStorePage.Title,
				Content = dataStorePage.Content,
				Order = dataStorePage.Order,
				ParentPageId = dataStorePage.ParentPageId,
			};
			clientPage.Bullets.Add(Models.CreateBullet(clientPage.Id));
			clientPage.Bullets.Add(dataStoreBullets[0]);
			var newBullet = clientPage.Bullets[0];

			var pageRepository = Mocks.Create<IPageRepository>();
			pageRepository.Setup(r => r.Read(clientPage.Id)).Returns(dataStorePage);
			pageRepository.Setup(r => r.Update(It.Is<Page>(p => p.Id == clientPage.Id)));

			var bulletRepository = Mocks.Create<IBulletRepository>();
			bulletRepository.Setup(r => r.ReadByPageId(clientPage.Id)).Returns(dataStoreBullets);
			bulletRepository.Setup(r => r.Create(It.Is<Bullet>(b => b.Text == newBullet.Text)));
			bulletRepository.Setup(r => r.Update(It.Is<Bullet>(b => b.Id == updatedBullet.Id)));
			bulletRepository.Setup(r => r.Delete(deletedBullet.Id));

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientPage);
			var processor = new SaveHelpRequestProcessor(bulletRepository.Object, pageRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result, Is.Not.Null, "A response state instance should be returned.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "The response content should contain JSON.");
			var resultPage = serializer.Deserialize<Page>(result.Content);
			Assert.That(resultPage.Id, Is.EqualTo(clientPage.Id), "The page id should not change.");
			Assert.That(resultPage.Bullets.Count, Is.EqualTo(clientPage.Bullets.Count), "The returned page should have the same number of bullets as the client page.");
			foreach (var clientBullet in clientPage.Bullets)
			{
				Assert.That(resultPage.Bullets.Where(resultBullet => resultBullet.Text == clientBullet.Text).Count(), Is.EqualTo(1), "All bullets in client model should be returned in the result.");
			}
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Saves a new page to the data store.")]
		public void Process_NewPage()
		{
			// Arrange
			var parentPage = Models.CreatePage(id: 74362);
			var lowerSiblingPage = Models.CreatePage(id: 63254, parentPageId: parentPage.Id, order: 1);
			var higherSiblingPage = Models.CreatePage(id: 1342, parentPageId: parentPage.Id, order: 2);
			var highestSiblingPage = Models.CreatePage(id: 8724, parentPageId: parentPage.Id, order: 3);
			var siblingPages = new List<Page> { lowerSiblingPage, higherSiblingPage, highestSiblingPage };

			var clientPage = Models.CreatePage(parentPageId: parentPage.Id, order: 2);
			var clientBullet = Models.CreateBullet(clientPage.Id);
			clientPage.Bullets.Add(clientBullet);

			var pageRepository = Mocks.Create<IPageRepository>();
			pageRepository.Setup(r => r.ReadByParentId(clientPage.ParentPageId)).Returns(siblingPages);
			pageRepository.Setup(r => r.Update(higherSiblingPage));
			pageRepository.Setup(r => r.Update(highestSiblingPage));
			pageRepository.Setup(r => r.Create(It.Is<Page>(p => p.Title == clientPage.Title)));

			var bulletRepository = Mocks.Create<IBulletRepository>();
			bulletRepository.Setup(r => r.Create(It.Is<Bullet>(b => b.Text == clientBullet.Text)));

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientPage);
			var processor = new SaveHelpRequestProcessor(bulletRepository.Object, pageRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result, Is.Not.Null, "Response state instance expected.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "JSON result expected.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Valid response code expected.");
			var resultPage = serializer.Deserialize<Page>(result.Content);
			Assert.That(resultPage.Title, Is.EqualTo(clientPage.Title), "The title of the result page should match the client page.");
			Assert.That(resultPage.Bullets.Count, Is.EqualTo(clientPage.Bullets.Count), "The page returned should have the same number of bullets as the original client page.");
		}

		#endregion
	}
}
