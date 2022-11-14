using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RadioButtonHandlerTests
	{
		AppCompatRadioButton GetNativeRadioButton(RadioButtonHandler radioButtonHandler) =>
			(AppCompatRadioButton)radioButtonHandler.PlatformView;

		bool GetNativeIsChecked(RadioButtonHandler radioButtonHandler) =>
			GetNativeRadioButton(radioButtonHandler).Checked;

		[Fact(DisplayName = "Control meets basic accessibility requirements")]
		[Category(TestCategory.Accessibility)]
		public async Task PlatformViewIsAccessible()
		{
			var view = new RadioButtonStub();
			await AssertPlatformViewIsAccessible(view);
		}
	}
}
