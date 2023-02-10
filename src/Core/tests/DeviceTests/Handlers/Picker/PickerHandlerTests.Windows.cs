using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerHandlerTests
	{
		[Fact(DisplayName = "ItemsSource Updates Correctly")]
		public async Task ItemsSourceUpdatesCorrectly()
		{
			int selectedIndex = 0;

			var oldItems = new List<string>
			{
				"Old Item 1",
				"Old Item 2",
				"Old Item 3"
			};

			var newItems = new List<string>
			{
				"New Item 1",
				"New Item 2",
				"New Item 3"
			};

			var picker = new PickerStub()
			{
				Title = "Select an Item",
				ItemsSource = oldItems,
				SelectedIndex = 1
			};

			picker.ItemsSource = newItems;
			picker.SelectedIndex = selectedIndex;

			string expected = picker.ItemsSource[selectedIndex].ToString();
			var actual = await GetValueAsync(picker, handler => GetNativeText(handler));

			Assert.Equal(expected, actual);
		}

		[Fact(DisplayName = "Items Updates Correctly")]
		public async Task ItemsUpdatesCorrectly()
		{
			int selectedIndex = 0;

			var oldItems = new List<string>
			{
				"Old Item 1",
				"Old Item 2",
				"Old Item 3"
			};

			var newItems = new List<string>
			{
				"New Item 1",
				"New Item 2",
				"New Item 3"
			};

			var picker = new PickerStub()
			{
				Title = "Select an Item",
				Items = oldItems,
				SelectedIndex = 1
			};

			picker.Items = newItems;
			picker.SelectedIndex = selectedIndex;

			string expected = picker.Items[selectedIndex].ToString();
			var actual = await GetValueAsync(picker, handler => GetNativeText(handler));

			Assert.Equal(expected, actual);
		}

		ComboBox GetNativePicker(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView;

		UI.Xaml.HorizontalAlignment GetNativeHorizontalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).HorizontalContentAlignment;

		UI.Xaml.VerticalAlignment GetNativeVerticalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).VerticalAlignment;

		string GetNativeText(PickerHandler pickerHandler)
		{
			var comboBox = GetNativePicker(pickerHandler);

			return comboBox.Items[comboBox.SelectedIndex].ToString();
		}
	}
}