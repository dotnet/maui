using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui34074 : ContentPage
{
	public Maui34074() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		readonly MockDeviceInfo _mockDeviceInfo;

		public Tests()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(_mockDeviceInfo = new MockDeviceInfo());
		}

		public void Dispose() => DeviceInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformViewMissingTargetUsesNullDefault(XamlInflator inflator)
		{
			_mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			var page = new Maui34074(inflator);
			Assert.Null(page.Content);
		}

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformViewMatchingTargetStillWorks(XamlInflator inflator)
		{
			_mockDeviceInfo.Platform = DevicePlatform.WinUI;
			var page = new Maui34074(inflator);
			var label = Assert.IsType<Label>(page.Content);
			Assert.Equal("WinUI only", label.Text);
		}
	}
}
