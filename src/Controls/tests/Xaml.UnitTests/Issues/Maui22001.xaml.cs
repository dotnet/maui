using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22001
{
	public Maui22001() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		MockDeviceDisplay mockDeviceDisplay;
		MockDeviceInfo mockDeviceInfo;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			DeviceDisplay.SetCurrent(mockDeviceDisplay = new MockDeviceDisplay());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			mockDeviceDisplay = null;
			mockDeviceInfo = null;
		}

		[Theory]
		[XamlInflatorData]
		internal void StateTriggerTargetName(XamlInflator inflator)
		{
			var page = new Maui22001(inflator);

			IWindow window = new Window { Page = page };
			Assert.True(page._firstGrid.IsVisible);
			Assert.False(page._secondGrid.IsVisible);

			mockDeviceDisplay.SetMainDisplayOrientation(DisplayOrientation.Landscape);
			Assert.False(page._firstGrid.IsVisible);
			Assert.True(page._secondGrid.IsVisible);
		}
	}
}
