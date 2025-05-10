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
		[Description("The CornerRadius of a RadioButton should match with native CornerRadius")]
		public async Task RadioButtonCornerRadius()
		{
			var radioButton = new RadioButton();
			radioButton.CornerRadius = 15;
			var expectedValue = radioButton.CornerRadius;

			var handler = await CreateHandlerAsync<RadioButtonHandler>(radioButton);
			var nativeView = GetNativeRadioButton(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var cornerRadiusTopLeft = (float)nativeView.CornerRadius.TopLeft;
				var cornerRadiusTopRight = (float)nativeView.CornerRadius.TopRight;
				var cornerRadiusBottomLeft = (float)nativeView.CornerRadius.BottomLeft;
				var cornerRadiusBottomRight = (float)nativeView.CornerRadius.BottomRight;
				Assert.Equal(expectedValue, cornerRadiusTopLeft);
				Assert.Equal(expectedValue, cornerRadiusTopRight);
				Assert.Equal(expectedValue, cornerRadiusBottomLeft);
				Assert.Equal(expectedValue, cornerRadiusBottomRight);
			});
		}

		[Fact]
		[Description("The IsEnabled of a RadioButton should match with native IsEnabled")]
		public async Task VerifyRadioButtonIsEnabledProperty()
		{
			var radioButton = new RadioButton
			{
				IsEnabled = false
			};
			var expectedValue = radioButton.IsEnabled;

			var handler = await CreateHandlerAsync<RadioButtonHandler>(radioButton);
			var nativeView = GetNativeRadioButton(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var isEnabled = nativeView.IsEnabled;
				Assert.Equal(expectedValue, isEnabled);
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