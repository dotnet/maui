using Microsoft.Maui.Handlers;
using System.ComponentModel;
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
