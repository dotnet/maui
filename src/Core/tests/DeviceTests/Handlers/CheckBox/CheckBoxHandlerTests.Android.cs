using System;
using System.Threading.Tasks;
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

		Task ValidateColor(ICheck checkBoxStub, Color color, Action action = null) =>
			 ValidateHasColor(checkBoxStub, color, action);

		Task ValidateHasColor(ICheck checkBoxStub, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeSwitch = GetNativeCheckBox(CreateHandler(checkBoxStub));
				action?.Invoke();
				nativeSwitch.AssertContainsColor(color);
			});
		}
	}
}