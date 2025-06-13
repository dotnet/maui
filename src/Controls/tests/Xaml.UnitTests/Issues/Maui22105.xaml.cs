using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui22105
	{
		public Maui22105()
		{
			InitializeComponent();
		}

		public Maui22105(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		// [TestFixture] - removed for xUnit
		class Test
		{
			public void Setup()
			{
				Application.SetCurrentApplication(new MockApplication());
				DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			}

			[TearDown] public void TearDown() => AppInfo.SetCurrent(null);

			[Fact]
			public void DefaultValueShouldBeApplied([Values(false, true)] bool useCompiledXaml)
			{
				var page = new Maui22105(useCompiledXaml);
				Assert.Equal(100, page.label.FontSize);
			}
		}
	}
}