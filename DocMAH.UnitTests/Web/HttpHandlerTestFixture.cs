using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Dependencies;
using DocMAH.Web;
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web
{
	[TestFixture]
	public class HttpHandlerTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Successfully processes a request that requires authorization.")]
		public void ProcessWrappedRequest_AuthorizedSuccess()
		{
			// Arrange
			var methodName = "TestMethodName";
			var itemId = "25";
			var responseDisposition = "Test Response Disposition";
			var responseContent = "Test response content";

			var editAuthorizer = Mocks.Create<IEditAuthorizer>();
			editAuthorizer.Setup(a => a.Authorize()).Returns(true);

			var requestProcessor = Mocks.Create<IRequestProcessor>();
			requestProcessor.Setup(p => p.RequiresEditAuthorization).Returns(true);
			var responseState = new ResponseState
										{
											Disposition = responseDisposition,
											Content = responseContent,
										};
			requestProcessor.Setup(p => p.Process(itemId)).Returns(responseState);

			var requestProcessorFactory = Mocks.Create<IRequestProcessorFactory>();
			requestProcessorFactory.Setup(f => f.Create(methodName)).Returns(requestProcessor.Object);

			var container = Mocks.Create<IContainer>();			
			container.Setup(c => c.ResolveInstance<IRequestProcessorFactory>()).Returns(requestProcessorFactory.Object);
			container.Setup(c => c.ResolveInstance<IEditAuthorizer>()).Returns(editAuthorizer.Object);

			var handler = new HttpHandler(container.Object);

			// The response writing code would be too cumbersome to setup for every test.
			// We'll stub this one out and verify in a dedicated test.
			var response = new Mock<HttpResponseBase>();
			response.Setup(r => r.Cache).Returns(new Mock<HttpCachePolicyBase>().Object);

			// Likewise, a lot goes on under the hood with the request that is not worth verifying.
			var httpContext = new Mock<HttpContextBase>();
			httpContext.Setup(c => c.Request.InputStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(string.Empty)));
			httpContext.Setup(c => c.Request["m"]).Returns(methodName);
			httpContext.Setup(c => c.Request["id"]).Returns(itemId);
			httpContext.Setup(c => c.Response).Returns(response.Object);

			// Act
			handler.ProcessWrappedRequest(httpContext.Object);

			// Assert
			Mocks.VerifyAll();
		}

		#endregion
	}
}
