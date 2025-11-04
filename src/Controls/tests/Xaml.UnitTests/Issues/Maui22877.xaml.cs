using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Dispatching;
using Microsoft.Maui.UnitTests;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui22877 : ContentPage
{
	public Maui22877() => InitializeComponent();

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
		public void OnBindingRelease([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Phone;
			var page = new Maui22877(inflator) { BindingContext = new { BoundString = "BoundString" } };
			Assert.That(page.label0.Text, Is.EqualTo("Grade"));

			mockDeviceInfo.Idiom = DeviceIdiom.Desktop;
			page = new Maui22877(inflator) { BindingContext = new { BoundString = "BoundString" } };
			Assert.That(page.label0.Text, Is.EqualTo("BoundString"));


		}
	}
}