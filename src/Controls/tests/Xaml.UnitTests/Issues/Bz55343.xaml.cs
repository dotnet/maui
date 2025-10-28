using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Bz55343 : ContentPage
	{
		public Bz55343()
		{
			InitializeComponent();
		}

		public Bz55343(bool useCompiledXaml)
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

			[Fact(Skip = "[Bug] Types that require conversion don't work in OnPlatform: https://github.com/xamarin/Microsoft.Maui.Controls/issues/13830")]
			[InlineData(true)]
			[InlineData(false)]
			public void OnPlatformFontConversion(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new Bz55343(useCompiledXaml);
				Assert.Equal(16d, layout.label0.FontSize);
				Assert.Equal(64d, layout.label1.FontSize);
			}
		}
	}
}