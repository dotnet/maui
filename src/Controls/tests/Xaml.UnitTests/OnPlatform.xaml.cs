using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class OnPlatform : ContentPage
{
	public OnPlatform() => InitializeComponent();

	[TestFixture]
	class Tests
	{
		MockDeviceInfo mockDeviceInfo;

		[SetUp]
		public void Setup()
		{
			DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());
		}

		[TearDown]
		public void TearDown()
		{
			DeviceInfo.SetCurrent(null);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void BoolToVisibility([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void BoolToVisibility([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.AreEqual(true, layout.label0.IsVisible);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(false, layout.label0.IsVisible);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void DoubleToWidth([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void DoubleToWidth([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.AreEqual(20, layout.label0.WidthRequest);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(30, layout.label0.WidthRequest);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(0.0, layout.label0.WidthRequest);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void StringToText([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void StringToText([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.AreEqual("Foo", layout.label0.Text);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new OnPlatform(inflator);
			Assert.AreEqual("Bar", layout.label0.Text);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(null, layout.label0.Text);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatformAsResource([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatformAsResource([Values] XamlInflator inflator)
#endif
		{
			var layout = new OnPlatform(inflator);
			var onplat = layout.Resources["fontAttributes"] as OnPlatform<FontAttributes>;
			Assert.NotNull(onplat);
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			Assert.AreEqual(FontAttributes.Bold, (FontAttributes)onplat);
			mockDeviceInfo.Platform = DevicePlatform.Android;
			Assert.AreEqual(FontAttributes.Italic, (FontAttributes)onplat);
			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			Assert.AreEqual(FontAttributes.None, (FontAttributes)onplat);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatformAsResourceAreApplied([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatformAsResourceAreApplied([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			var onidiom = layout.Resources["fontSize"] as OnIdiom<double>;
			Assert.NotNull(onidiom);
			Assert.That(onidiom.Phone, Is.TypeOf<double>());
			Assert.AreEqual(20, onidiom.Phone);
			Assert.AreEqual(FontAttributes.Bold, layout.label0.FontAttributes);

			mockDeviceInfo.Platform = DevicePlatform.Android;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(FontAttributes.Italic, layout.label0.FontAttributes);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatform2Syntax([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatform2Syntax([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.Android;
			var layout = new OnPlatform(inflator);
			Assert.AreEqual(42, layout.label0.HeightRequest);

			mockDeviceInfo.Platform = DevicePlatform.iOS;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(21, layout.label0.HeightRequest);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(63.0, layout.label0.HeightRequest);

			mockDeviceInfo.Platform = DevicePlatform.Create("FooBar");
			layout = new OnPlatform(inflator);
			Assert.AreEqual(42, layout.label0.HeightRequest);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatformDefault([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatformDefault([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.Create("\ud83d\ude80");
			var layout = new OnPlatform(inflator);
			Assert.AreEqual(63, layout.label0.HeightRequest);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatformInStyle0([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatformInStyle0([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.AreEqual(36, layout.button0.FontSize);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(0.0, layout.button0.FontSize);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatformInStyle1([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatformInStyle1([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.AreEqual(36, layout.button1.FontSize);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(0.0, layout.button1.FontSize);
		}

		[Test]
#if FIXME_BEFORE_PUBLIC_RELEASE
		public void OnPlatformInline([Values(XamlInflator.XamlC, XamlInflator.Runtime)] XamlInflator inflator)
#else
		public void OnPlatformInline([Values] XamlInflator inflator)
#endif
		{
			mockDeviceInfo.Platform = DevicePlatform.iOS;
			var layout = new OnPlatform(inflator);
			Assert.AreEqual(36, layout.button2.FontSize);

			mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
			layout = new OnPlatform(inflator);
			Assert.AreEqual(0.0, layout.button2.FontSize);
		}
	}
}