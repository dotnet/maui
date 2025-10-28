using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui3793 : ContentPage
	{
		public Maui3793() => InitializeComponent();
		public Maui3793(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Theory]
			public void Method([InlineData(false, true)] bool useCompiledXaml)
			{
				Maui3793 page;
				Assert.DoesNotThrow(() => page = new Maui3793(useCompiledXaml));
			}
		}
	}
}
