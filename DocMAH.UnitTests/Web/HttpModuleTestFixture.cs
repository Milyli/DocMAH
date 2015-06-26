using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using DocMAH.Content;
using DocMAH.Data;
using DocMAH.Dependencies;
using DocMAH.Web;
using DocMAH.Web.Authorization;
using DocMAH.Web.Requests;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web
{
	[TestFixture]
	public class HttpModuleTestFixture : BaseTestFixture
	{
		#region Private Fields

		private const string MvcAssemblyName = "System.Web.Mvc";

		#endregion

		#region Tests

		[Test]
		[Description("Skips attaching filter if already attached.")]
		public void AttachFilterEventHAndler_AlreadyAttached()
		{
			// Arrange
			var context = Mocks.Create<HttpContextBase>();
			context.Setup(c => c.Items.Contains(HttpModule.ContainerKey)).Returns(true);

			var module = new HttpModule(context.Object, null, null, null);

			// Act
			module.AttachFilterEventHandler(null, null);

			// Assert
			Mocks.VerifyAll();
		}

		[TestCase(MvcAssemblyName, HttpModule.MvcHandlerName)]
		[Description("Attaches the response filter for valid application requests.")]
		public void AttachFilterEventHandler_AttachToCompatibleHandler(string handlerAssemblyName, string handlerTypeName)
		{
			// Arrange
			var container = Mocks.Create<IContainer>();
			container.Setup(c => c.Resolve<IBulletRepository>()).Returns(Mocks.Create<IBulletRepository>().Object);
			container.Setup(c => c.Resolve<IEditAuthorizer>()).Returns(Mocks.Create<IEditAuthorizer>().Object);
			container.Setup(c => c.Resolve<IMinifier>()).Returns(Mocks.Create<IMinifier>().Object);
			container.Setup(c => c.Resolve<IPageRepository>()).Returns(Mocks.Create<IPageRepository>().Object);
			container.Setup(c => c.Resolve<IUserPageSettingsRepository>()).Returns(Mocks.Create<IUserPageSettingsRepository>().Object);

			var response = Mocks.Create<HttpResponseBase>();
			response.Setup(r => r.ContentType).Returns(ContentTypes.Html);
			response.Setup(r => r.Filter).Returns(new MemoryStream());
			response.SetupSet(r => r.Filter = It.IsAny<HttpResponseFilter>());

			var handlerAssembly = Assembly.Load(handlerAssemblyName);
			var handlerType = handlerAssembly.GetType(handlerTypeName);
			var handler = (IHttpHandler)Activator.CreateInstance(handlerType, new RequestContext());

			var context = Mocks.Create<HttpContextBase>();
			context.Setup(c => c.Items.Contains(HttpModule.ContainerKey)).Returns(false);
			context.Setup(c => c.Items.Add(HttpModule.ContainerKey, true));
			context.Setup(c => c.CurrentHandler).Returns(handler);
			context.Setup(c => c.Response).Returns(response.Object);

			var module = new HttpModule(context.Object, container.Object, null, null);

			// Act
			module.AttachFilterEventHandler(null, null);

			// Assert
			Mocks.VerifyAll();
		}
		
		#endregion
	}
}
