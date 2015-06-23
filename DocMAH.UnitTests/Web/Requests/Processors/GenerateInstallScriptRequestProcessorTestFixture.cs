using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Adapters;
using DocMAH.Content;
using DocMAH.Web.Requests;
using DocMAH.Web.Requests.Processors;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	[TestFixture]
	public class GenerateInstallScriptRequestProcessorTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Generates an install script.")]
		public void Process_Success()
		{
			// Arrange
			var tempPath = TestContext.CurrentContext.TestDirectory;
			var tempFile = Path.Combine(tempPath, ContentFileConstants.ContentFileName);

			var path = Mocks.Create<IPath>();
			path.Setup(p => p.MapPath("~")).Returns(tempPath);

			var helpContentManager = Mocks.Create<IHelpContentManager>();
			helpContentManager.Setup(m => m.ExportContent(tempFile));

			var testFileContent = "Test File Content";
			File.WriteAllText(tempFile, testFileContent);

			var processor = new GenerateInstallScriptRequestProcessor(path.Object, helpContentManager.Object);

			// Act
			var result = processor.Process(null);

			// Assert
			Assert.That(result, Is.Not.Null, "A valid response state should be returned.");
			Assert.That(result.Content, Is.EqualTo(testFileContent), "The response should contain the text of the content file.");
			Assert.That(result.ContentType, Is.EqualTo(ContentTypes.Text), "The content type is plain text.");
			Assert.That(result.Disposition.Contains(string.Format("filename={0}", ContentFileConstants.ContentFileName)), Is.True, "The response disposition should be set correctly.");

			if (File.Exists(tempFile))
				File.Delete(tempFile);
		}

		#endregion
	}
}
