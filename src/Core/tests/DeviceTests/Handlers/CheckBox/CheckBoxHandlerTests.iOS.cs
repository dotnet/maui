using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxHandlerTests
	{
		MauiCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			(MauiCheckBox)checkBoxHandler.NativeView;

		bool GetNativeIsChecked(CheckBoxHandler checkBoxHandler) =>
			GetNativeCheckBox(checkBoxHandler).IsChecked;
	}
}