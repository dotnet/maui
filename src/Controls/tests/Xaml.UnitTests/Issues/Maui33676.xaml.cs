using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33676 : ContentPage
{
	public Maui33676() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Tests() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		public void Dispose() => DeviceInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		// BUG: When a Setter uses OnPlatform without a Default value and the target platform 
		// doesn't match any of the specified platforms, SourceGen generates invalid code:
		// ((IValueProvider)).ProvideValue(...) - trying to call a method on a type instead of an instance
		// 
		// Expected behavior: The Setter should be skipped entirely or use a default value
		internal void SetterWithOnPlatformWithoutDefaultShouldNotGenerateInvalidCode(XamlInflator inflator)
		{
			// Test compiling for Android when OnPlatform only specifies iOS
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33676 : ContentPage
{
	public Maui33676() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui33676), targetFramework: "net10.0-android");
				
				var generated = result.GeneratedInitializeComponent();
				
				// BUG: Generated code contains invalid C# like:
				// ((global::Microsoft.Maui.Controls.Xaml.IValueProvider)).ProvideValue(...)
				// This causes: CS0119 'IValueProvider' is a type, which is not valid in the given context
				// This should NOT happen - when OnPlatform has no matching value, the Setter should be omitted
				Assert.DoesNotContain("((global::Microsoft.Maui.Controls.Xaml.IValueProvider)).ProvideValue", generated, StringComparison.Ordinal);
				
				Assert.Empty(result.Diagnostics);
			}
			else
			{
				mockDeviceInfo.Platform = DevicePlatform.Android;
				var page = new Maui33676(inflator);
				Assert.NotNull(page);
			}
		}

		[Theory]
		[XamlInflatorData]
		// When the platform DOES match, the Setter should work correctly
		internal void SetterWithOnPlatformMatchingPlatform(XamlInflator inflator)
		{
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33676 : ContentPage
{
	public Maui33676() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui33676), targetFramework: "net10.0-ios");
				
				Assert.Empty(result.Diagnostics);
				var generated = result.GeneratedInitializeComponent();
				Assert.Contains("0, 4, 0, 0", generated, StringComparison.Ordinal);
			}
			else
			{
				mockDeviceInfo.Platform = DevicePlatform.iOS;
				var page = new Maui33676(inflator);
				Assert.NotNull(page);
			}
		}
	}
}
