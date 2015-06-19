using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DocMAH.UnitTests.Web
{
	[TestFixture]
	public class HttpModuleTestFixture
	{
		#region Tests

		[Test]
		[Description("Skips attaching filter if already attached.")]
		public void AttachFilterEventHAndler_AlreadyAttached()
		{
			// Arrange
			throw new NotImplementedException();	

			// Act


			// Assert


		}

		[Test]
		[Description("Attaches the response filter for ASP.NET application requests.")]
		public void AttachFilterEventHandler_AttachForAspxHandler()
		{
			// Arrange
			throw new NotImplementedException();

			// Act


			// Assert


		}

		[Test]
		[Description("Attaches the response filter for MVC application requests.")]
		public void AttachFilterEventHAndler_AttachForMvcHandler()
		{
			// Arrange
			throw new NotImplementedException();

			// Act


			// Assert


		}

		#endregion
	}
}
