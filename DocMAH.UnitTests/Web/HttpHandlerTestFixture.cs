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
		#region Private Fields

		private const string MethodName = "TestMethodName";
		private const string ItemId = "25";

		#endregion

		#region Protected Properties

		protected Mock<IContainer> Container;
		protected Mock<IEditAuthorizer> EditAuthorizer;
		protected Mock<HttpContextBase> HttpContext;
		protected Mock<HttpResponseBase> Response;
		protected Mock<IRequestProcessorFactory> RequestProcessorFactory;

		#endregion

		#region SetUp / TearDown

		[SetUp]
		public void SetUp()
		{
			RequestProcessorFactory = Mocks.Create<IRequestProcessorFactory>();
			EditAuthorizer = Mocks.Create<IEditAuthorizer>();
			Container = Mocks.Create<IContainer>();
			Container.Setup(c => c.Resolve<IRequestProcessorFactory>()).Returns(RequestProcessorFactory.Object);
			Container.Setup(c => c.Resolve<IEditAuthorizer>()).Returns(EditAuthorizer.Object);

			// The response writing code would be too cumbersome to setup for every test.
			// We'll stub this one out and verify in a dedicated test.
			Response = new Mock<HttpResponseBase>();
			Response.Setup(r => r.Cache).Returns(new Mock<HttpCachePolicyBase>().Object);

			// Likewise, a lot goes on under the hood with the request that is not worth verifying.
			HttpContext = new Mock<HttpContextBase>();
			HttpContext.Setup(c => c.Request.InputStream).Returns(new MemoryStream(Encoding.UTF8.GetBytes(string.Empty)));
			HttpContext.Setup(c => c.Request["m"]).Returns(MethodName);
			HttpContext.Setup(c => c.Request["id"]).Returns(ItemId);
			HttpContext.Setup(c => c.Response).Returns(Response.Object);
		}

		#endregion
		
		#region Tests

		[Test]
		[Description("Authorization fails on required edit authorization.")]
		public void ProcessRequestInternal_EditAuthorizationFails()
		{
			// Arrange
			var processor = new EditAuthorizedProcessor();

			var unauthorizedProcessor = Mocks.Create<IRequestProcessor>();
			unauthorizedProcessor.Setup(p => p.Process(ItemId)).Returns(new ResponseState());

			RequestProcessorFactory.Setup(f => f.Create(MethodName)).Returns(processor);
			RequestProcessorFactory.Setup(c => c.Create(RequestTypes.Unauthorized)).Returns(unauthorizedProcessor.Object);

			EditAuthorizer.Setup(a => a.Authorize()).Returns(false);

			var handler = new HttpHandler(Container.Object);

			// Act
			handler.ProcessRequestInternal(HttpContext.Object);

			// Assert
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Successfully processes a request that does not require authorization.")]
		public void ProcessRequestInternal_NoAuthorization()
		{
			// Arrange
			var requestProcessor = Mocks.Create<IRequestProcessor>();
			requestProcessor.Setup(p => p.Process(ItemId)).Returns(new ResponseState());

			RequestProcessorFactory.Setup(f => f.Create(MethodName)).Returns(requestProcessor.Object);

			var handler = new HttpHandler(Container.Object);

			// Act
			handler.ProcessRequestInternal(HttpContext.Object);

			// Assert
			Mocks.VerifyAll();
		}

		#endregion

		[EditAuthorization]
		private class EditAuthorizedProcessor : IRequestProcessor
		{
			#region IRequestProcessor Members

			public ResponseState Process(string data)
			{
				return new ResponseState()
				{
					Content = "Success",
					ContentType = ContentTypes.Html,					 
				};
			}

			#endregion
		}

	}
}
