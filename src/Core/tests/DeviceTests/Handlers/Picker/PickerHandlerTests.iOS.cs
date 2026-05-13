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

		[Theory(DisplayName = "Updating SelectedIndex Does Not Affect CharacterSpacing")]
		[InlineData(0, 1)]
		[InlineData(1, 0)]
		public async Task SelectedIndexDoesNotAffectCharacterSpacing(int initialIndex, int newIndex)
		{
			var picker = new PickerStub
			{
				Title = "Select an Item",
				Items = new[] { "Apple", "Banana", "Cherry" },
				CharacterSpacing = 4,
				SelectedIndex = initialIndex
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(picker);
				handler.UpdateValue(nameof(IPicker.CharacterSpacing));

				var before = GetNativeCharacterSpacing(handler);
				Assert.Equal(4.0, before);

				picker.SelectedIndex = newIndex;
				handler.UpdateValue(nameof(IPicker.SelectedIndex));

				var after = GetNativeCharacterSpacing(handler);
				Assert.Equal(4.0, after);
			});
		}

		[Theory(DisplayName = "Selecting An Item With Done Keeps CharacterSpacing")]
		[InlineData(0, 1)]
		[InlineData(1, 0)]
		public async Task SelectingItemWithDoneDoesNotAffectCharacterSpacing(int initialIndex, int newIndex)
		{
			var picker = new PickerStub
			{
				Title = "Select an Item",
				Items = new[] { "Apple", "Banana", "Cherry" },
				CharacterSpacing = 4,
				SelectedIndex = initialIndex
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(picker);
				handler.UpdateValue(nameof(IPicker.CharacterSpacing));

				var before = GetNativeCharacterSpacing(handler);
				Assert.Equal(4.0, before);

				var mauiPicker = GetNativePicker(handler);
				mauiPicker.BecomeFirstResponder();

				var pickerView = mauiPicker.UIPickerView;
				var model = (PickerSource)pickerView.Model;
				pickerView.Select(newIndex, 0, false);
				model.Selected(pickerView, newIndex, 0);
				TapDoneOnInputAccessoryView(mauiPicker);

				Assert.Equal(picker.Items[newIndex], GetNativeText(handler));

				var after = GetNativeCharacterSpacing(handler);
				Assert.Equal(4.0, after);
			});
		}

		void TapDoneOnInputAccessoryView(MauiPicker mauiPicker)
		{
			var toolbar = mauiPicker.InputAccessoryView as UIToolbar;
			Assert.NotNull(toolbar);
			Assert.NotNull(toolbar.Items);
			Assert.NotEmpty(toolbar.Items);
			var doneButton = toolbar.Items[toolbar.Items.Length - 1];
			Assert.NotNull(doneButton);
			Assert.NotNull(doneButton.Target);
			Assert.NotNull(doneButton.Action);
			UIApplication.SharedApplication.SendAction(doneButton.Action, doneButton.Target, doneButton, null);
		}

		double GetNativeCharacterSpacing(PickerHandler pickerHandler)
		{
			var mauiPicker = GetNativePicker(pickerHandler);
			return mauiPicker.AttributedText.GetCharacterSpacing();
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