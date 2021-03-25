using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TimePickerHandlerTests
	{
		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var timePicker = new TimePickerStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Time = TimeSpan.FromHours(8)
			};

			float expectedValue = timePicker.CharacterSpacing.ToEm();

			var values = await GetValueAsync(timePicker, (handler) =>
			{
				return new
				{
					ViewValue = timePicker.CharacterSpacing,
					NativeViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue, EmCoefficientPrecision);
		}

		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("monospace")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var timePicker = new TimePickerStub()
			{
				Time = TimeSpan.FromHours(7),
				Font = Font.OfSize(family, 10)
			};

			var handler = await CreateHandlerAsync(timePicker);
			var nativeEntry = GetNativeTimePicker(handler);

			var fontManager = handler.Services.GetRequiredService<IFontManager>();

			var nativeFont = fontManager.GetTypeface(Font.OfSize(family, 0.0));

			Assert.Equal(nativeFont, nativeEntry.Typeface);

			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultTypeface, nativeEntry.Typeface);
			else
				Assert.NotEqual(fontManager.DefaultTypeface, nativeEntry.Typeface);
		}

		MauiTimePicker GetNativeTimePicker(TimePickerHandler timePickerHandler) =>
			(MauiTimePicker)timePickerHandler.View;

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

			if (mauiTimePicker != null)
			{
				return mauiTimePicker.LetterSpacing;
			}

			return -1;
		}

		double GetNativeUnscaledFontSize(TimePickerHandler timePickerHandler)
		{
			var mauiTimePicker = GetNativeTimePicker(timePickerHandler);
			return mauiTimePicker.TextSize / mauiTimePicker.Resources.DisplayMetrics.Density;
		}

		bool GetNativeIsBold(TimePickerHandler timePickerHandler) =>
			GetNativeTimePicker(timePickerHandler).Typeface.IsBold;

		bool GetNativeIsItalic(TimePickerHandler timePickerHandler) =>
			GetNativeTimePicker(timePickerHandler).Typeface.IsItalic;
	}
}