using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Graphics;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Test for: OnPlatform<Brush> with <OnPlatform.Default> element syntax
// Issue: "Cannot create an instance of the abstract type or interface 'Brush'"
// This test verifies that the XAML can be parsed and inflated without throwing exceptions.
public partial class Unreported011 : ContentPage
{
	public Unreported011() => InitializeComponent();

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
		internal void OnPlatformBrushWithDefaultDoesNotThrow(XamlInflator inflator)
		{
			// The main issue was that SourceGen tried to instantiate abstract type Brush
			// This test verifies no exception is thrown during inflation
			var page = new Unreported011(inflator);
			Assert.NotNull(page);
			
			// Verify the resource dictionary contains the key (may be null or have a value depending on platform)
			Assert.True(page.Resources.ContainsKey("TestBrush"));
		}
	}
}
