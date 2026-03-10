using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

using static Microsoft.Maui.Controls.Xaml.UnitTests.MockSourceGenerator;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33676_VisualState : ContentPage
{
	public Maui33676_VisualState() => InitializeComponent();

	[Collection("Issue")]
	public class Tests : IDisposable
	{
		MockDeviceInfo mockDeviceInfo;

		public Tests() => DeviceInfo.SetCurrent(mockDeviceInfo = new MockDeviceInfo());

		public void Dispose() => DeviceInfo.SetCurrent(null);

		[Theory]
		[XamlInflatorData]
		// When a Setter inside VisualState uses OnPlatform without a Default value and the target platform 
		// doesn't match, SourceGen should handle it gracefully
		internal void SetterInVisualStateWithOnPlatformWithoutDefaultShouldNotGenerateInvalidCode(XamlInflator inflator)
		{
			// Test compiling for MacCatalyst when OnPlatform only specifies Android
			if (inflator == XamlInflator.SourceGen)
			{
				var result = CreateMauiCompilation()
					.WithAdditionalSource(
"""
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public partial class Maui33676_VisualState : ContentPage
{
	public Maui33676_VisualState() => InitializeComponent();
}
""")
					.RunMauiSourceGenerator(typeof(Maui33676_VisualState), targetFramework: "net10.0-maccatalyst");
				
				var generated = result.GeneratedInitializeComponent();
				
				// Check that we're NOT generating code that tries to add object to IList<Setter>
				// This would happen if IValueProvider.ProvideValue() is called on a Setter with no value
				Assert.DoesNotContain("((global::Microsoft.Maui.Controls.Xaml.IValueProvider)).ProvideValue", generated, StringComparison.Ordinal);
				
				// Should not have any compilation errors
				Assert.Empty(result.Diagnostics);
			}
			else
			{
				mockDeviceInfo.Platform = DevicePlatform.macOS;
				var page = new Maui33676_VisualState(inflator);
				Assert.NotNull(page);
			}
		}
	}
}
