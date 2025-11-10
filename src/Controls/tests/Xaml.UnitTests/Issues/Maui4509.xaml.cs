using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui4509 : ContentPage
{
	public Maui4509() => InitializeComponent();


	public class Test : IDisposable
	{
		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
			AppInfo.SetCurrent(new MockAppInfo());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
			DispatcherProvider.SetCurrent(null);
			Application.SetCurrentApplication(null);
		}

		[Theory]
		[Values]
		public void OnPlatformAsCollectionElementiOS(XamlInflator inflator)
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
			var page = new Maui4509(inflator);
			Assert.Equal(2, page.layout.Children.Count);
		}

		[Theory]
		[Values]
		public void OnPlatformAsCollectionElementAndroid(XamlInflator inflator)
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.Android));
			var page = new Maui4509(inflator);
			Assert.Single(page.layout.Children);
		}
	}
}

