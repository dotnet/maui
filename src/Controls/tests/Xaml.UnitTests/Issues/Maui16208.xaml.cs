using System;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui16208
{
	public Maui16208() => InitializeComponent();

	public class Test : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Test()
		{
			Application.SetCurrentApplication(new MockApplication());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());

			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		public void Dispose()
		{
			AppInfo.SetCurrent(null);
			mockDeviceInfo = null;
			Application.Current = null;
			DispatcherProvider.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
		[Values]
		public void SetterAndTargetName(XamlInflator inflator)
		{
			// TODO: XUnit has no DoesNotThrow. Remove this or use try/catch if needed: // (() => new Maui16208(inflator));
			var page = new Maui16208(inflator);
			Assert.Equal(Colors.Green, page!.ItemLabel.BackgroundColor);
		}
	}
}
