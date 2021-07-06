using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.DatePicker)]
	public partial class DatePickerHandlerTests : HandlerTestBase<DatePickerHandler, DatePickerStub>
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

		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var datePicker = new DatePickerStub()
			{
				Date = DateTime.Today,
				Font = Font.OfSize("Arial", fontSize)
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Font.FontSize, GetNativeUnscaledFontSize, datePicker.Font.FontSize);
		}
	}
}