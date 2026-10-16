using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui39341OnIdiomOuter : ContentPage
{
	public Maui39341OnIdiomOuter() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		readonly MockDeviceInfo mockDeviceInfo;

		public Tests() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		public void Dispose() => DeviceInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformNestedInOnIdiomUsesFinalTargetProperty(XamlInflator inflator)
		{
			mockDeviceInfo.Platform = DevicePlatform.Android;
			mockDeviceInfo.Idiom = DeviceIdiom.Phone;

			var page = new Maui39341OnIdiomOuter(inflator);

			Assert.Equal("PhoneAndroid", page.label.Text);
		}

		[Fact]
		internal void AndroidSourceGeneratorUsesFinalTargetPropertyForNestedOnPlatform()
		{
			var result = CreateMauiCompilation()
				.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui39341OnIdiomOuter : ContentPage
{
	public Maui39341OnIdiomOuter() => InitializeComponent();
}
""")
				.RunMauiSourceGenerator(typeof(Maui39341OnIdiomOuter), targetFramework: "net11.0-android");

			Assert.Empty(result.Diagnostics);
			var generated = result.GeneratedInitializeComponent();
			Assert.DoesNotMatch(
				@"SimpleValueTargetProvider\(\s*new object\?\[\] \{[^\r\n]*\},\s*null,",
				generated);
			Assert.Matches(
				@"SimpleValueTargetProvider\(\s*new object\?\[\] \{[^\r\n]*\},\s*global::Microsoft\.Maui\.Controls\.Label\.TextProperty,",
				generated);
		}
	}
}
