using System;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CheckBoxHandlerTests
	{
		[Fact(DisplayName = "Disabled unchecked color is used when pressed state is present")]
		public void DisabledUncheckedColorIsUsedWhenPressed()
		{
			const int disabledUnchecked = unchecked((int)0xFF123456);
			const int fallback = unchecked((int)0xFF654321);
			using var colorStateList = PlatformInterop.GetCheckBoxColorStateList(
				unchecked((int)0xFF111111),
				unchecked((int)0xFF222222),
				unchecked((int)0xFF333333),
				disabledUnchecked);

			var color = colorStateList.GetColorForState(
				new[]
				{
					-global::Android.Resource.Attribute.StateEnabled,
					-global::Android.Resource.Attribute.StateChecked,
					global::Android.Resource.Attribute.StatePressed
				},
				new global::Android.Graphics.Color(fallback));

			Assert.Equal(disabledUnchecked, color);
		}

		[Theory(DisplayName = "Color (Foreground) Updates Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF0000FF)]
		public async Task ColorUpdatesCorrectly(uint color)
		{
			var layout = new LayoutStub();

			var checkBox = new CheckBoxStub
			{
				Foreground = new SolidPaint(Colors.Black),
				IsChecked = true
			};

			var button = new ButtonStub
			{
				Text = "Change Foreground"
			};

			layout.Add(checkBox);
			layout.Add(button);

			var clicked = false;

			var expected = Color.FromUint(color);

			button.Clicked += delegate
			{
				checkBox.Foreground = new SolidPaint(expected);
				clicked = true;
			};

			await PerformClick(button);

			Assert.True(clicked);

			await ValidateHasColor(checkBox, expected);
		}

		AppCompatCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			(AppCompatCheckBox)checkBoxHandler.PlatformView;

		bool GetNativeIsChecked(CheckBoxHandler checkBoxHandler) =>
			GetNativeCheckBox(checkBoxHandler).Checked;

		Task ValidateColor(ICheckBox checkBoxStub, Color color, Action action = null) =>
			ValidateHasColor(checkBoxStub, color, action);

		AppCompatButton GetNativeButton(ButtonHandler buttonHandler) =>
			buttonHandler.PlatformView;

		Task PerformClick(IButton button)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				GetNativeButton(CreateHandler<ButtonHandler>(button)).PerformClick();
			});
		}
	}
}