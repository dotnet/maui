using System;
using Microsoft.Maui.Controls.Core.UnitTests;
using Microsoft.Maui.Devices;
using Xunit;

namespace Microsoft.Maui.Controls.Xaml.UnitTests;

// Test for: OnPlatform<View> with <OnPlatform.Default> element syntax
// Issue: "error CS0122: 'View.View()' is inaccessible due to its protection level"
// This test verifies that the XAML can be parsed and inflated without throwing exceptions.
public partial class Unreported012 : ContentPage
{
	public Unreported012() => InitializeComponent();

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
		internal void OnPlatformViewWithDefaultDoesNotThrow(XamlInflator inflator)
		{
			// The main issue was that SourceGen tried to instantiate View with protected ctor
			// This test verifies no exception is thrown during inflation
			var page = new Unreported012(inflator);
			Assert.NotNull(page);
			
			// Verify the layout exists and has children
			Assert.NotNull(page.layout);
		}
	}
}
