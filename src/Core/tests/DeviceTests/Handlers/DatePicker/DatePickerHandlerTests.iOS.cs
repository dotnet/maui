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

		double GetNativeCharacterSpacing(DatePickerHandler datePickerHandler)
		{
			var mauiDatePicker = GetNativeDatePicker(datePickerHandler);
			return mauiDatePicker.AttributedText.GetCharacterSpacing();
		}

		double GetNativeUnscaledFontSize(DatePickerHandler datePickerHandler) =>
			GetNativeDatePicker(datePickerHandler).Font.PointSize;

		[Fact(DisplayName = "Date Picker Dialog Defaults to Today When Date is MinimumDate")]
		public async Task DatePickerDialogDefaultsToTodayWhenDateIsMinimumDate()
		{
			var datePicker = new DatePickerStub()
			{
				Date = new DateTime(1900, 1, 1) // Set to MinimumDate value
			};

			var values = await GetValueAsync(datePicker, (handler) =>
			{
				var dialog = handler.DatePickerDialog;
				return new
				{
					VirtualViewDate = datePicker.Date,
					DialogDate = dialog?.Date.ToDateTime()
				};
			});

			// VirtualView should have MinimumDate as set
			Assert.Equal(new DateTime(1900, 1, 1), values.VirtualViewDate);
			
			// Dialog should default to Today's date when Date is MinimumDate
			Assert.Equal(DateTime.Today, values.DialogDate?.Date);
		}

		[Fact(DisplayName = "Date Picker Dialog Uses Specified Date When Not MinimumDate")]
		public async Task DatePickerDialogUsesSpecifiedDateWhenNotMinimumDate()
		{
			var specificDate = new DateTime(2023, 6, 15);
			var datePicker = new DatePickerStub()
			{
				Date = specificDate
			};

			var values = await GetValueAsync(datePicker, (handler) =>
			{
				var dialog = handler.DatePickerDialog;
				return new
				{
					VirtualViewDate = datePicker.Date,
					DialogDate = dialog?.Date.ToDateTime()
				};
			});

			// VirtualView should have the specified date
			Assert.Equal(specificDate, values.VirtualViewDate);
			
			// Dialog should also have the specified date
			Assert.Equal(specificDate, values.DialogDate?.Date);
		}
	}
}
#endif