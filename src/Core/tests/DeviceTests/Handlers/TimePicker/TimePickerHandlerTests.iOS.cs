using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using UIKit;
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

		[Theory(DisplayName = "Font Family Initializes Correctly")]
		[InlineData(null)]
		[InlineData("Times New Roman")]
		[InlineData("Dokdo")]
		public async Task FontFamilyInitializesCorrectly(string family)
		{
			var timePicker = new TimePickerStub
			{
				Time = TimeSpan.FromHours(8),
				Font = Font.OfSize(family, 10)
			};

			var (services, nativeFont) = await GetValueAsync(timePicker, handler => (handler.Services, GetNativeTimePicker(handler).Font));

			var fontManager = services.GetRequiredService<IFontManager>();

			var expectedNativeFont = fontManager.GetFont(Font.OfSize(family, 0.0));

			Assert.Equal(expectedNativeFont.FamilyName, nativeFont.FamilyName);
			if (string.IsNullOrEmpty(family))
				Assert.Equal(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
			else
				Assert.NotEqual(fontManager.DefaultFont.FamilyName, nativeFont.FamilyName);
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

		double GetNativeUnscaledFontSize(TimePickerHandler timePickerHandler) =>
			GetNativeTimePicker(timePickerHandler).Font.PointSize;

		bool GetNativeIsBold(TimePickerHandler timePickerHandler) =>
			GetNativeTimePicker(timePickerHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Bold);

		bool GetNativeIsItalic(TimePickerHandler timePickerHandler) =>
			GetNativeTimePicker(timePickerHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Italic);
	}
}