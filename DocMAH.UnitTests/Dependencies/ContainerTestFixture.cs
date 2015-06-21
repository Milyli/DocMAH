using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocMAH.Dependencies;
using NUnit.Framework;

namespace DocMAH.UnitTests.Dependencies
{
	[TestFixture]
	public class ContainerTestFixture
	{
		#region Tests

		[Test]
		[Description("Throws ArgumentNullExpcetion if name is not provided.")]
		[ExpectedException(typeof(ArgumentNullException), MatchType = MessageMatch.Contains, ExpectedMessage = "The name of the resolver is required.")]
		public void RegisterNamedResolver_NameNull()
		{
			// Arrange
			var container = new Container();

			// Act
			container.RegisterNamedResolver<IContainer>(string.Empty, c => container);

			// Assert
			Assert.Fail("An ArgumentNullException should have been thrown.");
		}

		[Test]
		[Description("Throws ArgumentNullException if resolver is not provided.")]
		[ExpectedException(typeof(ArgumentNullException), MatchType= MessageMatch.Contains, ExpectedMessage = "A resolver is required to create the desired instance.")]
		public void RegisterNamedResolver_ResolverNull()
		{
			// Arrange
			var container = new Container();

			// Act
			container.RegisterNamedResolver<IContainer>("Container", null);

			// Assert
			Assert.Fail("An ArgumentNullException should have been thrown.");
		}

		[Test]
		[Description("Resolves an unnamed instance.")]
		public void ResolveInstance_Success()
		{
			// Arrange
			var container = new Container();
			container.RegisterResolver<IContainer>(c => container);

			// Act
			var result = container.ResolveInstance<IContainer>();

			// Assert
			Assert.That(result, Is.Not.Null, "An instance should have been resolved.");
			Assert.That(result, Is.SameAs(container), "The container should have resolved itself given the registration.");
		}

		[Test]
		[Description("Throws InvalidOperationException if no creators are registered with the given name.")]
		[ExpectedException(typeof(InvalidOperationException), MatchType = MessageMatch.Contains, ExpectedMessage = "with name 'Default'.")]
		public void ResolveNamedInstance_NameNotRegistered()
		{
			// Arrange
			var container = new Container();
			container.RegisterNamedResolver<IContainer>("NotDefault", c => container);

			// Act
			container.ResolveNamedInstance<IContainer>("Default");

			// Assert
			Assert.Fail("An InvalidOperationException should have been thrown.");

		}

		[Test]
		[Description("Throws ArgumentNullException if name is not provided.")]
		[ExpectedException(typeof(ArgumentNullException))]
		public void ResolveNamedInstance_NameNull()
		{
			// Arrange
			var container = new Container();
			container.RegisterNamedResolver<IContainer>("Container", c => container);
						
			// Act
			container.ResolveNamedInstance<IContainer>(null);

			// Assert
			Assert.Fail("An ArgumentNullException should have been thrown.");
		}

		[Test]
		[Description("Resolves a named instance.")]
		public void ResolveNamedInstance_Success()
		{
			// Arrange
			var creatorName = "NamedContainer";
			var container = new Container();
			container.RegisterNamedResolver<IContainer>(creatorName, c => container);

			// Act
			var result = container.ResolveNamedInstance<IContainer>(creatorName);

			// Assert
			Assert.That(result, Is.Not.Null, "An instance should have been resolved.");
			Assert.That(result, Is.SameAs(container), "The container should have resolved itself given the registration.");
		}

		[Test]
		[Description("Throws InvalidOperationException if no creators are registered for the requested type.")]
		[ExpectedException(typeof(InvalidOperationException), MatchType = MessageMatch.Contains, ExpectedMessage = "type 'DocMAH.Dependencies.IContainer'.")]
		public void ResolveNamedInstance_TypeNotRegistered()
		{
			// Arrange
			var container = new Container();

			// Act
			container.ResolveNamedInstance<IContainer>("default");

			// Assert
			Assert.Fail("An InvalidOperationException should be thrown.");
		}

		#endregion
	}
}
