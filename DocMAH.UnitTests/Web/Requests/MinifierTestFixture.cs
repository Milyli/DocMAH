using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using DocMAH.Adapters;
using DocMAH.Web.Requests;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests
{
	[TestFixture]
	public class MinifierTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Does not minify content when the debugger is attached.")]
		public void Minify_DebuggerAttached()
		{
			// Arrange
			var debugger = Mocks.Create<IDebugger>();
			debugger.Setup(d => d.IsAttached).Returns(true);
			
			var fullContent = "full";
			var minifiedContent = "minified";

			var minifier = new Minifier(debugger.Object, null);

			// Act
			var result = minifier.Minify(fullContent, minifiedContent);

			// Assert
			Assert.That(result, Is.EqualTo(fullContent), "The content should not be minified because the debugger is attached.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Does not minify content when debugging enabled in web.config.")]
		public void Minify_DebuggingEnabled()
		{
			// Arrange
			var debugger = Mocks.Create<IDebugger>();
			debugger.Setup(d => d.IsAttached).Returns(false);

			var httpContext = Mocks.Create<HttpContextBase>();
			httpContext.Setup(c => c.IsDebuggingEnabled).Returns(true);
			
			var fullContent = "full";
			var minifiedContent = "minified";

			var minifier = new Minifier(debugger.Object, httpContext.Object);

			// Act
			var result = minifier.Minify(fullContent, minifiedContent);

			// Assert
			Assert.That(result, Is.EqualTo(fullContent), "The content should not be minified because debugging is configured in web.config.");
			Mocks.VerifyAll();
		}

		[Test]
		[Description("Minifies the content.")]
		public void Minify_Minified()
		{
			// Arrange
			var debugger = Mocks.Create<IDebugger>();
			debugger.Setup(d => d.IsAttached).Returns(false);

			var httpContext = Mocks.Create<HttpContextBase>();
			httpContext.Setup(c => c.IsDebuggingEnabled).Returns(false);

			var fullContent = "full";
			var minifiedContent = "minified";

			var minifier = new Minifier(debugger.Object, httpContext.Object);

			// Act
			var result = minifier.Minify(fullContent, minifiedContent);

			// Assert
			Assert.That(result, Is.EqualTo(minifiedContent), "The content should be minified.");
			Mocks.VerifyAll();
		}

		#endregion
	}
}
