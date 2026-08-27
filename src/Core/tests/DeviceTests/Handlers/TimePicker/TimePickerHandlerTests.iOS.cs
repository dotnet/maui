#if !MACCATALYST
using System;
using System.Globalization;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TimePickerHandlerTests
	{
		[Fact(DisplayName = "Default Format Uses Current iOS Locale")]
		public Task DefaultFormatUsesCurrentiOSLocale()
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var time = new TimeSpan(14, 30, 0);
				var timePicker = new TimePickerStub
				{
					Format = "t",
					Time = time
				};
				var handler = CreateHandler(timePicker);

				var actual = GetNativeTimePicker(handler).Text;

				// Use native iOS formatting because LocaleIdentifier can contain
				// regional overrides that are not valid CultureInfo names.
				var today = DateTime.Today.Add(time);
				var components = new NSDateComponents
				{
					Year = today.Year,
					Month = today.Month,
					Day = today.Day,
					Hour = today.Hour,
					Minute = today.Minute,
					Second = today.Second,
					Calendar = NSCalendar.CurrentCalendar
				};
				var referenceDate = NSCalendar.CurrentCalendar.DateFromComponents(components);

				var dateFormatter = new NSDateFormatter
				{
					Locale = NSLocale.CurrentLocale,
					TimeStyle = NSDateFormatterStyle.Short,
					DateStyle = NSDateFormatterStyle.None
				};
				var expected = dateFormatter.StringFor(referenceDate);

				Assert.Equal(expected, actual);
			});
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var timePicker = new TimePickerStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Time = TimeSpan.FromHours(8)
			};

			var values = await GetValueAsync(timePicker, (handler) =>
			{
				return new
				{
					ViewValue = timePicker.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);
		}

		MauiTimePicker GetNativeTimePicker(TimePickerHandler timePickerHandler) =>
			(MauiTimePicker)timePickerHandler.PlatformView;

		Color GetNativeTextColor(TimePickerHandler timePickerHandler) =>
			GetNativeTimePicker(timePickerHandler).TextColor.ToColor();

		async Task ValidateTime(ITimePicker timePickerStub, Action action = null)
		{
			var actual = await GetValueAsync(timePickerStub, handler =>
			{
				var native = GetNativeTimePicker(handler);
				action?.Invoke();

				return native.Text;
			});

			var expected = timePickerStub.ToFormattedString();

			Assert.Equal(actual, expected);
		}

		double GetNativeCharacterSpacing(TimePickerHandler timePickerHandler)
		{
			var mauiTimePicker = GetNativeTimePicker(timePickerHandler);
			return mauiTimePicker.AttributedText.GetCharacterSpacing();
		}
	}
}
#endif