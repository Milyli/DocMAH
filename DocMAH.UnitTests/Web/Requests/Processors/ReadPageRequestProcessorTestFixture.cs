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
	public class ReadPageRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Reads a page and its bullets from the data store.")]
		public void Process_Success()
		{
			// Arrange
			var page = Models.CreatePage(id: 84932);
			var bullet = Models.CreateBullet(id: 98342, pageId: page.Id);

			var pageRepository = Mocks.Create<IPageRepository>();
			pageRepository.Setup(r => r.Read(page.Id)).Returns(page);

			var bulletRepository = Mocks.Create<IBulletRepository>();
			bulletRepository.Setup(r => r.ReadByPageId(page.Id)).Returns(new List<Bullet> { bullet });

			var processor = new ReadPageRequestProcessor(bulletRepository.Object, pageRepository.Object);

			// Act
			var result = processor.Process(page.Id.ToString());

			// Assert
			Assert.That(result, Is.Not.Null, "A valid ResponseState should be returned.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The request should succeed.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "JSON should be returned in the response.");
			var serializer = new JavaScriptSerializer();
			var clientPage = serializer.Deserialize<Page>(result.Content);
			Assert.That(clientPage.Id, Is.EqualTo(page.Id), "The page read from the repository should be included in the result.");
			Assert.That(clientPage.Bullets[0].Id, Is.EqualTo(bullet.Id), "The bullets read from the repository should be included in the result.");
		}

		#endregion
	}
}
