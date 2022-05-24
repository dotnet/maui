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
		[Theory("Checkbox Background Updates Correctly With BackgroundColor Property")]
		[InlineData("#FF0000")]
		[InlineData("#00FF00")]
		[InlineData("#0000FF")]
		public async Task UpdatingCheckBoxBackgroundColorUpdatesBackground(string colorStr)
		{
			var color = Color.Parse(colorStr);

			var checkBox = new CheckBox
			{
				BackgroundColor = Colors.HotPink
			};

			checkBox.BackgroundColor = color;

			await ValidateHasColor(checkBox, color);
		}

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