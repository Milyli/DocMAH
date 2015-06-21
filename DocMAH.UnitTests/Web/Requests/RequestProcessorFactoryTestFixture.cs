using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Dependencies;
using DocMAH.Web.Requests;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web.Requests
{
	[TestFixture]
	public class RequestProcessorFactoryTestFixture : BaseTestFixture
	{
		#region Tests

		[Test]
		[Description("Throws ArgumentNullException if requestType not provided.")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void Create_RequestTypeNull()
		{
			// Arrange
			var factory = new RequestProcessorFactory(null);

			// Act
			factory.Create(string.Empty);

			// Assert
			Assert.Fail("ArgumentNullException should have been thrown.");
		}

		[Test]
		[Description("Creates a processor to handle the requested type.")]
		public void Create_Success()
		{
			// Arrange
			var requestType = "TestRequestType";
			var processor = Mocks.Create<IRequestProcessor>();
			var container = Mocks.Create<IContainer>();
			container.Setup(c => c.ResolveNamedInstance<IRequestProcessor>(requestType)).Returns(processor.Object);
			var factory = new RequestProcessorFactory(container.Object);

			// Act
			var result = factory.Create(requestType);

			// Assert
			Assert.That(result, Is.Not.Null, "A request processor should be returned.");
			Assert.That(result, Is.SameAs(processor.Object), "The request processor returned should be the mock processor.");
			container.VerifyAll();
		}

		#endregion
	}
}
