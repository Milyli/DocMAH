using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using DocMAH.Data;
using DocMAH.Models;
using DocMAH.Web.Requests.Processors;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	[TestFixture]
	public class SaveUserPageSettingsRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Updates user settings for a page already visited.")]
		public void Process_ExistingPage()
		{
			// Arrange
			var pageId = 102;
			var userName = "testy@docmah.com";

			var requestContract = new UserPageSettings
			{
				HidePage = false,
				PageId = pageId,
				UserName = userName,
			};
			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(requestContract);

			var httpContext = Mocks.Create<HttpContextBase>();
			httpContext.Setup(c => c.User.Identity.Name).Returns(requestContract.UserName);

			var userPageSettingsRepository = Mocks.Create<IUserPageSettingsRepository>();
			userPageSettingsRepository.Setup(r => r.Read(requestContract.UserName, pageId)).Returns(requestContract as UserPageSettings);
			userPageSettingsRepository.Setup(r => r.Update(It.Is<UserPageSettings>(
				s => s.HidePage == requestContract.HidePage
					&& s.PageId == pageId
					&& s.UserName == requestContract.UserName
			)));

			var processor = new SaveUserPageSettingsRequestProcessor(httpContext.Object, userPageSettingsRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The new settings record should be created successfully.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Saves user page settings to the data store.")]
		public void Process_NewPage()
		{
			// Arrange
			var pageId = 100;
			var userName = "tester@docmah.com";

			var requestContract = new UserPageSettings
			{
				HidePage = true,
				PageId = pageId,
				UserName = "garbage",
			};
			var serializer = new JavaScriptSerializer();
			var requestData = serializer.Serialize(requestContract);

			var httpContext = Mocks.Create<HttpContextBase>();
			httpContext.Setup(c => c.User.Identity.Name).Returns(userName);

			var userPageSettingsRepository = Mocks.Create<IUserPageSettingsRepository>();
			userPageSettingsRepository.Setup(r => r.Read(userName, pageId)).Returns(null as UserPageSettings);
			userPageSettingsRepository.Setup(r => r.Create(It.Is<UserPageSettings>(
				s => s.HidePage == requestContract.HidePage
					&& s.PageId == pageId
					&& s.UserName == userName
			)));

			var processor = new SaveUserPageSettingsRequestProcessor(httpContext.Object, userPageSettingsRepository.Object);

			// Act
			var result = processor.Process(requestData);

			// Assert
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The new settings record should be created successfully.");
			Mocks.VerifyAll();

		}

		#endregion
	}
}
