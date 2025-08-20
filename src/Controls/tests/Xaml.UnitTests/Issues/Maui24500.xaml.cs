using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui24500 : ContentPage
{
	public Maui24500() => InitializeComponent();

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
		public void OnIdiomBindingValueTypeRelease([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Phone;
			var page = new Maui24500(inflator) { BindingContext = new { EditingMode = true } };
			Assert.That(page.label0.IsVisible, Is.EqualTo(false));

			mockDeviceInfo.Idiom = DeviceIdiom.Desktop;
			page = new Maui24500(inflator) { BindingContext = new { EditingMode = true } };
			Assert.That(page.label0.IsVisible, Is.EqualTo(true));


		}
	}
}