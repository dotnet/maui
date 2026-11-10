using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests : ControlsHandlerTestBase
	{
		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task WindowsClearButtonPreservesOpenState(bool open, bool rtl)
		{
			var selection = new WindowsPickerSelection();
			var picker = new Picker
			{
				Items = { "One", "Two" },
				IsClearButtonVisible = true,
				WidthRequest = 240,
				FlowDirection = rtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight
			};
			picker.SetBinding(Picker.SelectedIndexProperty,
				new Binding(nameof(WindowsPickerSelection.SelectedIndex), BindingMode.TwoWay, source: selection));
			var selectionChanges = 0;
			picker.SelectedIndexChanged += (_, _) => selectionChanges++;

			await AttachAndRun<PickerHandler>(picker, async handler =>
			{
				var native = Assert.IsType<MauiPicker>(handler.PlatformView);
				await AssertEventually(() => native.ClearButton?.IsLoaded == true);
				Assert.Equal(UI.Xaml.Visibility.Visible, native.ClearButton.Visibility);
				Assert.False(string.IsNullOrEmpty(AutomationProperties.GetName(native.ClearButton)));
				native.UpdateLayout();
				var presenter = native.GetDescendantByName<ContentPresenter>("ContentPresenter");
				var presenterBounds = presenter.TransformToVisual(native).TransformBounds(
					new global::Windows.Foundation.Rect(0, 0, presenter.ActualWidth, presenter.ActualHeight));
				var buttonBounds = native.ClearButton.TransformToVisual(native).TransformBounds(
					new global::Windows.Foundation.Rect(0, 0, native.ClearButton.ActualWidth, native.ClearButton.ActualHeight));
				Assert.True(native.ClearButton.ActualWidth >= 32);
				Assert.True(rtl ? buttonBounds.Right <= presenterBounds.Left : presenterBounds.Right <= buttonBounds.Left);

				if (open)
				{
					picker.IsOpen = true;
					await AssertEventually(() => native.PopupClearButton?.IsLoaded == true);
				}

				try
				{
					var button = open ? native.PopupClearButton : native.ClearButton;
					var peer = new ButtonAutomationPeer(button);
					Assert.IsAssignableFrom<IInvokeProvider>(peer.GetPattern(PatternInterface.Invoke)).Invoke();
					await AssertEventually(() => picker.SelectedIndex == -1);

					Assert.Null(picker.SelectedItem);
					Assert.Equal(-1, selection.SelectedIndex);
					Assert.Equal(-1, native.SelectedIndex);
					Assert.Equal(1, selectionChanges);
					Assert.Equal(open, native.IsDropDownOpen);
					Assert.Equal(open, picker.IsOpen);
					Assert.Equal(UI.Xaml.Visibility.Collapsed, native.ClearButton.Visibility);
					Assert.Equal(UI.Xaml.Visibility.Collapsed, native.PopupClearButton.Visibility);

					picker.IsOpen = false;
					Assert.Equal(-1, picker.SelectedIndex);
					selection.SelectedIndex = 0;
					Assert.Equal(0, native.SelectedIndex);
					Assert.Equal(UI.Xaml.Visibility.Visible, native.ClearButton.Visibility);
				}
				finally
				{
					picker.IsOpen = false;
				}
			});
		}

		class WindowsPickerSelection : BindableObject
		{
			int _selectedIndex = 1;

			public int SelectedIndex
			{
				get => _selectedIndex;
				set
				{
					if (_selectedIndex == value)
						return;

					_selectedIndex = value;
					OnPropertyChanged();
				}
			}
		}

		[Fact]
		public async Task WindowsClearButtonRestoresTemplateOnOptOutAndDisconnect()
		{
			var picker = new Picker { Items = { "One", "Two" }, SelectedIndex = 1 };
			await AttachAndRun<PickerHandler>(picker, async handler =>
			{
				var native = Assert.IsType<MauiPicker>(handler.PlatformView);
				await AssertEventually(() => native.IsLoaded);
				Assert.Null(native.ClearButton);
				var presenter = native.GetDescendantByName<ContentPresenter>("ContentPresenter");
				var originalParent = VisualTreeHelper.GetParent(presenter);

				picker.IsClearButtonVisible = true;
				Assert.NotNull(native.ClearButton);
				Assert.NotSame(originalParent, VisualTreeHelper.GetParent(presenter));
				picker.IsEnabled = false;
				Assert.False(native.ClearButton.IsEnabled);
				Assert.False(native.PopupClearButton.IsEnabled);
				picker.IsEnabled = true;
				Assert.True(native.ClearButton.IsEnabled);

				picker.SelectedIndex = -1;
				Assert.Equal(UI.Xaml.Visibility.Collapsed, native.ClearButton.Visibility);
				picker.SelectedIndex = 1;
				picker.Items.RemoveAt(1);
				Assert.Equal(UI.Xaml.Visibility.Collapsed, native.ClearButton.Visibility);
				picker.SelectedIndex = 0;
				Assert.Equal(UI.Xaml.Visibility.Visible, native.ClearButton.Visibility);

				picker.IsClearButtonVisible = false;
				Assert.Null(native.ClearButton);
				Assert.Null(native.PopupClearButton);
				Assert.Same(originalParent, VisualTreeHelper.GetParent(presenter));
				picker.IsClearButtonVisible = true;
				((IElementHandler)handler).DisconnectHandler();
				Assert.Null(native.ClearButton);
				Assert.Null(native.PopupClearButton);
				Assert.Null(native.ClearSelectionRequested);
				Assert.Same(originalParent, VisualTreeHelper.GetParent(presenter));
			});
		}

		[Fact]
		public async Task WindowsClearButtonsAreRecreatedWhenTemplateChanges()
		{
			var picker = new Picker
			{
				Items = { "One", "Two" },
				SelectedIndex = 1,
				IsClearButtonVisible = true
			};
			await AttachAndRun<PickerHandler>(picker, async handler =>
			{
				var native = Assert.IsType<MauiPicker>(handler.PlatformView);
				await AssertEventually(() => native.ClearButton?.IsLoaded == true);
				var originalTemplate = native.Template;
				var oldButton = native.ClearButton;
				native.Template = (UI.Xaml.Controls.ControlTemplate)XamlReader.Load("""
					<ControlTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
					                 xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
					                 TargetType="ComboBox">
					    <Grid>
					        <ContentPresenter x:Name="ContentPresenter" />
					        <Popup x:Name="Popup">
					            <Border x:Name="PopupBorder">
					                <ScrollViewer x:Name="ScrollViewer">
					                    <ItemsPresenter />
					                </ScrollViewer>
					            </Border>
					        </Popup>
					    </Grid>
					</ControlTemplate>
					""");
				native.ApplyTemplate();
				Assert.NotSame(oldButton, native.ClearButton);
				Assert.NotNull(native.PopupClearButton);
				native.Template = originalTemplate;
				native.ApplyTemplate();
				Assert.NotNull(native.ClearButton);
				Assert.Equal(UI.Xaml.Visibility.Visible, native.ClearButton.Visibility);
				Assert.Equal(1, picker.SelectedIndex);
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