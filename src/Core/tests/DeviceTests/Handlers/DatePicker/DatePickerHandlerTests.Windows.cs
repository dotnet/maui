using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class DatePickerHandlerTests
	{
		[Theory(DisplayName = "Native View Bounding Box is not empty")]
		[InlineData(1)]
		[InlineData(100)]
		[InlineData(1000)]
		public override async Task ReturnsNonEmptyNativeBoundingBox(int size)
		{
			var datePicker = new DatePickerStub()
			{
				Height = size,
				Width = size,
				Date = DateTime.Today,
				MinimumDate = DateTime.Today.AddDays(-1),
				MaximumDate = DateTime.Today.AddDays(1)
			};

			var nativeBoundingBox = await GetValueAsync(datePicker, handler => GetBoundingBox(handler));
			Assert.NotEqual(nativeBoundingBox, Rect.Zero);

			var expectedSize = new Size(size, size);
			AssertWithinTolerance(expectedSize, nativeBoundingBox.Size);
		}

		[Fact]
		public override async Task DisconnectHandlerDoesntCrash()
		{
			var datePicker = new DatePickerStub
			{
				MinimumDate = DateTime.Today.AddDays(-1),
				MaximumDate = DateTime.Today.AddDays(1),
				Date = DateTime.Today
			};

			var handler = await CreateHandlerAsync(datePicker) as IPlatformViewHandler;
			await InvokeOnMainThreadAsync(handler.DisconnectHandler);
		}

		[Theory(DisplayName = "Format Initializes Correctly")]
		[InlineData("dd/MM/yyyy", "{day.integer(2)}/{month.integer(2)}/{year.full}")]
		[InlineData("d/M/yy", "{day.integer}/{month.integer(1)}/{year.abbreviated}")]
		[InlineData("ddd/MMM/yyyy", "{dayofweek.abbreviated}/{month.abbreviated}/{year.full}")]
		[InlineData("dddd/MMMM/yyyy", "{dayofweek.full}/{month.full}/{year.full}")]
		public async Task FormatInitializesCorrectly(string format, string nativeFormat)
		{
			var datePicker = new DatePickerStub();

			datePicker.Date = DateTime.Today;
			datePicker.Format = format;

			await ValidatePropertyInitValue(datePicker, () => datePicker.Format, GetNativeFormat, format, nativeFormat);
		}

		CalendarDatePicker GetNativeDatePicker(DatePickerHandler datePickerHandler) =>
			datePickerHandler.PlatformView;

		string GetNativeFormat(DatePickerHandler datePickerHandler)
		{
			var plaformDatePicker = GetNativeDatePicker(datePickerHandler);
			return plaformDatePicker.DateFormat;
		}

		DateTime? GetNativeDate(DatePickerHandler datePickerHandler)
		{
			var plaformDatePicker = GetNativeDatePicker(datePickerHandler);
			var date = plaformDatePicker.Date;

			if (date.HasValue)
				return date.Value.DateTime;

			return DateTime.MinValue;
		}

		Color GetNativeTextColor(DatePickerHandler datePickerHandler)
		{
			var foreground = GetNativeDatePicker(datePickerHandler).Foreground;

			if (foreground is UI.Xaml.Media.SolidColorBrush solidColorBrush)
				return solidColorBrush.Color.ToColor();

			return null;
		}

		[Fact]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var datePicker = new DatePickerStub()
			{
				Date = DateTime.Today,
				CharacterSpacing = 10
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.CharacterSpacing, GetNativeCharacterSpacing, 10.0);
		}

		double GetNativeCharacterSpacing(DatePickerHandler datePickerHandler)
		{
			var platformDatePicker = GetNativeDatePicker(datePickerHandler);

			// Since CalendarDatePicker doesn't have CharacterSpacing property directly,
			// we need to check if it was applied to internal TextBlock elements
			var textBlocks = platformDatePicker.GetChildren<Microsoft.UI.Xaml.Controls.TextBlock>();
			foreach (var textBlock in textBlocks)
			{
				if (textBlock is not null && textBlock.CharacterSpacing > 0)
				{
					// Convert back from Em to original value using the extension method
					return textBlock.CharacterSpacing.FromEm();
				}
			}

			return 0.0;
		}

		[Fact]
		public async Task CharacterSpacingUpdatesCorrectly()
		{
			var datePicker = new DatePickerStub()
			{
				Date = DateTime.Today,
				CharacterSpacing = 5
			};

			await ValidatePropertyUpdatesValue(
				datePicker,
				nameof(IDatePicker.CharacterSpacing),
				GetNativeCharacterSpacing,
				5.0,
				15.0);
		}
	}
}
