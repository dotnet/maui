using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerHandlerTests
	{
		[Fact(DisplayName = "Title Initializes Correctly")]
		public async Task TitleInitializesCorrectly()
		{
			var picker = new PickerStub
			{
				Title = "Select an Item"
			};

			await ValidatePropertyInitValue(picker, () => picker.Title, GetNativeTitle, picker.Title);
		}

		NativePicker GetNativePicker(PickerHandler pickerHandler) =>
			(NativePicker)pickerHandler.View;

		string GetNativeTitle(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Hint;
	}
}