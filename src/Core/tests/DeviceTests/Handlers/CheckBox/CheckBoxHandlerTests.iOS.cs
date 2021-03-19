using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxHandlerTests
	{
		CheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			(CheckBox)checkBoxHandler.View;

		bool GetNativeIsChecked(CheckBoxHandler checkBoxHandler) =>
			GetNativeCheckBox(checkBoxHandler).IsChecked;
	}
}