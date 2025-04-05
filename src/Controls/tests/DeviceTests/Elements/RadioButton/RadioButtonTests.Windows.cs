using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RadioButtonTests
	{
		UI.Xaml.Controls.RadioButton GetNativeRadioButton(RadioButtonHandler radioButtonHandler) =>
			radioButtonHandler.PlatformView;

		bool GetNativeIsChecked(RadioButtonHandler radioButtonHandler) =>
			GetNativeRadioButton(radioButtonHandler).IsChecked ?? false;

		[Fact]
		[Description("The Opacity property of a RadioButton should match with native Opacity")]
		public async Task VerifyRadioButtonOpacityProperty()
		{
			var radioButton = new RadioButton
			{
				Opacity = 0.35f
			};
			var expectedValue = radioButton.Opacity;

			var handler = await CreateHandlerAsync<RadioButtonHandler>(radioButton);
			var nativeView = GetNativeRadioButton(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var nativeOpacityValue = (float)nativeView.Opacity;
				   Assert.Equal(expectedValue, nativeOpacityValue);
			   });
		}

		[Fact]
		[Description("The IsVisible property of a RadioButton should match with native IsVisible")]
		public async Task VerifyRadioButtonIsVisibleProperty()
		{
			var radioButton = new RadioButton();
			radioButton.IsVisible = false;
			var expectedValue = radioButton.IsVisible;

			var handler = await CreateHandlerAsync<RadioButtonHandler>(radioButton);
			var nativeView = GetNativeRadioButton(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				   var isVisible = nativeView.Visibility == Microsoft.UI.Xaml.Visibility.Visible;
				   Assert.Equal(expectedValue, isVisible);
			   });
		}
	}
}
