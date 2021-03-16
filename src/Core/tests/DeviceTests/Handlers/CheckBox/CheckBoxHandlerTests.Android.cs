using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxHandlerTests
	{
		AppCompatCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			(AppCompatCheckBox)checkBoxHandler.View;

		bool GetNativeIsChecked(CheckBoxHandler checkBoxHandler) =>
			GetNativeCheckBox(checkBoxHandler).Checked;
	}
}