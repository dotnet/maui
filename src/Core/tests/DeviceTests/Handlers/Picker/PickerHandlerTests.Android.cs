using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerHandlerTests
	{
		[Fact(DisplayName = "Title Initializes Correctly")]
		public async Task TitleInitializesCorrectly()
		{
			var picker = new PickerStub
			{
				Title = "Select an Item"
			};

			await ValidatePropertyInitValue(picker, () => picker.Title, GetNativeTitle, picker.Title);
		}

		[Fact(DisplayName = "Title Color Initializes Correctly")]
		public async Task TitleColorInitializesCorrectly()
		{
			var picker = new PickerStub
			{
				Title = "Select an Item",
				TitleColor = Color.CadetBlue
			};

			await ValidatePropertyInitValue(picker, () => picker.TitleColor, GetNativeTitleColor, picker.TitleColor);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var items = new List<string>
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var picker = new PickerStub()
			{
				Title = "Select an Item",
				CharacterSpacing = xplatCharacterSpacing
			};

			picker.ItemsSource = items;
			picker.SelectedIndex = 0;

			float expectedValue = picker.CharacterSpacing.ToEm();

			var values = await GetValueAsync(picker, (handler) =>
			{
				return new
				{
					ViewValue = picker.CharacterSpacing,
					NativeViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.NativeViewValue, EmCoefficientPrecision);
		}

		MauiPicker GetNativePicker(PickerHandler pickerHandler) =>
			pickerHandler.NativeView;

		string GetNativeTitle(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Hint;

		double GetNativeCharacterSpacing(PickerHandler pickerHandler)
		{
			var mauiPicker = GetNativePicker(pickerHandler);

			if (mauiPicker != null)
			{
				return mauiPicker.LetterSpacing;
			}

			return -1;
		}

		double GetNativeUnscaledFontSize(PickerHandler pickerHandler)
		{
			var mauiPicker = GetNativePicker(pickerHandler);
			return mauiPicker.TextSize / mauiPicker.Resources.DisplayMetrics.Density;
		}

		bool GetNativeIsBold(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Typeface.IsBold;

		bool GetNativeIsItalic(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Typeface.IsItalic;

		Color GetNativeTitleColor(PickerHandler pickerHandler)
		{
			var currentTextColorInt = GetNativePicker(pickerHandler).CurrentTextColor;
			var currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}
	}
}