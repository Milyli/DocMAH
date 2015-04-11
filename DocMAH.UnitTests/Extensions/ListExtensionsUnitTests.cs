using System;
using System.Collections.Generic;
using NUnit.Framework;
using DocMAH.Extensions;

namespace DocMAH.UnitTests.Extensions
{
	[TestFixture]
	public class ListExtensionsUnitTests
	{
		#region Tests

		[Test]
		[Description("Creates a CSV from a generic list of ints.")]
		public void ToCsv_Success()
		{
			// Arrange
			var list = new List<int> { 1, 2, 3 };

			// Act
			var result = list.ToCsv();

			// Assert
			Assert.AreEqual("1,2,3", result);
		}

		#endregion
	}
}
