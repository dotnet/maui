using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Gh7156 : ContentPage
{
	public Gh7156() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp] public void Setup() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		[TearDown] public void TearDown() => DeviceInfo.SetCurrent(null);

		[Test]
		public void OnPlatformDefaultToBPDefaultValue([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.Android;
			var layout = new Gh7156(inflator);
			Assert.That(layout.l0.Text, Is.EqualTo(Label.TextProperty.DefaultValue));
			Assert.That(layout.l0.WidthRequest, Is.EqualTo(VisualElement.WidthRequestProperty.DefaultValue));
			Assert.That(layout.l1.Text, Is.EqualTo("bar"));
			Assert.That(layout.l1.WidthRequest, Is.EqualTo(20d));
		}
	}
}
