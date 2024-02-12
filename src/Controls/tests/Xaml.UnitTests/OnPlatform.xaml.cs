using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using NUnit.Framework;

namespace Microsoft.Maui.Controls.Xaml.UnitTests
{
	public partial class OnPlatform : ContentPage
	{
		public OnPlatform()
		{
			InitializeComponent();
		}

		public OnPlatform(bool useCompiledXaml)
		{
			//this stub will be replaced at compile time
		}

		[TestFixture]
		public class Tests
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

			[TestCase(false)]
			[TestCase(true)]
			public void BoolToVisibility(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(true, layout.label0.IsVisible);

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(false, layout.label0.IsVisible);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void DoubleToWidth(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(20, layout.label0.WidthRequest);

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(30, layout.label0.WidthRequest);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(0.0, layout.label0.WidthRequest);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void StringToText(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual("Foo", layout.label0.Text);

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual("Bar", layout.label0.Text);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(null, layout.label0.Text);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformAsResource(bool useCompiledXaml)
			{
				var layout = new OnPlatform(useCompiledXaml);
				var onplat = layout.Resources["fontAttributes"] as OnPlatform<FontAttributes>;
				Assert.NotNull(onplat);
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				Assert.AreEqual(FontAttributes.Bold, (FontAttributes)onplat);
				mockDeviceInfo.Platform = DevicePlatform.Android;
				Assert.AreEqual(FontAttributes.Italic, (FontAttributes)onplat);
				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				Assert.AreEqual(FontAttributes.None, (FontAttributes)onplat);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformAsResourceAreApplied(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				var onidiom = layout.Resources["fontSize"] as OnIdiom<double>;
				Assert.NotNull(onidiom);
				Assert.That(onidiom.Phone, Is.TypeOf<double>());
				Assert.AreEqual(20, onidiom.Phone);
				Assert.AreEqual(FontAttributes.Bold, layout.label0.FontAttributes);

				mockDeviceInfo.Platform = DevicePlatform.Android;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(FontAttributes.Italic, layout.label0.FontAttributes);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatform2Syntax(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.Android;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(42, layout.label0.HeightRequest);

				mockDeviceInfo.Platform = DevicePlatform.iOS;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(21, layout.label0.HeightRequest);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(63.0, layout.label0.HeightRequest);

				mockDeviceInfo.Platform = DevicePlatform.Create("FooBar");
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(42, layout.label0.HeightRequest);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformDefault(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.Create("\ud83d\ude80");
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(63, layout.label0.HeightRequest);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformInStyle0(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(36, layout.button0.FontSize);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(0.0, layout.button0.FontSize);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformInStyle1(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(36, layout.button1.FontSize);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(0.0, layout.button1.FontSize);
			}

			[TestCase(false)]
			[TestCase(true)]
			public void OnPlatformInline(bool useCompiledXaml)
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(36, layout.button2.FontSize);

				mockDeviceInfo.Platform = DevicePlatform.MacCatalyst;
				layout = new OnPlatform(useCompiledXaml);
				Assert.AreEqual(0.0, layout.button2.FontSize);
			}
		}
	}
}