using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.Content;
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
		[InlineData(false, false)]
		[InlineData(true, false)]
		[InlineData(false, true)]
		[InlineData(true, true)]
		public async Task LongSelectedTextScrollsHorizontallyAndRemainsReadOnly(bool useMaterialPicker, bool delayPressedState)
		{
			await RunPickerGestureTest(useMaterialPicker, delayPressedState, async (platformPicker, getClickCount) =>
			{
				DispatchTap(platformPicker);
				await WaitForPostedCallbacks(platformPicker);
				Assert.Equal(1, getClickCount());

				platformPicker.ScrollTo(0, platformPicker.ScrollY);
				DispatchHorizontalDrag(platformPicker);
				await WaitForPostedCallbacks(platformPicker);

				Assert.True(
					platformPicker.ScrollX > 0,
					$"Expected Picker ScrollX to be greater than 0 after a horizontal drag, but it was {platformPicker.ScrollX}.");
				Assert.Equal(1, getClickCount());

				DispatchTap(platformPicker);
				await WaitForPostedCallbacks(platformPicker);
				Assert.Equal(2, getClickCount());
			});
		}

		[Theory(DisplayName = "Picker Tap Jitter Clicks But Fast Swipe Does Not")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task TapJitterClicksButFastSwipeDoesNot(bool useMaterialPicker)
		{
			await RunPickerGestureTest(useMaterialPicker, delayPressedState: false, async (platformPicker, getClickCount) =>
			{
				DispatchJitteredTap(platformPicker);
				await WaitForPostedCallbacks(platformPicker);
				Assert.Equal(1, getClickCount());

				DispatchSwipeWithoutMove(platformPicker);
				await WaitForPostedCallbacks(platformPicker);
				Assert.Equal(1, getClickCount());

				DispatchTap(platformPicker);
				await WaitForPostedCallbacks(platformPicker);
				Assert.Equal(2, getClickCount());
			});
		}

		[Theory(DisplayName = "Picker Vertical Drag Does Not Click")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task VerticalDragDoesNotClick(bool useMaterialPicker)
		{
			await RunPickerGestureTest(useMaterialPicker, delayPressedState: false, async (platformPicker, getClickCount) =>
			{
				DispatchVerticalDrag(platformPicker);
				await WaitForPostedCallbacks(platformPicker);
				Assert.Equal(0, getClickCount());

				DispatchTap(platformPicker);
				await WaitForPostedCallbacks(platformPicker);
				Assert.Equal(1, getClickCount());
			});
		}

		[Theory(DisplayName = "Picker Canceled Drag Does Not Suppress Next Tap")]
		[InlineData(false)]
		[InlineData(true)]
		public async Task CanceledDragDoesNotSuppressNextTap(bool useMaterialPicker)
		{
			await RunPickerGestureTest(useMaterialPicker, delayPressedState: true, async (platformPicker, getClickCount) =>
			{
				DispatchCanceledDrag(platformPicker);
				await WaitForPostedCallbacks(platformPicker);
				Assert.Equal(0, getClickCount());

				DispatchTap(platformPicker);
				await WaitForPostedCallbacks(platformPicker);
				Assert.Equal(1, getClickCount());
			});
		}

		[Fact(DisplayName = "Picker Pointer Replacement Without Movement Remains A Tap")]
		public async Task PointerReplacementWithoutMovementRemainsATap()
		{
			await InvokeOnMainThreadAsync(() =>
			{
				using var platformPicker = new MauiPicker(MauiContext.Context);
				var filter = new PickerDragGestureFilter();
				var downTime = SystemClock.UptimeMillis();

				using var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, 10f, 10f, 0);
				using var pointerDown = CreateMultiPointerEvent(
					downTime,
					downTime + 16,
					(MotionEventActions)((int)MotionEventActions.PointerDown | (1 << 8)),
					[0, 1],
					[(10f, 10f), (50f, 10f)]);
				using var pointerUp = CreateMultiPointerEvent(
					downTime,
					downTime + 32,
					MotionEventActions.PointerUp,
					[0, 1],
					[(10f, 10f), (50f, 10f)]);
				using var up = CreateMultiPointerEvent(
					downTime,
					downTime + 48,
					MotionEventActions.Up,
					[1],
					[(50f, 10f)]);

				Assert.False(filter.ShouldCancelClick(platformPicker, down));
				Assert.False(filter.ShouldCancelClick(platformPicker, pointerDown));
				Assert.False(filter.ShouldCancelClick(platformPicker, pointerUp));
				Assert.False(filter.ShouldCancelClick(platformPicker, up));
			});
		}

		MauiPicker GetNativePicker(PickerHandler pickerHandler) =>
			pickerHandler.PlatformView;

		async Task RunPickerGestureTest(
			bool useMaterialPicker,
			bool delayPressedState,
			Func<EditText, Func<int>, Task> test)
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

				using var host = delayPressedState
					? new DelayedPressedFrameLayout(context)
					: new FrameLayout(context);

				host.AddView(platformPicker, new FrameLayout.LayoutParams(width, height)
				{
					Gravity = GravityFlags.Center
				});

				await host.AttachAndRun(async () =>
				{
					LayoutNativePicker(platformPicker, width, height);
					Assert.Equal(longSelectedItem, platformPicker.Text);
					AssertReadOnlyPicker(platformPicker);

					var clickCount = 0;
					platformPicker.Click += OnPickerClicked;

					try
					{
						await test(platformPicker, () => clickCount);
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
			Assert.True(platformPicker.Focusable);
			Assert.False(platformPicker.FocusableInTouchMode);
			Assert.False(platformPicker.IsTextSelectable);
			Assert.False(platformPicker.LongClickable);
		}

		static void DispatchHorizontalDrag(EditText platformPicker)
		{
			var downTime = SystemClock.UptimeMillis();
			var startX = platformPicker.Width - 4f;
			var midX = platformPicker.Width / 2f;
			var endX = 4f;
			var y = platformPicker.Height / 2f;

			using var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, startX, y, 0);
			using var move = MotionEvent.Obtain(downTime, downTime + 16, MotionEventActions.Move, midX, y, 0);
			using var secondMove = MotionEvent.Obtain(downTime, downTime + 32, MotionEventActions.Move, endX, y, 0);
			using var up = MotionEvent.Obtain(downTime, downTime + 48, MotionEventActions.Up, endX, y, 0);

			platformPicker.DispatchTouchEvent(down);
			platformPicker.DispatchTouchEvent(move);
			platformPicker.DispatchTouchEvent(secondMove);
			platformPicker.DispatchTouchEvent(up);
		}

		static void DispatchVerticalDrag(EditText platformPicker)
		{
			var downTime = SystemClock.UptimeMillis();
			var x = platformPicker.Width / 2f;
			var startY = 4f;
			var endY = platformPicker.Height - 4f;

			using var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, x, startY, 0);
			using var move = MotionEvent.Obtain(downTime, downTime + 16, MotionEventActions.Move, x + 1, endY, 0);
			using var up = MotionEvent.Obtain(downTime, downTime + 32, MotionEventActions.Up, x + 1, endY, 0);

			platformPicker.DispatchTouchEvent(down);
			platformPicker.DispatchTouchEvent(move);
			platformPicker.DispatchTouchEvent(up);
		}

		static void DispatchCanceledDrag(EditText platformPicker)
		{
			var downTime = SystemClock.UptimeMillis();
			var y = platformPicker.Height / 2f;

			using var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, platformPicker.Width - 4f, y, 0);
			using var move = MotionEvent.Obtain(downTime, downTime + 16, MotionEventActions.Move, 4f, y, 0);
			using var cancel = MotionEvent.Obtain(downTime, downTime + 32, MotionEventActions.Cancel, 4f, y, 0);

			platformPicker.DispatchTouchEvent(down);
			platformPicker.DispatchTouchEvent(move);
			platformPicker.DispatchTouchEvent(cancel);
		}

		static MotionEvent CreateMultiPointerEvent(
			long downTime,
			long eventTime,
			MotionEventActions action,
			int[] pointerIds,
			(float X, float Y)[] points)
		{
			var pointerCoords = new MotionEvent.PointerCoords[points.Length];
			var pointerProperties = new MotionEvent.PointerProperties[points.Length];

			for (var i = 0; i < points.Length; i++)
			{
				pointerProperties[i] = new MotionEvent.PointerProperties
				{
					Id = pointerIds[i],
					ToolType = MotionEventToolType.Finger
				};

				pointerCoords[i] = new MotionEvent.PointerCoords
				{
					X = points[i].X,
					Y = points[i].Y,
					Pressure = 1,
					Size = 1
				};
			}

			return MotionEvent.Obtain(
				downTime,
				eventTime,
				action,
				pointerIds.Length,
				pointerProperties,
				pointerCoords,
				MetaKeyStates.None,
				(MotionEventButtonState)0,
				1,
				1,
				0,
				(Edge)0,
				InputSourceType.Touchscreen,
				MotionEventFlags.None)!;
		}

		static void DispatchSwipeWithoutMove(EditText platformPicker)
		{
			var downTime = SystemClock.UptimeMillis();
			var y = platformPicker.Height / 2f;

			using var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, platformPicker.Width - 4f, y, 0);
			using var up = MotionEvent.Obtain(downTime, downTime + 16, MotionEventActions.Up, 4f, y, 0);

			platformPicker.DispatchTouchEvent(down);
			platformPicker.DispatchTouchEvent(up);
		}

		static void DispatchJitteredTap(EditText platformPicker)
		{
			var downTime = SystemClock.UptimeMillis();
			var x = platformPicker.Width / 2f;
			var y = platformPicker.Height / 2f;
			var touchSlop = ViewConfiguration.Get(platformPicker.Context)?.ScaledTouchSlop ?? 0;
			var jitter = global::System.Math.Max(1, touchSlop / 2f);

			using var down = MotionEvent.Obtain(downTime, downTime, MotionEventActions.Down, x, y, 0);
			using var move = MotionEvent.Obtain(downTime, downTime + 16, MotionEventActions.Move, x + jitter, y + jitter, 0);
			using var up = MotionEvent.Obtain(downTime, downTime + 32, MotionEventActions.Up, x + jitter, y + jitter, 0);

			platformPicker.DispatchTouchEvent(down);
			platformPicker.DispatchTouchEvent(move);
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

		sealed class DelayedPressedFrameLayout : FrameLayout
		{
			public DelayedPressedFrameLayout(Context context)
				: base(context)
			{
			}

			public override bool ShouldDelayChildPressedState() => true;
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