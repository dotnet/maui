using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;

using Microsoft.Maui.Graphics;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui19388 : ContentPage
{
	public Maui19388() => InitializeComponent();

	class Test
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public void Setup()
		{
			Application.SetCurrentApplication(new MockApplication());
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
			DispatcherProvider.SetCurrent(new DispatcherProviderStub());
		}


		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Test]
		public void OnPlatformAppThemeBindingRelease([Values] XamlInflator inflator)
		{
			Application.Current.UserAppTheme = AppTheme.Light;
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var page = new Maui19388(inflator);
			Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.Green));

			mockDeviceInfo.Platform = DevicePlatform.Android;
			page = new Maui19388(inflator);
			Assert.That(page.label0.BackgroundColor, Is.EqualTo(Colors.Red));
		}
	}
}
