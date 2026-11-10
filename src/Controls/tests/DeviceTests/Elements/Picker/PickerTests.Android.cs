using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Android.Widget;
using Android.Content;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
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
		public async Task ClearButtonClearsWithoutOpeningPicker(bool material, bool rtl)
		{
			var picker = new Picker
			{
				Items = { "One", "Two" },
				SelectedIndex = 1,
				IsClearButtonVisible = true
			};

			if (material)
			{
				var handler = await CreateHandlerAsync<PickerHandler2>(picker);
				await InvokeOnMainThreadAsync(() => AssertClearButton(picker, handler.PlatformView.Picker, rtl));
			}
			else
			{
				var handler = await CreateHandlerAsync<PickerHandler>(picker);
				await InvokeOnMainThreadAsync(() => AssertClearButton(picker, handler.PlatformView, rtl));
			}
		}

		static void AssertClearButton(Picker picker, AppCompatEditText platformPicker, bool rtl)
		{
			platformPicker.LayoutDirection = rtl ? LayoutDirection.Rtl : LayoutDirection.Ltr;
			platformPicker.Layout(0, 0, 400, 100);
			Assert.NotNull(platformPicker.GetCompoundDrawablesRelative()[2]);
			var clicks = 0;
			platformPicker.Click += (_, _) => clicks++;
			var x = rtl ? 1 : platformPicker.Width - 1;
			using var down = MotionEvent.Obtain(0, 0, MotionEventActions.Down, x, 50, 0);
			using var up = MotionEvent.Obtain(0, 1, MotionEventActions.Up, x, 50, 0);

			platformPicker.DispatchTouchEvent(down);
			platformPicker.DispatchTouchEvent(up);

			Assert.Equal(-1, picker.SelectedIndex);
			Assert.Null(picker.SelectedItem);
			Assert.False(picker.IsOpen);
			Assert.Equal(0, clicks);
			Assert.Null(platformPicker.GetCompoundDrawablesRelative()[2]);

			picker.SelectedIndex = 0;
			Assert.NotNull(platformPicker.GetCompoundDrawablesRelative()[2]);
			picker.IsClearButtonVisible = false;
			Assert.Null(platformPicker.GetCompoundDrawablesRelative()[2]);
			Assert.Equal(0, picker.SelectedIndex);
		}

		[Fact]
		public async Task ClearButtonKeepsAndroidDialogOpenAndClearsCheckedRow()
		{
			var picker = new Picker
			{
				Items = { "One", "Two" },
				SelectedIndex = 1,
				IsClearButtonVisible = true
			};
			_ = await CreateHandlerAsync<ClearablePickerHandler>(picker);

			await CreateHandlerAndAddToWindow<ClearablePickerHandler>(picker, handler =>
			{
				picker.IsOpen = true;
				try
				{
					var dialog = handler.Dialog;
					Assert.NotNull(dialog);
					var clearButton = dialog.GetButton((int)DialogButtonType.Neutral);
					Assert.Equal(ViewStates.Visible, clearButton.Visibility);
					clearButton.PerformClick();

					Assert.Equal(-1, picker.SelectedIndex);
					Assert.Null(picker.SelectedItem);
					Assert.True(picker.IsOpen);
					Assert.True(dialog.IsShowing);
					Assert.Equal(-1, dialog.ListView.CheckedItemPosition);
					Assert.Equal(ViewStates.Gone, clearButton.Visibility);

					picker.SelectedIndex = 0;
					Assert.Equal(ViewStates.Visible, clearButton.Visibility);
					Assert.Equal(0, dialog.ListView.CheckedItemPosition);
					picker.IsClearButtonVisible = false;
					Assert.Equal(ViewStates.Gone, clearButton.Visibility);
				}
				finally
				{
					picker.IsOpen = false;
				}
				return Task.CompletedTask;
			});
		}

		class ClearablePickerHandler : PickerHandler
		{
			public ClearablePickerHandler() { }

			public AndroidX.AppCompat.App.AlertDialog Dialog => _dialog;
		}

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

		[Fact]
		[Description("The ScaleX property of a Picker should match with native ScaleX")]
		public async Task ScaleXConsistent()
		{
			var picker = new Picker() { ScaleX = 0.45f };
			var expected = picker.ScaleX;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			Assert.Equal(expected, platformScaleX);
		}

		[Fact]
		[Description("The ScaleY property of a Picker should match with native ScaleY")]
		public async Task ScaleYConsistent()
		{
			var picker = new Picker() { ScaleY = 1.23f };
			var expected = picker.ScaleY;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The Scale property of a Picker should match with native Scale")]
		public async Task ScaleConsistent()
		{
			var picker = new Picker() { Scale = 2.0f };
			var expected = picker.Scale;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformScaleX = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleX);
			var platformScaleY = await InvokeOnMainThreadAsync(() => handler.PlatformView.ScaleY);
			Assert.Equal(expected, platformScaleX);
			Assert.Equal(expected, platformScaleY);
		}

		[Fact]
		[Description("The RotationX property of a Picker should match with native RotationX")]
		public async Task RotationXConsistent()
		{
			var picker = new Picker() { RotationX = 33.0 };
			var expected = picker.RotationX;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformRotationX = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationX);
			Assert.Equal(expected, platformRotationX);
		}

		[Fact]
		[Description("The RotationY property of a Picker should match with native RotationY")]
		public async Task RotationYConsistent()
		{
			var picker = new Picker() { RotationY = 87.0 };
			var expected = picker.RotationY;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformRotationY = await InvokeOnMainThreadAsync(() => handler.PlatformView.RotationY);
			Assert.Equal(expected, platformRotationY);
		}

		[Fact]
		[Description("The Rotation property of a Picker should match with native Rotation")]
		public async Task RotationConsistent()
		{
			var picker = new Picker() { Rotation = 23.0 };
			var expected = picker.Rotation;
			var handler = await CreateHandlerAsync<PickerHandler>(picker);
			var platformRotation = await InvokeOnMainThreadAsync(() => handler.PlatformView.Rotation);
			Assert.Equal(expected, platformRotation);
		}
	}
}