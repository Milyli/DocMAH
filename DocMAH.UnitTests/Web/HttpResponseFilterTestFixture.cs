using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Web;
using DocMAH.Web.Html;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web
{
	[TestFixture]
	public class HttpResponseFilterTestFixture : BaseTestFixture
	{
		#region Tests
		
		[Test]
		[Description("Inserts CSS link and HTML content into response stream.")]
		public void InsertContent_Success()
		{
			// Arrange
			var helpCssLink = "Test CSS Link";
			var helpHtml = "Test HTML";
			var documentPart1 = "<html><he";
			var documentPart2 = "ad>{0}Hea";
			var documentPart3 = "d Content</head><body>Body Content{1}</b";
			var documentPart4 = "ody></html>";

			var responseStream = new MemoryStream();

			var htmlBuilder = Mocks.Create<IHtmlBuilder>();
			htmlBuilder.Setup(b => b.CreateFirstTimeHelpCssLink()).Returns(helpCssLink);
			htmlBuilder.Setup(b => b.CreateFirstTimeHelpHtml()).Returns(helpHtml);

			var filter = new HttpResponseFilter(responseStream, htmlBuilder.Object);

			// Act
			filter.Write(Encoding.UTF8.GetBytes(documentPart1), 0, documentPart1.Length);
			filter.Write(Encoding.UTF8.GetBytes(documentPart2.Replace("{0}", string.Empty)), 0, documentPart2.Length - 3);
			filter.Write(Encoding.UTF8.GetBytes(documentPart3.Replace("{1}", string.Empty)), 0, documentPart3.Length - 3);
			filter.Write(Encoding.UTF8.GetBytes(documentPart4), 0, documentPart4.Length);
			filter.Flush();

			// Assert
			responseStream.Position = 0;
			var responseReader = new StreamReader(responseStream);
			var response = responseReader.ReadToEnd();
			Assert.That(response, Is.EqualTo(string.Format(documentPart1 + documentPart2 + documentPart3 + documentPart4, helpCssLink, helpHtml)), "The response should have had the correct content inserted at the correct locations.");
		}

		#endregion
	}
}
