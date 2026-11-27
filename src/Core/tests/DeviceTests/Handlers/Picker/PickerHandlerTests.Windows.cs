using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerHandlerTests
	{
		[Fact]
		public async Task PickerHeaderUpdatesWithoutPropertyPathBindings()
		{
			var picker = new PickerStub { Title = "Initial title", CharacterSpacing = 1, TitleColor = Colors.Red };

			await AttachAndRun(picker, async handler =>
			{
				var comboBox = handler.PlatformView;
				TextBlock FindTitle(string text) => comboBox.GetChildren<TextBlock>().FirstOrDefault(child => child?.Text == text);

				await AssertEventually(() => FindTitle("Initial title") is not null);
				var header = FindTitle("Initial title");
				var binding = header.GetBindingExpression(TextBlock.TextProperty).ParentBinding;
				Assert.True(binding.Path is null || string.IsNullOrEmpty(binding.Path.Path));

				picker.Title = "Updated title";
				handler.UpdateValue(nameof(IPicker.Title));
				picker.CharacterSpacing = 2;
				handler.UpdateValue(nameof(IPicker.CharacterSpacing));
				picker.TitleColor = Colors.Blue;
				handler.UpdateValue(nameof(IPicker.TitleColor));

				await AssertEventually(() => FindTitle("Updated title") is not null);
				Assert.Same(header, FindTitle("Updated title"));
				Assert.Same(picker, comboBox.Header);
				Assert.Equal(picker.CharacterSpacing.ToEm(), header.CharacterSpacing);
				Assert.Equal(picker.TitleColor.ToWindowsColor(), Assert.IsType<SolidColorBrush>(header.Foreground).Color);

				picker.Title = null;
				handler.UpdateValue(nameof(IPicker.Title));
				Assert.Null(comboBox.Header);
			});
		}

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