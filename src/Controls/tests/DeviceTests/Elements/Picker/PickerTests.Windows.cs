using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Xunit;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests : ControlsHandlerTestBase
	{
		[Fact(DisplayName = "HorizontalOptions Initializes Correctly")]
		public async Task HorizontalOptionsInitializesCorrectly()
		{
			var items = new ObservableCollection<string>()
			{
				"Item 1",
				"Item 2"
			};

			Picker picker = new Picker()
			{
				HorizontalOptions = LayoutOptions.End,
				ItemsSource = items,
				SelectedIndex = 0
			};

			var handler = await CreateHandlerAsync<PickerHandler>(picker);

			await InvokeOnMainThreadAsync(() => Assert.Equal(UI.Xaml.HorizontalAlignment.Right, GetPlatformHorizontalOptions(handler.PlatformView)));

		}

		[Fact(DisplayName = "VerticalOptions Initializes Correctly")]
		public async Task VerticalOptionsInitializesCorrectly()
		{
			var items = new ObservableCollection<string>()
			{
				"Item 1",
				"Item 2"
			};

			Picker picker = new Picker()
			{
				VerticalOptions = LayoutOptions.End,
				ItemsSource = items,
				SelectedIndex = 0
			};

			var handler = await CreateHandlerAsync<PickerHandler>(picker);

			await InvokeOnMainThreadAsync(() => Assert.Equal(UI.Xaml.VerticalAlignment.Bottom, GetPlatformVerticalOptions(handler.PlatformView)));
		}

		[Fact(DisplayName = "Title maps to PlaceholderText")]
		public async Task TitleMapsToPlaceholderText()
		{
			var picker = new Picker()
			{
				Title = "Select Option",
				ItemsSource = new ObservableCollection<string>()
				{
					"Item 1",
					"Item 2"
				}
			};

			var handler = await CreateHandlerAsync<PickerHandler>(picker);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal("Select Option", handler.PlatformView.PlaceholderText);
			});
		}

		[Fact(DisplayName = "Null title maps to empty PlaceholderText")]
		public async Task NullTitleMapsToEmptyPlaceholderText()
		{
			var picker = new Picker()
			{
				Title = null,
				ItemsSource = new ObservableCollection<string>()
				{
					"Item 1",
					"Item 2"
				}
			};

			var handler = await CreateHandlerAsync<PickerHandler>(picker);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(string.Empty, handler.PlatformView.PlaceholderText);
			});
		}

		[Fact(DisplayName = "Empty title maps to empty PlaceholderText")]
		public async Task EmptyTitleMapsToEmptyPlaceholderText()
		{
			var picker = new Picker()
			{
				Title = string.Empty,
				ItemsSource = new ObservableCollection<string>()
				{
					"Item 1",
					"Item 2"
				}
			};

			var handler = await CreateHandlerAsync<PickerHandler>(picker);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(string.Empty, handler.PlatformView.PlaceholderText);
			});
		}

		[Fact(DisplayName = "TitleColor maps to PlaceholderForeground")]
		public async Task TitleColorMapsToPlaceholderForeground()
		{
			var picker = new Picker()
			{
				Title = "Select Option",
				TitleColor = Colors.Red,
				ItemsSource = new ObservableCollection<string>()
				{
					"Item 1",
					"Item 2"
				}
			};

			var handler = await CreateHandlerAsync<PickerHandler>(picker);

			await InvokeOnMainThreadAsync(() =>
			{
				var placeholderBrush = Assert.IsType<WSolidColorBrush>(handler.PlatformView.PlaceholderForeground);
				Assert.Equal(Colors.Red, placeholderBrush.Color.ToColor());
			});
		}
		protected Task<string> GetPlatformControlText(ComboBox platformView)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				// these will only work if you've attached the combobox to a window
				var textBlock =
					platformView
						.GetDescendantByName<UI.Xaml.Controls.ContentPresenter>("ContentPresenter")
						?.GetFirstDescendant<TextBlock>();

				if (textBlock != null)
					return textBlock.Text;

				return platformView.SelectedItem?.ToString();
			});
		}

		UI.Xaml.HorizontalAlignment GetPlatformHorizontalOptions(ComboBox platformView)
		{
			return platformView.HorizontalAlignment;
		}

		UI.Xaml.VerticalAlignment GetPlatformVerticalOptions(ComboBox platformView)
		{
			return platformView.VerticalAlignment;
		}

		ComboBox GetPlatformPicker(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView;

		Task<float> GetPlatformOpacity(PickerHandler pickerHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformPicker(pickerHandler);
				return (float)nativeView.Opacity;
			});
		}
	}
}