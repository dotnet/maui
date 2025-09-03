using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz44213 : ContentPage
{
	public Bz44213()
	{
		InitializeComponent();
	}


	[TestFixture]
	class Tests
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp] public void Setup() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		[TearDown] public void TearDown() => DeviceInfo.SetCurrent(null);

		[Test]
		public void BindingInOnPlatform([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var p = new Bz44213(inflator);
			p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
			Assert.AreEqual("Foo", p.label.Text);
			mockDeviceInfo.Platform = DevicePlatform.Android;
			p = new Bz44213(inflator);
			p.BindingContext = new { Foo = "Foo", Bar = "Bar" };
			Assert.AreEqual("Bar", p.label.Text);
		}
	}
}
