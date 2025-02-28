using System;
using System.ComponentModel;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Collection(RunInNewWindowCollection)]
	public partial class CheckBoxTests
	{
		AppCompatCheckBox GetNativeCheckBox(CheckBoxHandler checkBoxHandler) =>
			checkBoxHandler.PlatformView;

		Task<float> GetPlatformOpacity(CheckBoxHandler CheckBoxHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetNativeCheckBox(CheckBoxHandler);
				return nativeView.Alpha;
			});
		}
        
		[Fact]
		[Description("The ScaleX property of a CheckBox should match with native ScaleX")]
        public async Task ScaleXConsistent()
        {
            var checkBox = new CheckBox() { ScaleX = 0.45f };
            var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
            var expected = checkBox.ScaleX;
            var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
            Assert.Equal(expected, platformScaleX);
        }

        [Fact]
		[Description("The ScaleY property of a BoxView should match with native ScaleY")]
        public async Task ScaleYConsistent()
        {
            var checkBox = new CheckBox() { ScaleY = 0.45f };
            var handler = await CreateHandlerAsync<CheckBoxHandler>(checkBox);
            var expected = checkBox.ScaleY;
            var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
            Assert.Equal(expected, platformScaleY);
        }
	}
}