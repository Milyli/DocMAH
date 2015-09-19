using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using DocMAH.Configuration;
using DocMAH.Models;
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;
using DocMAH.Web.Requests.Processors;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	[TestFixture]
	public class ReadApplicationSettingsRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Returns application settings.")]
		public void Process_Success()
		{
			// Arrange
			var isAuthorized = true;

			Configuration.DocumentationConfiguration.Setup(c => c.Disabled).Returns(true);

			var editAuthorizer = Mocks.Create<IEditAuthorizer>();
			editAuthorizer.Setup(a => a.Authorize()).Returns(isAuthorized);

			var processor = new ReadApplicationSettingsRequestProcessor(Configuration.Object, editAuthorizer.Object);

			// Act
			var result = processor.Process(null);

			// Assert
			Assert.That(result, Is.Not.Null, "A valid ResponseState should be returned.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.OK), "The request should succeed.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Json), "JSON should be returned.");

			var serializer = new JavaScriptSerializer();
			var settings = serializer.Deserialize<ApplicationSettings>(result.Content);
			Assert.That(settings.CanEdit, Is.EqualTo(isAuthorized), "The JSON result should contain the edit authorization result.");
			Assert.That(settings.DisableDocumentation, Is.True, "Documentation should be disabled in the JSON result.");

			Mocks.VerifyAll();
		}

		#endregion
	}
}
