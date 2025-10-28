using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz44213 : ContentPage
	{
		public Bz44213()
		{
			InitializeComponent();
		}

		public Bz44213(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}		class Tests
		{
			MockDeviceInfo mockDeviceInfo;

			// NOTE: xUnit uses constructor for setup. This may need manual conversion.
		// [SetUp]
			public void Setup()
			{
				DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			}

			// NOTE: xUnit uses IDisposable.Dispose() for teardown. This may need manual conversion.
		// [TearDown]
			public void TearDown()
			{
				DeviceInfo.SetCurrent(null);
			}

			[InlineData(true)]
			[InlineData(false)]
			public void BindingInOnPlatform(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var p = new Bz44213(useCompiledXaml);
				p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
				Assert.Equal("Foo", p.label.Text);
				mockDeviceInfo.Platform = DevicePlatform.Android;
				p = new Bz44213(useCompiledXaml);
				p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
				Assert.Equal("Bar", p.label.Text);
			}
		}
	}
}