using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;

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
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		MauiTimePicker GetNativeTimePicker(TimePickerHandler timePickerHandler) =>
			(MauiTimePicker)timePickerHandler.PlatformView;

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

		Color GetNativeTextColor(TimePickerHandler timePickerHandler)
		{
			int currentTextColorInt = GetNativeTimePicker(timePickerHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}
	}
}