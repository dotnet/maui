using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;
using ATextAlignment = Android.Views.TextAlignment;

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
				TitleColor = Colors.CadetBlue
			};

			await ValidatePropertyInitValue(picker, () => picker.TitleColor, GetNativeTitleColor, picker.TitleColor);
		}

		[Fact(DisplayName = "Text Color Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var picker = new PickerStub
			{
				Title = "Select an Item",
				TextColor = Colors.CadetBlue,
				Items = new[] { "Item 1", "Item2", "Item3" },
				SelectedIndex = 1
			};

			await ValidatePropertyInitValue(picker, () => picker.TextColor, GetNativeTextColor, picker.TextColor);
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
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		MauiPicker GetNativePicker(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView;

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

		ATextAlignment GetNativeHorizontalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).TextAlignment;

		Color GetNativeTitleColor(PickerHandler pickerHandler)
		{
			var currentTextColorInt = GetNativePicker(pickerHandler).CurrentHintTextColor;
			var currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		Color GetNativeTextColor(PickerHandler pickerHandler)
		{
			var currentTextColorInt = GetNativePicker(pickerHandler).CurrentTextColor;
			var currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		GravityFlags GetNativeVerticalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Gravity;
	}
}