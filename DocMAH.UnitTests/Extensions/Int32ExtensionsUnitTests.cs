using System;
using NUnit.Framework;
using DocMAH.Extensions;

namespace DocMAH.UnitTests.Extensions
{
	[TestFixture]
	public class Int32ExtensionsUnitTests
	{
		#region Tests

		[Test]
		[Description("Returns the SQL string value for a non-null int?.")]
		public void ToNullableSqlValue_NotNull()
		{
			// Arrange
			var testValue = (int?)5;

			// Act
			var result = testValue.ToNullableSqlValue();

			// Assert
			Assert.AreEqual("5", result);
		}

		[Test]
		[Description("Returns the null SQL string value for a null int?")]
		public void ToNullableSqlValue_Null()
		{
			// Arrange
			var testValue = null as int?;

			// Act
			var result = testValue.ToNullableSqlValue();

			// Assert
			Assert.AreEqual("NULL", result);
		}

		#endregion
	}
}
