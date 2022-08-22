using System;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxHandlerTests
	{
		CheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			checkBoxHandler.PlatformView;

		bool GetNativeIsChecked(CheckBoxHandler checkBoxHandler) =>
			(bool)GetNativeCheckBox(checkBoxHandler).IsChecked;

		async Task ValidateColor(ICheckBox checkBoxStub, Color color, Action action = null)
		{
			var expected = await GetValueAsync(checkBoxStub, handler =>
			{
				var native = GetNativeCheckBox(handler);
				action?.Invoke();

				var foreground = native.Foreground;

				if (foreground is SolidColorBrush solidColorBrush)
					return solidColorBrush.Color.ToColor();

				return null;
			});

			Assert.Equal(expected, color);
		}
	}
}
