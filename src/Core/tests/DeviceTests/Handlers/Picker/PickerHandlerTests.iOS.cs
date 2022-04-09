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