using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class Maui4509 : ContentPage
	{
		public Maui4509() => InitializeComponent();
		public Maui4509(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}
		class Test
		{
			public Test()
			{
				AppInfo.SetCurrent(new MockAppInfo());
			}

			public void Dispose()
			{
				AppInfo.SetCurrent(null);
				DeviceInfo.SetCurrent(null);
			}

			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void OnPlatformAsCollectionElementiOS(bool useCompiledXaml)
			{
				DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
				var page = new Maui4509(useCompiledXaml);
				Assert.Equal(2, page.layout.Children.Count);
			}

			[Theory]
			[InlineData(false)]
			[InlineData(true)]
			public void OnPlatformAsCollectionElementAndroid(bool useCompiledXaml)
			{
				DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.Android));
				var page = new Maui4509(useCompiledXaml);
				Assert.Equal(1, page.layout.Children.Count);
			}
		}
	}
}

