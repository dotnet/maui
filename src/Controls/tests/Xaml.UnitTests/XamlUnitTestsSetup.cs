using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	[SetUpFixture]
	public class MySetUpClass
	{
		// NOTE: xUnit doesn't have OneTimeSetUp. This may need to use ICollectionFixture or ModuleInitializer.
		// [OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			Microsoft.Maui.Controls.Hosting.CompatibilityCheck.UseCompatibility();
		}
	}
}
