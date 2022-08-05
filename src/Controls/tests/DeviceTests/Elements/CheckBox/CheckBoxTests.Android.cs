using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Xunit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;
using AndroidX.AppCompat.Widget;
using System;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	public partial class CheckBoxTests
	{
		AppCompatCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			checkBoxHandler.PlatformView;

		Task ValidateHasColor(ICheckBox checkBox, Color color, Action action = null)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler<CheckBoxHandler>(checkBox);
				var nativeSwitch = GetNativeCheckBox(handler);
				action?.Invoke();
				nativeSwitch.AssertContainsColor(color);
			});
		}
	}
}