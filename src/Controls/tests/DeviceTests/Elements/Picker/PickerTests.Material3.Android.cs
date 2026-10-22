using System.Threading.Tasks;
using Android.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests;

public partial class PickerTests
{
	// Run in separate apps with UseMaterial3=true, false, and omitted. Do not mutate the process switch.
	[Fact]
	public Task PickerHandlerMatchesMaterialFeatureSwitch() =>
		InvokeOnMainThreadAsync(() =>
		{
			var picker = new Picker { Title = "Choose", ItemsSource = new[] { "One", "Two" }, SelectedIndex = 0 };
			var handler = CreateHandler(picker);
			try
			{
				EditText child;
				if (RuntimeFeature.IsMaterial3Enabled)
				{
					var material = Assert.IsType<PickerHandler2>(handler);
					child = Assert.IsType<MauiMaterialPicker>(material.PlatformView.EditText);
					Assert.Equal(Google.Android.Material.TextField.TextInputLayout.BoxBackgroundOutline,
						material.PlatformView.BoxBackgroundMode);
					Assert.True(material.PlatformView.EndIconVisible);
				}
				else
				{
					child = Assert.IsType<MauiPicker>(Assert.IsType<PickerHandler>(handler).PlatformView);
				}

				Assert.Equal("One", child.Text);
				picker.SelectedIndex = 1;
				Assert.Equal("Two", child.Text);
				picker.SelectedIndex = -1;
				Assert.True(string.IsNullOrEmpty(child.Text));
			}
			finally
			{
				var platformView = ((IViewHandler)handler).PlatformView as global::Android.Views.View;
				handler.DisconnectHandler();
				platformView?.Dispose();
			}
		});
}
