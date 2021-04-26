using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerHandlerTests
	{
		MauiPicker GetNativePicker(PickerHandler pickerHandler) =>
			(MauiPicker)pickerHandler.NativeView;

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

		double GetNativeUnscaledFontSize(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Font.PointSize;

		bool GetNativeIsBold(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Bold);

		bool GetNativeIsItalic(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Font.FontDescriptor.SymbolicTraits.HasFlag(UIFontDescriptorSymbolicTraits.Italic);

		UITextAlignment GetNativeHorizontalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).TextAlignment;
	}
}