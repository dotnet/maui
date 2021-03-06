﻿using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TimePicker)]
	public partial class TimePickerHandlerTests : HandlerTestBase<TimePickerHandler, TimePickerStub>
	{
		[Fact(DisplayName = "Time Initializes Correctly")]
		public async Task TimeInitializesCorrectly()
		{
			var timePicker = new TimePickerStub
			{
				Format = "HH:mm"
			};

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

			await ValidatePropertyInitValue(timePicker, () => timePicker.Font.Size, GetNativeUnscaledFontSize, timePicker.Font.Size);
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
			await ValidatePropertyInitValue(timePicker, () => timePicker.Font.Slant == FontSlant.Italic, GetNativeIsItalic, isItalic);
		}

		[Fact(DisplayName = "Null Text Color Doesn't Crash")]
		public async Task NullTextColorDoesntCrash()
		{
			var timePicker = new TimePickerStub()
			{
				Time = DateTime.Now.TimeOfDay,
				TextColor = null
			};

			await CreateHandlerAsync(timePicker);
		}
	}
}