using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerHandlerTests
	{
		[Fact(DisplayName = "Title Color Initializes Correctly")]
		public async Task TitleColorInitializesCorrectly()
		{
			var xplatTitleColor = Colors.CadetBlue;

			var picker = new PickerStub
			{
				Title = "Select an Item",
				TitleColor = xplatTitleColor
			};

			var expectedValue = xplatTitleColor.ToPlatform();

			var values = await GetValueAsync(picker, (handler) =>
			{
				return new
				{
					ViewValue = picker.TitleColor,
					PlatformViewValue = GetNativeTitleColor(handler)
				};
			});

			Assert.Equal(xplatTitleColor, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "Text Color Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var xplatTitleColor = Colors.CadetBlue;

			var picker = new PickerStub
			{
				Title = "Select an Item",
				TextColor = xplatTitleColor,
				Items = new[] { "Item 1", "Item2", "Item3" },
				SelectedIndex = 1
			};

			var expectedValue = xplatTitleColor.ToPlatform();

			var values = await GetValueAsync(picker, (handler) =>
			{
				return new
				{
					ViewValue = picker.TextColor,
					PlatformViewValue = GetNativeTextColor(handler)
				};
			});

			Assert.Equal(xplatTitleColor, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}
    
    [Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			const double xplatCharacterSpacing = 4;

			var picker = new PickerStub
			{
				Items = new[] { "Item 1", "Item 2", "Item 3" },
				SelectedIndex = 1,
				CharacterSpacing = xplatCharacterSpacing
			};

			var values = await GetValueAsync(picker, (handler) =>
			{
				return new
				{
					ViewValue = picker.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(xplatCharacterSpacing, values.PlatformViewValue);
		}

		[Fact(DisplayName = "CharacterSpacing Maintained After SelectedIndex Change")]
		public async Task CharacterSpacingMaintainedAfterSelectedIndexChange()
		{
			const double xplatCharacterSpacing = 4;

			var picker = new PickerStub
			{
				Items = new[] { "Item 1", "Item 2", "Item 3" },
				SelectedIndex = 0,
				CharacterSpacing = xplatCharacterSpacing
			};

			await SetValueAsync(picker, 2, (handler, value) =>
			{
				handler.VirtualView.SelectedIndex = value;
				handler.UpdateValue(nameof(IPicker.SelectedIndex));
			});

			var nativeCharacterSpacing = await GetValueAsync(picker, (handler) =>
				GetNativeCharacterSpacing(handler));

			Assert.Equal(xplatCharacterSpacing, nativeCharacterSpacing);
		}

		double GetNativeCharacterSpacing(PickerHandler pickerHandler)
		{
			var mauiPicker = GetNativePicker(pickerHandler);
			return mauiPicker.AttributedText.GetCharacterSpacing();
		}

		[Fact(DisplayName = "RoundedRect Picker Focus Halo Uses RoundedRect Shape on MacCatalyst 26+")]
		public async Task RoundedRectPickerFocusHaloUsesRoundedRectShapeOnMacCatalyst26()
		{
			if (!OperatingSystem.IsMacCatalystVersionAtLeast(26))
				return;

			var picker = new PickerStub
			{
				Title = "Select an Item"
			};

			var values = await GetValueAsync(picker, handler =>
			{
				handler.PlatformArrange(new Rect(0, 0, 220, 44));
				var nativePicker = GetNativePicker(handler);
				nativePicker.LayoutSubviews();

				var focusEffect = nativePicker.FocusEffect;
				var expectedRounded = UIFocusHaloEffect.Create(UIBezierPath.FromRoundedRect(nativePicker.Bounds, 5f));
				var expectedCapsule = UIFocusHaloEffect.Create(UIBezierPath.FromRoundedRect(nativePicker.Bounds, nativePicker.Bounds.Height / 2));

				var matchesExpectedRoundedShape = focusEffect?.IsEqual(expectedRounded) ?? false;
				var matchesCapsuleShape = focusEffect?.IsEqual(expectedCapsule) ?? false;

				nativePicker.BorderStyle = UITextBorderStyle.None;
				nativePicker.LayoutSubviews();
				var clearsWhenNotRoundedRect = nativePicker.FocusEffect is null;

				nativePicker.BorderStyle = UITextBorderStyle.RoundedRect;
				nativePicker.LayoutSubviews();
				var restoresWhenRoundedRect = nativePicker.FocusEffect is not null;

				return new
				{
					BorderStyle = nativePicker.BorderStyle,
					HasFocusEffect = focusEffect is not null,
					MatchesExpectedRoundedShape = matchesExpectedRoundedShape,
					MatchesCapsuleShape = matchesCapsuleShape,
					ClearsWhenNotRoundedRect = clearsWhenNotRoundedRect,
					RestoresWhenRoundedRect = restoresWhenRoundedRect
				};
			});

			Assert.Equal(UITextBorderStyle.RoundedRect, values.BorderStyle);
			Assert.True(values.HasFocusEffect);
			Assert.True(values.ClearsWhenNotRoundedRect);
			Assert.True(values.RestoresWhenRoundedRect);
			Assert.True(values.MatchesExpectedRoundedShape);
			Assert.False(values.MatchesCapsuleShape);
		}

		MauiPicker GetNativePicker(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView;

		string GetNativeTitle(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Text;

		string GetNativeText(PickerHandler pickerHandler) =>
			 GetNativePicker(pickerHandler).Text;

		async Task ValidateNativeItemsSource(IPicker picker, int itemsCount)
		{
			var expected = await GetValueAsync(picker, handler =>
			{
				var pickerView = GetNativePicker(handler).UIPickerView;
				var model = (PickerSource)pickerView.Model;
				return model.GetRowsInComponent(pickerView, 0);
			});
			Assert.Equal(expected, itemsCount);
		}

		async Task ValidateNativeSelectedIndex(IPicker slider, int selectedIndex)
		{
			var expected = await GetValueAsync(slider, handler =>
			{
				var pickerView = GetNativePicker(handler).UIPickerView;
				var model = (PickerSource)pickerView.Model;
				return model.SelectedIndex;
			});
			Assert.Equal(expected, selectedIndex);
		}

		UITextAlignment GetNativeHorizontalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).TextAlignment;

		UIColor GetNativeTitleColor(PickerHandler pickerHandler)
		{
			var mauiPicker = GetNativePicker(pickerHandler);
			return mauiPicker.AttributedPlaceholder.GetForegroundColor();
		}

		UIColor GetNativeTextColor(PickerHandler pickerHandler)
		{
			var mauiPicker = GetNativePicker(pickerHandler);
			return mauiPicker.TextColor;
		}

		UIControlContentVerticalAlignment GetNativeVerticalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).VerticalAlignment;
	}
}