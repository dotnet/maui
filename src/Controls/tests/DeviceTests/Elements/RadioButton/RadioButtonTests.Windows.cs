using Microsoft.Maui.Handlers;
using xUnit;
using System.ComponentModel;
using System.Threading.Tasks;

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
			var nativeView = GetPlatformControl(handler);
			await InvokeOnMainThreadAsync(() =>
   			{
				var nativeOpacityValue = (float)nativeView.Alpha;
				Assert.Equal(expectedValue, nativeOpacityValue);
			});
		}
	}
}
