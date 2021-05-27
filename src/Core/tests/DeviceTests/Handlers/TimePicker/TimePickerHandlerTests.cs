using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TimePicker)]
	public partial class TimePickerHandlerTests : HandlerTestBase<TimePickerHandler, TimePickerStub>
	{
		[Fact(DisplayName = "Time Initializes Correctly")]
		public async Task IsToggledInitializesCorrectly()
		{
			var timePicker = new TimePickerStub();

			var time = new TimeSpan(17, 0, 0);

			await ValidateTime(timePicker, () => timePicker.Time = time);
		}

		[Theory(DisplayName = "Font Size Initializes Correctly")]
		[InlineData(1)]
		[InlineData(10)]
		[InlineData(20)]
		[InlineData(100)]
		public async Task FontSizeInitializesCorrectly(int fontSize)
		{
			var timePicker = new TimePickerStub()
			{
				Time = new TimeSpan(17, 0, 0),
				Font = Font.OfSize("Arial", fontSize)
			};

			await ValidatePropertyInitValue(timePicker, () => timePicker.Font.FontSize, GetNativeUnscaledFontSize, timePicker.Font.FontSize);
		}

		[Theory(DisplayName = "Font Attributes Initialize Correctly")]
		[InlineData(FontWeight.Regular, false, false)]
		[InlineData(FontWeight.Bold, true, false)]
		[InlineData(FontWeight.Regular, false, true)]
		[InlineData(FontWeight.Bold, true, true)]
		public async Task FontAttributesInitializeCorrectly(FontWeight weight, bool isBold, bool isItalic)
		{
			var timePicker = new TimePickerStub()
			{
				Time = new TimeSpan(17, 0, 0),
				Font = Font.OfSize("Arial", 10, weight, isItalic ? FontSlant.Italic : FontSlant.Default)
			};

			await ValidatePropertyInitValue(timePicker, () => timePicker.Font.Weight == FontWeight.Bold, GetNativeIsBold, isBold);
			await ValidatePropertyInitValue(timePicker, () => timePicker.Font.FontSlant == FontSlant.Italic, GetNativeIsItalic, isItalic);
		}
	}
}