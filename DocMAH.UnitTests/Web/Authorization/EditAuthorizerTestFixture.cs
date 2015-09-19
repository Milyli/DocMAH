using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Configuration;
using DocMAH.Web.Authorization;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Authorization
{
	[TestFixture]
	public class EditAuthorizerTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Authorization fails when editing is diabled.")]
		public void Authorize_Disabled()
		{
			// Arrange
			var httpRequest = Mocks.Create<HttpRequestBase>();

			var httpContext = Mocks.Create<HttpContextBase>();
			httpContext.Setup(c => c.Request).Returns(httpRequest.Object);

			Configuration.EditHelpConfiguration.Setup(c => c.IsDisabled).Returns(true);

			var authorizer = new EditAuthorizer(httpContext.Object, Configuration.Object);

			// Act
			var result = authorizer.Authorize();

			// Assert
			Assert.That(result, Is.False, "Authorization should fail because editing is disabled.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Authorization fails when authentication is required but user is not authenticated.")]
		public void Authorize_NotAuthenticated()
		{
			// Arrange
			var httpRequest = Mocks.Create<HttpRequestBase>();
			httpRequest.Setup(r => r.IsAuthenticated).Returns(false);

			var httpContext = Mocks.Create<HttpContextBase>();
			httpContext.Setup(c => c.Request).Returns(httpRequest.Object);

			Configuration.EditHelpConfiguration.Setup(c => c.IsDisabled).Returns(false);
			Configuration.EditHelpConfiguration.Setup(c => c.RequireAuthentication).Returns(true);

			var authorizer = new EditAuthorizer(httpContext.Object, Configuration.Object);

			// Act
			var result = authorizer.Authorize();

			// Assert
			Assert.That(result, Is.False, "Authorization should fail because request is not authorized.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Authorization fails when local requests are required but request is not local.")]
		public void Authorize_NotLocal()
		{
			// Arrange
			var httpRequest = Mocks.Create<HttpRequestBase>();
			httpRequest.Setup(r => r.IsLocal).Returns(false);

			var httpContext = Mocks.Create<HttpContextBase>();
			httpContext.Setup(c => c.Request).Returns(httpRequest.Object);

			Configuration.EditHelpConfiguration.Setup(c => c.IsDisabled).Returns(false);
			Configuration.EditHelpConfiguration.Setup(c => c.RequireAuthentication).Returns(false);
			Configuration.EditHelpConfiguration.Setup(c => c.RequireLocalConnection).Returns(true);

			var authorizer = new EditAuthorizer(httpContext.Object, Configuration.Object);

			// Act
			var result = authorizer.Authorize();

			// Assert
			Assert.That(result, Is.False, "Authorization should fail because request is not local.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("User has edit authorization.")]
		public void Authorize_Success()
		{
			// Arrange
			var httpRequest = Mocks.Create<HttpRequestBase>();

			var httpContext = Mocks.Create<HttpContextBase>();
			httpContext.Setup(c => c.Request).Returns(httpRequest.Object);

			Configuration.EditHelpConfiguration.Setup(c => c.IsDisabled).Returns(false);
			Configuration.EditHelpConfiguration.Setup(c => c.RequireAuthentication).Returns(false);
			Configuration.EditHelpConfiguration.Setup(c => c.RequireLocalConnection).Returns(false);

			var authorizer = new EditAuthorizer(httpContext.Object, Configuration.Object);

			// Act
			var result = authorizer.Authorize();

			// Assert
			Assert.That(result, Is.True, "Authorization should succeed.");
			Mocks.VerifyAll();
		}

		#endregion
	}
}
