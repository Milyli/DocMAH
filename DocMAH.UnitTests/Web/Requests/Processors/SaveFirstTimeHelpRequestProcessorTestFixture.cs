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
	public class SaveFirstTimeHelpRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests
		
		[Test]
		[Description("saves a help page and returns the updated model.")]
		public void Process_ExistingPage()
		{
			// Arrange
			var dataStorePage = Models.CreateFirstTimeHelp(id: 84926);

			var dataStoreBullets = new List<Bullet>();
			dataStoreBullets.Add(Models.CreateBullet(id: 54829, pageId: dataStorePage.Id));
			dataStoreBullets.Add(Models.CreateBullet(id: 29334, pageId: dataStorePage.Id));
			var updatedBullet = dataStoreBullets[0];
			var deletedBullet = dataStoreBullets[1];

			var clientHelp = new FirstTimeHelp
			{
				Id = dataStorePage.Id,			// Same id as data store page because this is an existing page.
				Title = dataStorePage.Title,
				Content = dataStorePage.Content,
			};
			clientHelp.Bullets.Add(Models.CreateBullet(clientHelp.Id));
			clientHelp.Bullets.Add(dataStoreBullets[0]);
			var newBullet = clientHelp.Bullets[0];

			var helpRepository = Mocks.Create<IFirstTimeHelpRepository>();
			helpRepository.Setup(r => r.Update(It.Is<FirstTimeHelp>(p => p.Id == clientHelp.Id)));

			var bulletRepository = Mocks.Create<IBulletRepository>();
			bulletRepository.Setup(r => r.ReadByPageId(clientHelp.Id)).Returns(dataStoreBullets);
			bulletRepository.Setup(r => r.Create(It.Is<Bullet>(b => b.Text == newBullet.Text)));
			bulletRepository.Setup(r => r.Update(It.Is<Bullet>(b => b.Id == updatedBullet.Id)));
			bulletRepository.Setup(r => r.Delete(deletedBullet.Id));

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientHelp);
			var processor = new SaveFirstTimeHelpRequestProcessor(bulletRepository.Object, helpRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result, Is.Not.Null, "A response state instance should be returned.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "The response content should contain JSON.");
			var resultHelp = serializer.Deserialize<FirstTimeHelp>(result.Content);
			Assert.That(resultHelp.Id, Is.EqualTo(clientHelp.Id), "The page id should not change.");
			Assert.That(resultHelp.Bullets.Count, Is.EqualTo(clientHelp.Bullets.Count), "The returned page should have the same number of bullets as the client page.");
			foreach (var clientBullet in clientHelp.Bullets)
			{
				Assert.That(resultHelp.Bullets.Where(resultBullet => resultBullet.Text == clientBullet.Text).Count(), Is.EqualTo(1), "All bullets in client model should be returned in the result.");
			}
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Saves a new page to the data store.")]
		public void Process_NewPage()
		{
			// Arrange
			var clientHelp = Models.CreateFirstTimeHelp();
			var clientBullet = Models.CreateBullet(clientHelp.Id);
			clientHelp.Bullets.Add(clientBullet);

			var helpRepository = Mocks.Create<IFirstTimeHelpRepository>();
			helpRepository.Setup(r => r.Create(It.Is<FirstTimeHelp>(p => p.Title == clientHelp.Title)));

			var bulletRepository = Mocks.Create<IBulletRepository>();
			bulletRepository.Setup(r => r.Create(It.Is<Bullet>(b => b.Text == clientBullet.Text)));

			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(clientHelp);
			var processor = new SaveFirstTimeHelpRequestProcessor(bulletRepository.Object, helpRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result, Is.Not.Null, "Response state instance expected.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "JSON result expected.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Valid response code expected.");
			var resultPage = serializer.Deserialize<FirstTimeHelp>(result.Content);
			Assert.That(resultPage.Title, Is.EqualTo(clientHelp.Title), "The title of the result page should match the client page.");
			Assert.That(resultPage.Bullets.Count, Is.EqualTo(clientHelp.Bullets.Count), "The page returned should have the same number of bullets as the original client page.");
		}

		#endregion
	}
}
