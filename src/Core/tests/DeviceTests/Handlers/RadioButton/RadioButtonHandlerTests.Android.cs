using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RadioButtonHandlerTests
	{
		AppCompatRadioButton GetNativeRadioButton(RadioButtonHandler radioButtonHandler) =>
			(AppCompatRadioButton)radioButtonHandler.View;

		bool GetNativeIsChecked(RadioButtonHandler radioButtonHandler) =>
			GetNativeRadioButton(radioButtonHandler).Checked;
	}
}
