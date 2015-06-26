using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Web.Requests.Processors;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests.Processors
{
	[TestFixture]
	public class NotFoundRequestProcessorTestFixture
	{
		#region Tests

		[Test]
		[Description("Processes a request for the not found error content.")]
		public void Process_Success()
		{
			// Arrange
			var processor = new NotFoundRequestProcessor();

			// Act
			var result = processor.Process(null);

			// Assert
			Assert.That(result, Is.Not.Null, "A valid ResponseState should be returned.");
			Assert.That(result.StatusCode, Is.EqualTo(HttpStatusCode.NotFound), "The HTTP NotFound status code should be returned.");
		}

		#endregion
	}
}
