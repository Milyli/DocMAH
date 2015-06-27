using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;

namespace DocMAH.UnitTests
{
	[TestFixture]
	public class BaseTestFixture
	{
		#region Protected Properties

		protected MockRepository Mocks { get; set; }
		protected ModelFactory Models { get; set; }		

		#endregion

		#region SetUp / TearDown
		
		[SetUp]
		public void BaseSetUp()
		{
			Mocks = new MockRepository(MockBehavior.Strict);
			Models = new ModelFactory();
		}

		[TearDown]
		public void BaseTearDown()
		{
			Mocks = null;
			Models = null;
		}

		#endregion
		
	}
}
