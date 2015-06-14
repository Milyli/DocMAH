using System;
using NUnit.Framework;
using DocMAH.Extensions;

namespace DocMAH.UnitTests.Extensions
{
	[TestFixture]
	public class StringExtensionTestFixture
	{
		#region Tests

		[Test]
		[Description("Create a nullable SQL string value for an empty value.")]
		public void ToNullableSqlValue_Empty()
		{
			// Arrange
			var testValue = null as string;

			// Act
			var result = testValue.ToNullableSqlValue();

			// Assert
			Assert.AreEqual("NULL", result);
		}

		[Test]
		[Description("Creates a nullable SQL string value for a non-empty value.")]
		public void ToNullableSqlValue_NonEmpty()
		{
			// Arrange
			var testValue = "This test's nullable value.";

			// Act
			var result = testValue.ToNullableSqlValue();

			// Assert
			Assert.AreEqual("'This test''s nullable value.'", result);
		}

		[Test]
		[Description("Creates a SQL string value for an empty value.")]
		public void ToSqlValue_Empty()
		{
			// Arrange
			var testValue = string.Empty;

			// Act
			var result = testValue.ToSqlValue();

			// Assert
			Assert.AreEqual("''", result);
		}

		[Test]
		[Description("Creates a SQL string value for a non-empty value.")]
		public void ToSqlValue_NonEmpty()
		{
			// Arrange
			var testValue = "This test's unit test.";

			// Act
			var result = testValue.ToSqlValue();

			// Assert
			Assert.AreEqual("'This test''s unit test.'", result);
		}

		#endregion
	}
}
