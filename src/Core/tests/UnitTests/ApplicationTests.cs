using System;
using Microsoft.Maui.Tests;
using Xunit;

namespace Microsoft.Maui.UnitTests
{
	[Category(TestCategory.Core, TestCategory.Lifecycle)]
	public class ApplicationTests : IDisposable
	{
		[Fact]
		public void CanCreateApplication()
		{
			var application = new AppStub();

			Assert.NotNull(App.Current);
			Assert.Equal(App.Current, application);
		}

		[Fact]
		public void ShouldntCreateMultipleApp()
		{
			var application = new AppStub();

			Assert.Throws<InvalidOperationException>(() => new AppStub());
		}

		public void Dispose()
		{
			(App.Current as AppStub)?.ClearApp();
		}
	}
}