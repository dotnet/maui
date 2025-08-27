using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui4509 : ContentPage
{
	public Maui4509() => InitializeComponent();

	[TestFixture]
	class Test
	{
		[SetUp] public void Setup() => AppInfo.SetCurrent(new MockAppInfo());
		[TearDown]
		public void TearDown()
		{
			AppInfo.SetCurrent(null);
			DeviceInfo.SetCurrent(null);
		}

		[Test]
		public void OnPlatformAsCollectionElementiOS([Values] XamlInflator inflator)
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
			var page = new Maui4509(inflator);
			Assert.That(page.layout.Children.Count, Is.EqualTo(2));
		}

		[Test]
		public void OnPlatformAsCollectionElementAndroid([Values] XamlInflator inflator)
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.Android));
			var page = new Maui4509(inflator);
			Assert.That(page.layout.Children.Count, Is.EqualTo(1));
		}
	}
}

