using System;
using System.Threading.Tasks;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	public partial class CheckBoxTests
	{
		MauiCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
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