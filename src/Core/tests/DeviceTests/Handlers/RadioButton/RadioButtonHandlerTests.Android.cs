using System.Threading.Tasks;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class RadioButtonHandlerTests
	{
		AppCompatRadioButton GetNativeRadioButton(RadioButtonHandler radioButtonHandler) =>
			(AppCompatRadioButton)radioButtonHandler.View;

		bool GetNativeIsChecked(RadioButtonHandler radioButtonHandler) =>
			GetNativeRadioButton(radioButtonHandler).Checked;

		[Theory(DisplayName = "IsChecked Initializes Correctly")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task IsCheckedInitializesCorrectly(bool isChecked)
		{
			bool xplatIsChecked = isChecked;

			var slider = new RadioButtonStub()
			{
				IsChecked = xplatIsChecked
			};

			bool expectedValue = isChecked;

			var values = await GetValueAsync(slider, (handler) =>
			{
				return new
				{
					ViewValue = slider.IsChecked,
					NativeViewValue = GetNativeIsChecked(handler)
				};
			});

			Assert.Equal(xplatIsChecked, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue);
		}
	}
}
