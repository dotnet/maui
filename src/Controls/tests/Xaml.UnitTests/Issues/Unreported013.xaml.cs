using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Test for: OnPlatform<Brush> without Default when platform doesn't match
// Should not throw an exception and generate default(T) = null
public partial class Unreported013 : ContentPage
{
	public Unreported013() => InitializeComponent();

	[Collection("Issue")]
	public class Test : IDisposable
	{
		MockDeviceInfo _mockDeviceInfo;

		public Test()
		{
			DeviceInfo.SetCurrent(_mockDeviceInfo = new MockDeviceInfo());
		}

		public void Dispose()
		{
			DeviceInfo.SetCurrent(null);
		}

		[Theory]
		[XamlInflatorData]
		internal void OnPlatformBrushWithoutDefaultDoesNotThrow(XamlInflator inflator)
		{
			// The main issue was that SourceGen tried to instantiate abstract type Brush
			// when no matching platform and no Default. Now it generates default(T) = null.
			// This test verifies no exception is thrown during inflation
			var page = new Unreported013(inflator);
			Assert.NotNull(page);
			
			// Verify the page was created successfully - the resource may or may not exist
			// depending on the inflator and platform matching behavior
			Assert.NotNull(page.testBorder);
		}
	}
}
