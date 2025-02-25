using Microsoft.Maui.Handlers;
using Xunit;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

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
			var nativeView = GetNativeRadioButton(handler);
			await InvokeOnMainThreadAsync(() =>
			{
				var nativeOpacityValue = (float)nativeView.Opacity;
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}

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
                var cornerRadius = (float)nativeView.CornerRadius.TopLeft;
                Assert.Equal(expected, cornerRadius);
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
				var isEnabled = nativeView.Enabled;
				Assert.Equal(expectedValue, isEnabled);
			});		
		}
	}
}
