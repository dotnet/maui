#if !MACCATALYST
using System;
using System.Globalization;
using System.Threading.Tasks;
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
		[Theory(DisplayName = "Default Format Uses Managed Current Culture")]
		[InlineData("en-US")]
		[InlineData("fr-FR")]
		[InlineData("zh-Hant-TW")]
		public Task DefaultFormatUsesManagedCurrentCulture(string cultureName)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var originalCulture = CultureInfo.CurrentCulture;

				try
				{
					var culture = new CultureInfo(cultureName);
					culture.DateTimeFormat.ShortTimePattern = $"'managed-{cultureName}-'HH:mm";
					CultureInfo.CurrentCulture = culture;
					var time = new TimeSpan(14, 30, 0);
					var timePicker = new TimePickerStub
					{
						Format = "t",
						Time = time
					};
					var handler = CreateHandler(timePicker);

					var actual = GetNativeTimePicker(handler).Text;
					var expected = DateTime.Today.Add(time).ToString("t", culture);

					Assert.Equal(expected, actual);
				}
				finally
				{
					CultureInfo.CurrentCulture = originalCulture;
				}
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