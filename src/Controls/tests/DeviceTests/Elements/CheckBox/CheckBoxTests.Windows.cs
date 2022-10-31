using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxTests
	{
		CheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			checkBoxHandler.PlatformView;

		Task ValidateHasColor(ICheckBox checkBox, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CheckBoxHandler>(checkBox);
				var nativeSwitch = GetNativeCheckBox(handler);
				action?.Invoke();
				nativeSwitch.AssertContainsColorAsync(color);
			});
		}
	}
}
