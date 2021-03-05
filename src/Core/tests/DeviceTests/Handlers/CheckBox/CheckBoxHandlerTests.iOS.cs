using System;
using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxHandlerTests
	{
		NativeCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			(NativeCheckBox)checkBoxHandler.View;

		bool GetNativeIsChecked(CheckBoxHandler checkBoxHandler) =>
			GetNativeCheckBox(checkBoxHandler).IsChecked;

		async Task ValidateColor(ICheck checkBoxStub, Color color, Action action = null)
		{
			var expected = await GetValueAsync(checkBoxStub, handler =>
			{
				var native = GetNativeCheckBox(handler);
				action?.Invoke();
				return native.CheckBoxTintColor;
			});
			Assert.Equal(expected, color);
		}
	}
}