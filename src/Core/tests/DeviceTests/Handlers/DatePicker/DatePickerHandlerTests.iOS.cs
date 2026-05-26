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

		[Fact(DisplayName = "Date Format Shows Four Digit Year")]
		public async Task DateFormatShowsFourDigitYear()
		{
			var datePicker = new DatePickerStub()
			{
				Date = new DateTime(2024, 6, 15),
				Format = "d" // Short date format
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, (handler) =>
			{
				var text = GetNativeDatePicker(handler).Text ?? string.Empty;
				// The text should contain "2024" not "24"
				Assert.Contains("2024", text, StringComparison.OrdinalIgnoreCase);
				return datePicker.Date;
			}, datePicker.Date);
		}

		[Fact(DisplayName = "Date Format With Empty Format Shows Four Digit Year")]
		public async Task DateFormatWithEmptyFormatShowsFourDigitYear()
		{
			var datePicker = new DatePickerStub()
			{
				Date = new DateTime(2024, 6, 15),
				Format = string.Empty // Empty format (default)
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, (handler) =>
			{
				var text = GetNativeDatePicker(handler).Text ?? string.Empty;
				// The text should contain "2024" not "24"
				Assert.Contains("2024", text, StringComparison.OrdinalIgnoreCase);
				return datePicker.Date;
			}, datePicker.Date);
		}

		[Fact(DisplayName = "Date Format With Null Format Shows Four Digit Year")]
		public async Task DateFormatWithNullFormatShowsFourDigitYear()
		{
			var datePicker = new DatePickerStub()
			{
				Date = new DateTime(2024, 6, 15),
				Format = null // Null format (default)
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, (handler) =>
			{
				var text = GetNativeDatePicker(handler).Text ?? string.Empty;
				// The text should contain "2024" not "24"  
				Assert.Contains("2024", text, StringComparison.OrdinalIgnoreCase);
				return datePicker.Date;
			}, datePicker.Date);
		}

		[Fact(DisplayName = "Long Date Format Uses Full Format")]
		public async Task LongDateFormatUsesFullFormat()
		{
			var datePicker = new DatePickerStub()
			{
				Date = new DateTime(2024, 6, 15),
				Format = "D" // Long date format
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, (handler) =>
			{
				var text = GetNativeDatePicker(handler).Text ?? string.Empty;
				// Long format should contain the full year
				Assert.Contains("2024", text, StringComparison.OrdinalIgnoreCase);
				// And should be a longer format (contains month name)
				Assert.True(text.Length > 10, $"Long date format should be longer than short format, got: {text}");
				return datePicker.Date;
			}, datePicker.Date);
		}

		[Fact(DisplayName = "Custom Format With Slash Shows Four Digit Year")]
		public async Task CustomFormatWithSlashShowsFourDigitYear()
		{
			var datePicker = new DatePickerStub()
			{
				Date = new DateTime(2024, 6, 15),
				Format = "MM/dd/yyyy" // Custom format with slash
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, (handler) =>
			{
				var text = GetNativeDatePicker(handler).Text ?? string.Empty;
				// Should show the exact format specified
				Assert.Equal("06/15/2024", text);
				return datePicker.Date;
			}, datePicker.Date);
		}

		[Fact(DisplayName = "Custom Format Without Slash Shows Four Digit Year")]
		public async Task CustomFormatWithoutSlashShowsFourDigitYear()
		{
			var datePicker = new DatePickerStub()
			{
				Date = new DateTime(2024, 6, 15),
				Format = "yyyy-MM-dd" // Custom format without slash
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, (handler) =>
			{
				var text = GetNativeDatePicker(handler).Text ?? string.Empty;
				// Should show the exact format specified
				Assert.Equal("2024-06-15", text);
				return datePicker.Date;
			}, datePicker.Date);
		}

		[Fact(DisplayName = "Null Date Shows Empty Text")]
		public async Task NullDateShowsEmptyText()
		{
			var datePicker = new DatePickerStub()
			{
				Date = null,
				Format = "d"
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, (handler) =>
			{
				var text = GetNativeDatePicker(handler).Text;
				// Should show empty text for null date
				Assert.Equal(string.Empty, text, StringComparer.OrdinalIgnoreCase);
				return datePicker.Date;
			}, datePicker.Date);
		}

		[Fact(DisplayName = "Case Insensitive Short Format Shows Four Digit Year")]
		public async Task CaseInsensitiveShortFormatShowsFourDigitYear()
		{
			var datePicker = new DatePickerStub()
			{
				Date = new DateTime(2024, 6, 15),
				Format = "D" // Uppercase D should use long format, not short format with year fix
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, (handler) =>
			{
				var text = GetNativeDatePicker(handler).Text ?? string.Empty;
				// Should use long date format, not short format
				Assert.Contains("2024", text, StringComparison.OrdinalIgnoreCase);
				// Should be longer than a typical short format
				Assert.True(text.Length > 15, $"Expected long format to be longer, got: {text}");
				return datePicker.Date;
			}, datePicker.Date);
		}

		[Fact(DisplayName = "Custom Format With Existing Four Digit Year Works")]
		public async Task CustomFormatWithExistingFourDigitYearWorks()
		{
			var datePicker = new DatePickerStub()
			{
				Date = new DateTime(2024, 6, 15),
				Format = "dd/MM/yyyy" // Already has yyyy
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, (handler) =>
			{
				var text = GetNativeDatePicker(handler).Text ?? string.Empty;
				// Should maintain the 4-digit year
				Assert.Equal("15/06/2024", text);
				return datePicker.Date;
			}, datePicker.Date);
 		}
		
		MauiDatePicker GetNativeDatePicker(DatePickerHandler datePickerHandler) =>
			datePickerHandler.PlatformView;

		DateTime? GetNativeDate(DatePickerHandler datePickerHandler)
		{
			var dateString = GetNativeDatePicker(datePickerHandler).Text ?? string.Empty;
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
	}
}
#endif