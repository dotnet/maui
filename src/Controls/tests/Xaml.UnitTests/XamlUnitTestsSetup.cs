using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	// XUnit assembly fixture - runs once before all tests
	public class AssemblyFixture : IDisposable
	{
		public AssemblyFixture()
		{
			Microsoft.Maui.Controls.Hosting.CompatibilityCheck.UseCompatibility();
		}

		public void Dispose()
		{
			// Cleanup if needed
		}
	}

	// Collection definition for XUnit to use the assembly fixture
	[CollectionDefinition("XamlUnitTests")]
	public class XamlUnitTestsCollection : ICollectionFixture<AssemblyFixture>
	{
	}
}
