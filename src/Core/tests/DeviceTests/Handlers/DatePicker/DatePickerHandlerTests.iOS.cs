#if !MACCATALYST
using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class DatePickerHandlerTests
	{
		[Fact(DisplayName = "Minimum Date Initializes Correctly")]
		public async Task MinimumDateInitializesCorrectly()
		{
			DateTime xplatMinimumDate = new DateTime(2000, 01, 01);

			var datePicker = new DatePickerStub()
			{
				MinimumDate = xplatMinimumDate,
				Date = DateTime.Today
			};

			DateTime expectedValue = xplatMinimumDate;

			var values = await GetValueAsync(datePicker, (handler) =>
			{
				return new
				{
					ViewValue = datePicker.MinimumDate,
					PlatformViewValue = GetNativeMinimumDate(handler)
				};
			});

			Assert.Equal(xplatMinimumDate, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "Maximum Date Initializes Correctly")]
		public async Task MaximumDateInitializesCorrectly()
		{
			DateTime xplatMaximumDate = new DateTime(2030, 01, 01);

			var datePicker = new DatePickerStub()
			{
				MinimumDate = new DateTime(2000, 01, 01),
				MaximumDate = new DateTime(2030, 01, 01),
				Date = DateTime.Today
			};

			DateTime expectedValue = xplatMaximumDate;

			var values = await GetValueAsync(datePicker, (handler) =>
			{
				return new
				{
					ViewValue = datePicker.MaximumDate,
					PlatformViewValue = GetNativeMaximumDate(handler)
				};
			});

			Assert.Equal(xplatMaximumDate, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var datePicker = new DatePickerStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Date = DateTime.Today
			};

			var values = await GetValueAsync(datePicker, (handler) =>
			{
				return new
				{
					ViewValue = datePicker.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);
		}

		[Fact(DisplayName = "Null Date Uses Today's Date in Picker")]
		public async Task NullDateUsesTodaysDateInPicker()
		{
			var datePicker = new DatePickerStub()
			{
				Date = null
			};

			var values = await GetValueAsync(datePicker, (handler) =>
			{
				return new
				{
					ViewValue = datePicker.Date,
					PlatformViewValue = GetNativePickerDate(handler)
				};
			});

			Assert.Null(values.ViewValue);
			Assert.Equal(DateTime.Today.Date, values.PlatformViewValue.Date);
		}

		[Fact(DisplayName = "Null Date Updates To Today's Date When Picker Updates")]
		public async Task NullDateUpdatesToTodaysDateWhenPickerUpdates()
		{
			var datePicker = new DatePickerStub()
			{
				Date = null
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler(datePicker);
				var platformView = handler.PlatformView;
				
				// Initially null date should show today's date in picker
				var picker = handler.DatePickerDialog;
				Assert.NotNull(picker);
				Assert.Equal(DateTime.Today.Date, picker.Date.ToDateTime().Date);
				
				// Update to a specific date
				var testDate = new DateTime(2023, 5, 15);
				datePicker.Date = testDate;
				
				// Verify the picker updates to the specific date
				await Task.Delay(100); // Allow update to process
				Assert.Equal(testDate.Date, picker.Date.ToDateTime().Date);
			});
		}

		MauiDatePicker GetNativeDatePicker(DatePickerHandler datePickerHandler) =>
			datePickerHandler.PlatformView;

		DateTime? GetNativeDate(DatePickerHandler datePickerHandler)
		{
			var dateString = GetNativeDatePicker(datePickerHandler).Text;
			DateTime.TryParse(dateString, out DateTime result);

			return result;
		}

		Color GetNativeTextColor(DatePickerHandler datePickerHandler) =>
			GetNativeDatePicker(datePickerHandler).TextColor.ToColor();

		DateTime GetNativeMinimumDate(DatePickerHandler datePickerHandler)
		{
			var dialog = datePickerHandler.DatePickerDialog;
			var minDate = dialog.MinimumDate;

			return minDate.ToDateTime();
		}

		DateTime GetNativeMaximumDate(DatePickerHandler datePickerHandler)
		{
			var dialog = datePickerHandler.DatePickerDialog;
			var maxDate = dialog.MaximumDate;

			return maxDate.ToDateTime();
		}

		DateTime GetNativePickerDate(DatePickerHandler datePickerHandler)
		{
			var dialog = datePickerHandler.DatePickerDialog;
			return dialog.Date.ToDateTime();
		}

		double GetNativeCharacterSpacing(DatePickerHandler datePickerHandler)
		{
			var mauiDatePicker = GetNativeDatePicker(datePickerHandler);
			return mauiDatePicker.AttributedText.GetCharacterSpacing();
		}

		double GetNativeUnscaledFontSize(DatePickerHandler datePickerHandler) =>
			GetNativeDatePicker(datePickerHandler).Font.PointSize;
	}
}
#endif