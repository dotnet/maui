using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ClearButtonVisibilityTracksSelection()
		{
			var picker = new Picker
			{
				Items = { "One", "Two" },
				SelectedIndex = 1
			};
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			await InvokeOnMainThreadAsync(() =>
			{
				var platformPicker = handler.PlatformView;
				Assert.Equal(UITextFieldViewMode.Never, platformPicker.ClearButtonMode);
				picker.IsClearButtonVisible = true;
				Assert.Equal(UITextFieldViewMode.Always, platformPicker.ClearButtonMode);

				Assert.False(platformPicker.ShouldClear(platformPicker));

				Assert.Equal(-1, picker.SelectedIndex);
				Assert.Null(picker.SelectedItem);
				Assert.True(string.IsNullOrEmpty(platformPicker.Text));
				Assert.Equal(UITextFieldViewMode.Never, platformPicker.ClearButtonMode);

				picker.SelectedIndex = 0;
				Assert.Equal(UITextFieldViewMode.Always, platformPicker.ClearButtonMode);
				picker.IsClearButtonVisible = false;
				Assert.Equal(UITextFieldViewMode.Never, platformPicker.ClearButtonMode);
				Assert.Equal(0, picker.SelectedIndex);
			});
		}

#if !MACCATALYST
		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task ClearWhileOpenIsNotRecommittedUntilAnItemIsChosen(bool chooseAgain, bool immediately)
		{
			var picker = new Picker
			{
				Items = { "One", "Two" },
				SelectedIndex = 1,
				IsClearButtonVisible = true
			};
			Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.Picker.SetUpdateMode(picker,
				immediately ? Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UpdateMode.Immediately
					: Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific.UpdateMode.WhenFinished);
			_ = await CreateHandlerAsync<PickerHandler>(picker);

			await CreateHandlerAndAddToWindow<PickerHandler>(picker, async handler =>
			{
				picker.IsOpen = true;
				await AssertEventually(() => picker.IsOpen && handler.PlatformView.IsFirstResponder);
				try
				{
					var native = handler.PlatformView;
					Assert.False(native.ShouldClear(native));
					Assert.True(picker.IsOpen);
					Assert.True(native.IsFirstResponder);
					Assert.Equal(-1, picker.SelectedIndex);
					var wheel = native.UIPickerView;
					var source = Assert.IsType<PickerSource>(wheel.Model);
					Assert.Equal(-1, source.SelectedIndex);

					if (chooseAgain)
						source.Selected(wheel, 0, 0);

					var accessory = Assert.IsType<MauiDoneAccessoryView>(native.InputAccessoryView);
					accessory.SendDoneClicked();
					Assert.Equal(chooseAgain ? 0 : -1, picker.SelectedIndex);
					Assert.Equal(chooseAgain ? "One" : null, picker.SelectedItem);
				}
				finally
				{
					picker.IsOpen = false;
				}
			});
		}

		[Fact]
		public async Task DoneStillSelectsFirstItemWhenSelectionHasNotBeenCleared()
		{
			var picker = new Picker { Items = { "One", "Two" }, IsClearButtonVisible = true };
			_ = await CreateHandlerAsync<PickerHandler>(picker);
			await CreateHandlerAndAddToWindow<PickerHandler>(picker, async handler =>
			{
				picker.IsOpen = true;
				await AssertEventually(() => handler.PlatformView.IsFirstResponder);
				try
				{
					var accessory = Assert.IsType<MauiDoneAccessoryView>(handler.PlatformView.InputAccessoryView);
					accessory.SendDoneClicked();
					Assert.Equal(0, picker.SelectedIndex);
					Assert.Equal("One", picker.SelectedItem);
				}
				finally
				{
					picker.IsOpen = false;
				}
			});
		}
#else
		[Fact]
		public async Task ClearButtonKeepsMacCatalystDialogOpen()
		{
			var picker = new Picker
			{
				Items = { "One", "Two" },
				SelectedIndex = 1,
				IsClearButtonVisible = true
			};
			_ = await CreateHandlerAsync<PickerHandler>(picker);
			await CreateHandlerAndAddToWindow<PickerHandler>(picker, async handler =>
			{
				picker.IsOpen = true;
				try
				{
					await AssertEventually(() => CurrentController(handler.PlatformView.Window?.RootViewController) is UIAlertController);
					var dialog = Assert.IsAssignableFrom<UIAlertController>(
						CurrentController(handler.PlatformView.Window?.RootViewController));
					var button = Assert.Single(dialog.View.Subviews.OfType<UIButton>(),
						view => view.AccessibilityIdentifier == "PickerClearButton");
					Assert.False(button.Hidden);
					button.SendActionForControlEvents(UIControlEvent.TouchUpInside);

					Assert.Equal(-1, picker.SelectedIndex);
					Assert.Null(picker.SelectedItem);
					Assert.True(picker.IsOpen);
					Assert.False(dialog.IsBeingDismissed);
					Assert.True(button.Hidden);
					var wheel = Assert.Single(dialog.View.Subviews.OfType<UIPickerView>());
					Assert.Equal(-1, Assert.IsType<PickerSource>(wheel.Model).SelectedIndex);
				}
				finally
				{
					handler.Invoke(nameof(IView.Unfocus), null);
					await AssertEventually(() => !picker.IsOpen);
				}
			});
		}

		static UIViewController CurrentController(UIViewController controller)
		{
			while (controller?.PresentedViewController is { } presented)
				controller = presented;
			return controller;
		}
#endif

		protected Task<string> GetPlatformControlText(MauiPicker platformView)
		{
			return InvokeOnMainThreadAsync(() => platformView.Text);
		}

		MauiPicker GetPlatformPicker(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView;

		Task<float> GetPlatformOpacity(PickerHandler pickerHandler)
		{
			return InvokeOnMainThreadAsync(() =>
			{
				var nativeView = GetPlatformPicker(pickerHandler);
				return (float)nativeView.Alpha;
			});
		}

	}
}