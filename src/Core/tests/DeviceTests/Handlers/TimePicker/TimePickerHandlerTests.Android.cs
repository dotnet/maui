using System;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class TimePickerHandlerTests
	{
		[Theory(DisplayName = "IsCustom24HourFormat detects HH patterns correctly")]
		[InlineData("HH:mm", true)]
		[InlineData("HH:mm:ss", true)]
		[InlineData("HH.mm", true)]
		[InlineData("HH-mm-ss", true)]
		[InlineData("hh:mm", false)]
		[InlineData("hh:mm tt", false)]
		[InlineData("h:mm", false)]
		[InlineData("H:mm", false)]
		[InlineData("t", false)]
		[InlineData("T", false)]
		[InlineData("", false)]
		[InlineData(null, false)]
		public void IsCustom24HourFormatDetectsCorrectly(string format, bool expected)
		{
			Assert.Equal(expected, TimePickerHandler.IsCustom24HourFormat(format));
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var timePicker = new TimePickerStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Time = TimeSpan.FromHours(8)
			};

			float expectedValue = timePicker.CharacterSpacing.ToEm();

			var values = await GetValueAsync(timePicker, (handler) =>
			{
				return new
				{
					ViewValue = timePicker.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		[Fact(DisplayName = "Material3 Gradient Background Initializes Correctly")]
		public async Task Material3GradientBackgroundInitializesCorrectly()
		{
			var timePicker = new TimePickerStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Orange),
				Time = TimeSpan.FromHours(8),
				Width = 200,
				Height = 60
			};

			await AttachAndRun<TimePickerHandler2>(timePicker, async handler =>
			{
				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);
				await handler.PlatformView.AssertContainsColor(Colors.Orange, MauiContext);
			});
		}

		[Fact(DisplayName = "Material3 TimePicker Honors AtMost Height")]
		public async Task Material3TimePickerHonorsAtMostHeight()
		{
			var timePicker = new TimePickerStub
			{
				Time = TimeSpan.FromHours(8),
				Width = 200,
				Height = 44
			};

			await AttachAndRun<TimePickerHandler2>(timePicker, handler =>
			{
				var widthMeasureSpec = View.MeasureSpec.MakeMeasureSpec(200, MeasureSpecMode.Exactly);
				handler.PlatformView.Measure(widthMeasureSpec, View.MeasureSpec.MakeMeasureSpec(0, MeasureSpecMode.Unspecified));

				var constrainedHeight = handler.PlatformView.MeasuredHeight - 1;
				handler.PlatformView.Measure(widthMeasureSpec, View.MeasureSpec.MakeMeasureSpec(constrainedHeight, MeasureSpecMode.AtMost));

				Assert.Equal(constrainedHeight, handler.PlatformView.MeasuredHeight);
			});
		}

		[Fact(DisplayName = "Material3 Focus Enables Inner Field Focusability")]
		public async Task Material3FocusEnablesInnerFieldFocusability()
		{
			var timePicker = new TimePickerStub
			{
				Time = TimeSpan.FromHours(8),
				Width = 200,
				Height = 44
			};

			await AttachAndRun<TimePickerHandler2>(timePicker, handler =>
			{
				var inputEditText = handler.PlatformView.InputEditText!;

				// At rest the read-only field must never be an initial-focus candidate.
				Assert.False(inputEditText.Focusable);

				handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());

				Assert.True(inputEditText.Focusable);
				Assert.True(inputEditText.FocusableInTouchMode);
			});
		}

		[Fact(DisplayName = "Material3 Unfocus Resets Inner Field Focusability")]
		public async Task Material3UnfocusResetsInnerFieldFocusability()
		{
			var timePicker = new TimePickerStub
			{
				Time = TimeSpan.FromHours(8),
				Width = 200,
				Height = 44
			};

			await AttachAndRun<TimePickerHandler2>(timePicker, handler =>
			{
				var inputEditText = handler.PlatformView.InputEditText!;

				handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());
				Assert.True(inputEditText.Focusable);

				handler.Invoke(nameof(IView.Unfocus), null);

				Assert.False(inputEditText.Focusable);
				Assert.False(inputEditText.FocusableInTouchMode);
			});
		}

		[Fact(DisplayName = "Material3 IsFocused Syncs With Inner Field Focus")]
		public async Task Material3IsFocusedSyncsWithInnerFieldFocus()
		{
			var timePicker = new TimePickerStub
			{
				Time = TimeSpan.FromHours(8),
				Width = 200,
				Height = 44
			};

			await AttachAndRun<TimePickerHandler2>(timePicker, async handler =>
			{
				handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());
				await AssertEventually(() => timePicker.IsFocused);

				handler.Invoke(nameof(IView.Unfocus), null);
				await AssertEventually(() => !timePicker.IsFocused);
			});
		}

		[Fact(DisplayName = "Material3 Disconnect While Focused Resets Focus State And Focusability")]
		public async Task Material3DisconnectWhileFocusedResetsFocusStateAndFocusability()
		{
			var timePicker = new TimePickerStub
			{
				Time = TimeSpan.FromHours(8),
				Width = 200,
				Height = 44
			};

			await AttachAndRun<TimePickerHandler2>(timePicker, async handler =>
			{
				var inputEditText = handler.PlatformView.InputEditText!;

				handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());
				await AssertEventually(() => timePicker.IsFocused);
				Assert.True(inputEditText.Focusable);

				((IElementHandler)handler).DisconnectHandler();

				// The platform view is reused across reconnects, so a disconnect while focused must reset
				// both focus state and focusability; otherwise Controls remains focused while the read-only
				// field becomes an initial-focus candidate on reconnect.
				await AssertEventually(() => !timePicker.IsFocused);
				Assert.False(inputEditText.Focusable);
				Assert.False(inputEditText.FocusableInTouchMode);
			});
		}

		MauiTimePicker GetNativeTimePicker(TimePickerHandler timePickerHandler) =>
			(MauiTimePicker)timePickerHandler.PlatformView;

		async Task ValidateTime(ITimePicker timePickerStub, Action action = null)
		{
			var actual = await GetValueAsync(timePickerStub, handler =>
			{
				var native = GetNativeTimePicker(handler);
				action?.Invoke();
				return native.Text;
			});

			var expected = timePickerStub.ToFormattedString();

			Assert.Equal(actual, expected);
		}

		double GetNativeCharacterSpacing(TimePickerHandler timePickerHandler)
		{
			var mauiTimePicker = GetNativeTimePicker(timePickerHandler);

			if (mauiTimePicker != null)
			{
				return mauiTimePicker.LetterSpacing;
			}

			return -1;
		}

		Color GetNativeTextColor(TimePickerHandler timePickerHandler)
		{
			int currentTextColorInt = GetNativeTimePicker(timePickerHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}
	}
}