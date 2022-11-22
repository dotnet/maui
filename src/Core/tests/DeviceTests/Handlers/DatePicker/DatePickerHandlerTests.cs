#if !MACCATALYST
using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.DatePicker)]
	public partial class DatePickerHandlerTests : CoreHandlerTestBase<DatePickerHandler, DatePickerStub>
	{
		[Fact(DisplayName = "Date Initializes Correctly")]
		public async Task DateInitializesCorrectly()
		{
			var datePicker = new DatePickerStub();

			datePicker.Date = DateTime.Today;

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, GetNativeDate, datePicker.Date);
		}

		[Fact(DisplayName = "TextColor Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var datePicker = new DatePickerStub()
			{
				Date = DateTime.Today,
				TextColor = Colors.Yellow
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.TextColor, GetNativeTextColor, datePicker.TextColor);
		}

		[Fact(DisplayName = "Null Text Color Doesn't Crash")]
		public async Task NullTextColorDoesntCrash()
		{
			var datePicker = new DatePickerStub()
			{
				Date = DateTime.Today,
				TextColor = null
			};

			await CreateHandlerAsync(datePicker);
		}

		[Category(TestCategory.DatePicker)]
		public class DatePickerTextStyleTests : TextStyleHandlerTests<DatePickerHandler, DatePickerStub>
		{

			protected override void SetText(DatePickerStub stub)
			{
				stub.Date = new DateTime(2042, 1, 1);
			}
		}
	}
}
#endif