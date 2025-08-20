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
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatformAsCollectionElementiOS([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatformAsCollectionElementiOS([Values] XamlInflator inflator)
#endif
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.iOS));
			var page = new Maui4509(inflator);
			Assert.That(page.layout.Children.Count, Is.EqualTo(2));
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatformAsCollectionElementAndroid([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatformAsCollectionElementAndroid([Values] XamlInflator inflator)
#endif
		{
			DeviceInfo.SetCurrent(new MockDeviceInfo(platform: DevicePlatform.Android));
			var page = new Maui4509(inflator);
			Assert.That(page.layout.Children.Count, Is.EqualTo(1));
		}
	}
}

