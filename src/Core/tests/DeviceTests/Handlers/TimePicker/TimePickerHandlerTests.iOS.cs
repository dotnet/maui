using System;
using System.Threading.Tasks;
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

            var values = await GetValueAsync(timePicker, (handler) =>
            {
                return new
                {
                    ViewValue = timePicker.CharacterSpacing,
                    NativeViewValue = GetNativeCharacterSpacing(handler)
                };
            });

            Assert.Equal(xplatCharacterSpacing, values.ViewValue);
            Assert.Equal(xplatCharacterSpacing, values.NativeViewValue);
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
            return mauiTimePicker.AttributedText.GetCharacterSpacing();
        }
    }
}