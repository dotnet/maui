using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RadioButtonHandlerTests
	{
		AppCompatRadioButton GetNativeRadioButton(RadioButtonHandler radioButtonHandler) =>
			(AppCompatRadioButton)radioButtonHandler.PlatformView;

		bool GetNativeIsChecked(RadioButtonHandler radioButtonHandler) =>
			GetNativeRadioButton(radioButtonHandler).Checked;
	}
}
