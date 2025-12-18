using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui4509 : ContentPage
{
	public Maui4509() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		public Tests() => AppInfo.SetCurrent(new MockAppInfo());

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformAsCollectionElementiOS(XamlInflator inflator)
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
			var page = new Maui4509(inflator);
			Assert.Equal(2, page.layout.Children.Count);
		}

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformAsCollectionElementAndroid(XamlInflator inflator)
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.Android));
			var page = new Maui4509(inflator);
			Assert.Single(page.layout.Children);
		}
	}
}

