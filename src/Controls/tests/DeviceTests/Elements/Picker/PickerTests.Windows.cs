using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task TitleColorTracksThemeChanges(bool customTitleColor)
		{
			var picker = new Picker
			{
				Title = "Select an option",
				TitleColor = customTitleColor ? Colors.Red : null
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				var handler = CreateHandler<PickerHandler>(picker);
				var native = handler.PlatformView;
				var reference = (TextBlock)UI.Xaml.Markup.XamlReader.Load("""
					<TextBlock xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
					           Text="Reference"
					           Foreground="{ThemeResource DefaultTextForegroundThemeBrush}" />
					""");
				var host = new StackPanel
				{
					Children = { native, reference }
				};

				await host.AttachAndRun(async () =>
				{
					await AssertHelpers.AssertEventually(() =>
						native.GetDescendantByName<UI.Xaml.Controls.ContentPresenter>("HeaderContentPresenter")
							?.GetFirstDescendant<TextBlock>()?.IsLoaded == true &&
						reference.IsLoaded,
						message: "The Picker header and reference TextBlock did not load.");

					var title = native.GetDescendantByName<UI.Xaml.Controls.ContentPresenter>("HeaderContentPresenter")
						.GetFirstDescendant<TextBlock>();
					Assert.Equal(picker.Title, title.Text);
					var initialTheme = reference.ActualTheme;
					var initialColor = Assert.IsType<UI.Xaml.Media.SolidColorBrush>(reference.Foreground).Color;
					var expectedColor = customTitleColor ? Colors.Red.ToWindowsColor() : initialColor;
					await AssertHelpers.AssertEventually(() =>
						title.Foreground is UI.Xaml.Media.SolidColorBrush brush && brush.Color == expectedColor,
						message: "The Picker title color did not match the initial application theme or explicit TitleColor.");

					var nextTheme = initialTheme == UI.Xaml.ElementTheme.Light
						? UI.Xaml.ElementTheme.Dark
						: UI.Xaml.ElementTheme.Light;
					foreach (var theme in new[] { nextTheme, initialTheme })
					{
						var previousColor = Assert.IsType<UI.Xaml.Media.SolidColorBrush>(reference.Foreground).Color;
						host.RequestedTheme = theme;

						await AssertHelpers.AssertEventually(() =>
							reference.ActualTheme == theme &&
							reference.Foreground is UI.Xaml.Media.SolidColorBrush brush && brush.Color != previousColor,
							message: $"The reference TextBlock did not update for the {theme} theme.");

						expectedColor = customTitleColor
							? Colors.Red.ToWindowsColor()
							: Assert.IsType<UI.Xaml.Media.SolidColorBrush>(reference.Foreground).Color;
						await AssertHelpers.AssertEventually(() =>
							title.ActualTheme == theme &&
							title.Foreground is UI.Xaml.Media.SolidColorBrush brush && brush.Color == expectedColor,
							message: $"The Picker title color was incorrect after switching to the {theme} theme (customTitleColor: {customTitleColor}).");
					}
				}, MauiContext);
			});
		}

		[Fact]
		public async Task TitleColorUnsetPreservesApplicationHeaderTemplate()
		{
			var picker = new Picker
			{
				Title = "Select an option",
				TitleColor = null
			};

			await InvokeOnMainThreadAsync(() =>
			{
				var appResources = UI.Xaml.Application.Current.Resources;
				var customHeaderTemplate = (UI.Xaml.DataTemplate)UI.Xaml.Markup.XamlReader.Load("""
					<DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
						<TextBlock Text="Custom header" />
					</DataTemplate>
					""");
				var appPickerResources = new UI.Xaml.ResourceDictionary
				{
					["ComboBoxHeader"] = customHeaderTemplate
				};

				try
				{
					appResources.MergedDictionaries.Add(appPickerResources);
					var handler = CreateHandler<PickerHandler>(picker);

					Assert.Same(customHeaderTemplate, handler.PlatformView.HeaderTemplate);

					picker.TitleColor = Colors.Red;
					Assert.Same(customHeaderTemplate, handler.PlatformView.HeaderTemplate);

					picker.TitleColor = null;
					Assert.Same(customHeaderTemplate, handler.PlatformView.HeaderTemplate);
				}
				finally
				{
					appResources.MergedDictionaries.Remove(appPickerResources);
				}
			});
		}

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