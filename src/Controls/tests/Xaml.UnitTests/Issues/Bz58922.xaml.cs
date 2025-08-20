using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Bz58922 : ContentPage
{
	public Bz58922()
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
		public void OnIdiomXDouble([Values] XamlInflator inflator)
		{
			mockDeviceInfo.Idiom = DeviceIdiom.Phone;
			var layout = new Bz58922(inflator);
			Assert.That(layout.grid.HeightRequest, Is.EqualTo(320));

			mockDeviceInfo.Idiom = DeviceIdiom.Tablet;
			layout = new Bz58922(inflator);
			Assert.That(layout.grid.HeightRequest, Is.EqualTo(480));
		}
	}
}