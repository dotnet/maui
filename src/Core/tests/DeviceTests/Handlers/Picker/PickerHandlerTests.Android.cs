using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;
using ATextAlignment = Android.Views.TextAlignment;

namespace Microsoft.Maui.DeviceTests
{
	public partial class PickerHandlerTests
	{
		[Fact(DisplayName = "Title Initializes Correctly")]
		public async Task TitleInitializesCorrectly()
		{
			var picker = new PickerStub
			{
				Title = "Select an Item"
			};

			await ValidatePropertyInitValue(picker, () => picker.Title, GetNativeTitle, picker.Title);
		}

		[Fact(DisplayName = "Title Color Initializes Correctly")]
		public async Task TitleColorInitializesCorrectly()
		{
			var picker = new PickerStub
			{
				Title = "Select an Item",
				TitleColor = Colors.CadetBlue
			};

			await ValidatePropertyInitValue(picker, () => picker.TitleColor, GetNativeTitleColor, picker.TitleColor);
		}

		[Fact(DisplayName = "Text Color Initializes Correctly")]
		public async Task TextColorInitializesCorrectly()
		{
			var picker = new PickerStub
			{
				Title = "Select an Item",
				TextColor = Colors.CadetBlue,
				Items = new[] { "Item 1", "Item2", "Item3" },
				SelectedIndex = 1
			};

			await ValidatePropertyInitValue(picker, () => picker.TextColor, GetNativeTextColor, picker.TextColor);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var items = new List<string>
			{
				"Item 1",
				"Item 2",
				"Item 3"
			};

			var picker = new PickerStub()
			{
				Title = "Select an Item",
				CharacterSpacing = xplatCharacterSpacing
			};

			picker.ItemsSource = items;
			picker.SelectedIndex = 0;

			float expectedValue = picker.CharacterSpacing.ToEm();

			var values = await GetValueAsync(picker, (handler) =>
			{
				return new
				{
					ViewValue = picker.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		[Theory(DisplayName = "Long Selected Text Scrolls Horizontally And Remains Read Only")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task LongSelectedTextScrollsHorizontallyAndRemainsReadOnly(bool useMaterialPicker)
		{
			const double pickerWidth = 96;
			const double pickerHeight = 80;
			const string longSelectedItem = "This is a very long selected picker item that should scroll horizontally";

			var picker = new PickerStub
			{
				Width = pickerWidth,
				Height = pickerHeight,
				Items = new[] { longSelectedItem },
				SelectedIndex = 0
			};

			await InvokeOnMainThreadAsync(async () =>
			{
				using var platformPicker = CreateNativePicker(picker, useMaterialPicker);
				var context = platformPicker.Context;
				var width = (int)context.ToPixels(pickerWidth);
				var height = (int)context.ToPixels(pickerHeight);

				LayoutNativePicker(platformPicker, width, height);

				await platformPicker.AttachAndRun(async () =>
				{
					Assert.Equal(longSelectedItem, platformPicker.Text);
					AssertReadOnlyPicker(platformPicker);

					var clickCount = 0;
					platformPicker.Click += OnPickerClicked;

					try
					{
						DispatchTap(platformPicker);
						await WaitForPostedCallbacks(platformPicker);
						Assert.Equal(1, clickCount);

						platformPicker.ScrollTo(0, platformPicker.ScrollY);
						DispatchHorizontalDrag(platformPicker);
						await WaitForPostedCallbacks(platformPicker);

						Assert.True(
							platformPicker.ScrollX > 0,
							$"Expected Picker ScrollX to be greater than 0 after a horizontal drag, but it was {platformPicker.ScrollX}.");
						Assert.Equal(1, clickCount);

						DispatchTap(platformPicker);
						await WaitForPostedCallbacks(platformPicker);
						Assert.Equal(2, clickCount);
					}
					finally
					{
						platformPicker.Click -= OnPickerClicked;
					}

					void OnPickerClicked(object sender, System.EventArgs e)
					{
						clickCount++;
					}
				});
			});
		}

		MauiPicker GetNativePicker(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView;

		EditText CreateNativePicker(PickerStub picker, bool useMaterialPicker)
		{
			if (useMaterialPicker)
			{
				var materialPicker = new MauiMaterialPicker(MauiContext.Context);
				materialPicker.UpdatePicker(picker);
				return materialPicker;
			}

			var platformPicker = new MauiPicker(MauiContext.Context);
			platformPicker.UpdatePicker(picker);
			return platformPicker;
		}

		static void LayoutNativePicker(EditText platformPicker, int width, int height)
		{
			platformPicker.Measure(
				View.MeasureSpec.MakeMeasureSpec(width, MeasureSpecMode.Exactly),
				View.MeasureSpec.MakeMeasureSpec(height, MeasureSpecMode.Exactly));

			platformPicker.Layout(0, 0, width, height);
		}

		static void AssertReadOnlyPicker(EditText platformPicker)
		{
			Assert.Equal(InputTypes.Null, platformPicker.InputType);
			Assert.Null(platformPicker.KeyListener);
			Assert.False(platformPicker.FocusableInTouchMode);
			Assert.False(platformPicker.IsTextSelectable);
		}

		static void DispatchHorizontalDrag(EditText platformPicker)
		{
			var downTime = SystemClock.UptimeMillis();
			var y = platformPicker.Height / 2f;
			var startX = platformPicker.Width - 4f;
			var midX = platformPicker.Width / 2f;
			var endX = 4f;

			using var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, startX, y, 0);
			using var move = MotionEvent.Obtain(downTime, downTime + 16, MotionEventActions.Move, midX, y, 0);
			using var secondMove = MotionEvent.Obtain(downTime, downTime + 32, MotionEventActions.Move, endX, y, 0);
			using var up = MotionEvent.Obtain(downTime, downTime + 48, MotionEventActions.Up, endX, y, 0);

			platformPicker.DispatchTouchEvent(down);
			platformPicker.DispatchTouchEvent(move);
			platformPicker.DispatchTouchEvent(secondMove);
			platformPicker.DispatchTouchEvent(up);
		}

		static void DispatchTap(EditText platformPicker)
		{
			var downTime = SystemClock.UptimeMillis();
			var x = platformPicker.Width / 2f;
			var y = platformPicker.Height / 2f;

			using var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, x, y, 0);
			using var up = MotionEvent.Obtain(downTime, downTime + 48, MotionEventActions.Up, x, y, 0);

			platformPicker.DispatchTouchEvent(down);
			platformPicker.DispatchTouchEvent(up);
		}

		static Task WaitForPostedCallbacks(View view)
		{
			var completionSource = new TaskCompletionSource<bool>();
			view.Post(new Java.Lang.Runnable(() => completionSource.SetResult(true)));
			return completionSource.Task;
		}

		string GetNativeTitle(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Hint;

		double GetNativeCharacterSpacing(PickerHandler pickerHandler)
		{
			var mauiPicker = GetNativePicker(pickerHandler);

			if (mauiPicker != null)
			{
				return mauiPicker.LetterSpacing;
			}

			return -1;
		}

		ATextAlignment GetNativeHorizontalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).TextAlignment;

		Color GetNativeTitleColor(PickerHandler pickerHandler)
		{
			var currentTextColorInt = GetNativePicker(pickerHandler).CurrentHintTextColor;
			var currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		Color GetNativeTextColor(PickerHandler pickerHandler)
		{
			var currentTextColorInt = GetNativePicker(pickerHandler).CurrentTextColor;
			var currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		GravityFlags GetNativeVerticalTextAlignment(PickerHandler pickerHandler) =>
			GetNativePicker(pickerHandler).Gravity;
	}
}