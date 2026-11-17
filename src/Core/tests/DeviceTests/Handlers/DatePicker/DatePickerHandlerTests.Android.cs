using System;
using System.Threading.Tasks;
using Android.Views;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;
using AColor = Android.Graphics.Color;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class DatePickerHandlerTests
	{
		[Fact(DisplayName = "Minimum Date Initializes Correctly")]
		public async Task MinimumDateInitializesCorrectly()
		{
			DateTime xplatMinimumDate = new DateTime(2000, 01, 01);

			var datePicker = new DatePickerStub()
			{
				MinimumDate = xplatMinimumDate,
				Date = DateTime.Today
			};

			double expectedValue = (long)xplatMinimumDate.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;

			var values = await GetValueAsync(datePicker, (handler) =>
			{
				EnsureDialogCreated(datePicker, handler);
				return new
				{
					ViewValue = datePicker.MinimumDate,
					PlatformViewValue = GetNativeMinimumDate(handler)
				};
			});

			Assert.Equal(xplatMinimumDate, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "Maximum Date Initializes Correctly")]
		public async Task MaximumDateInitializesCorrectly()
		{
			DateTime xplatMaximumDate = new DateTime(2030, 01, 01);

			var datePicker = new DatePickerStub()
			{
				MinimumDate = new DateTime(2000, 01, 01),
				MaximumDate = new DateTime(2030, 01, 01),
				Date = DateTime.Today
			};

			double expectedValue = (long)xplatMaximumDate.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;

			var values = await GetValueAsync(datePicker, (handler) =>
			{
				EnsureDialogCreated(datePicker, handler);
				return new
				{
					ViewValue = datePicker.MaximumDate,
					PlatformViewValue = GetNativeMaximumDate(handler)
				};
			});

			Assert.Equal(xplatMaximumDate, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue);
		}

		[Fact(DisplayName = "CharacterSpacing Initializes Correctly")]
		public async Task CharacterSpacingInitializesCorrectly()
		{
			var xplatCharacterSpacing = 4;

			var datePicker = new DatePickerStub()
			{
				CharacterSpacing = xplatCharacterSpacing,
				Date = DateTime.Today
			};

			float expectedValue = datePicker.CharacterSpacing.ToEm();

			var values = await GetValueAsync(datePicker, (handler) =>
			{
				return new
				{
					ViewValue = datePicker.CharacterSpacing,
					PlatformViewValue = GetNativeCharacterSpacing(handler)
				};
			});

			Assert.Equal(xplatCharacterSpacing, values.ViewValue);
			Assert.Equal(expectedValue, values.PlatformViewValue, EmCoefficientPrecision);
		}

		[Fact(DisplayName = "Material3 Gradient Background Initializes Correctly")]
		public async Task Material3GradientBackgroundInitializesCorrectly()
		{
			var datePicker = new DatePickerStub
			{
				Background = new LinearGradientPaintStub(Colors.Red, Colors.Orange),
				Date = DateTime.Today,
				Width = 200,
				Height = 60
			};

			await AttachAndRun<DatePickerHandler2>(datePicker, async handler =>
			{
				await handler.PlatformView.AssertContainsColor(Colors.Red, MauiContext);
				await handler.PlatformView.AssertContainsColor(Colors.Orange, MauiContext);
			});
		}

		[Fact(DisplayName = "Material3 DatePicker Honors AtMost Height")]
		public async Task Material3DatePickerHonorsAtMostHeight()
		{
			var datePicker = new DatePickerStub
			{
				Date = DateTime.Today,
				Width = 200,
				Height = 44
			};

			await AttachAndRun<DatePickerHandler2>(datePicker, handler =>
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
			var datePicker = new DatePickerStub
			{
				Date = DateTime.Today,
				Width = 200,
				Height = 44
			};

			await AttachAndRun<DatePickerHandler2>(datePicker, handler =>
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
			var datePicker = new DatePickerStub
			{
				Date = DateTime.Today,
				Width = 200,
				Height = 44
			};

			await AttachAndRun<DatePickerHandler2>(datePicker, handler =>
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
			var datePicker = new DatePickerStub
			{
				Date = DateTime.Today,
				Width = 200,
				Height = 44
			};

			await AttachAndRun<DatePickerHandler2>(datePicker, async handler =>
			{
				handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());
				await AssertEventually(() => datePicker.IsFocused);

				handler.Invoke(nameof(IView.Unfocus), null);
				await AssertEventually(() => !datePicker.IsFocused);
			});
		}

		[Fact(DisplayName = "Material3 Disconnect While Focused Resets Focus State And Focusability")]
		public async Task Material3DisconnectWhileFocusedResetsFocusStateAndFocusability()
		{
			var datePicker = new DatePickerStub
			{
				Date = DateTime.Today,
				Width = 200,
				Height = 44
			};

			await AttachAndRun<DatePickerHandler2>(datePicker, async handler =>
			{
				var inputEditText = handler.PlatformView.InputEditText!;

				handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());
				await AssertEventually(() => datePicker.IsFocused);
				Assert.True(inputEditText.Focusable);

				((IElementHandler)handler).DisconnectHandler();

				// The platform view is reused across reconnects, so a disconnect while focused must reset
				// both focus state and focusability; otherwise Controls remains focused while the read-only
				// field becomes an initial-focus candidate on reconnect.
				await AssertEventually(() => !datePicker.IsFocused);
				Assert.False(inputEditText.Focusable);
				Assert.False(inputEditText.FocusableInTouchMode);
			});
		}

		MauiDatePicker GetNativeDatePicker(DatePickerHandler datePickerHandler) =>
			datePickerHandler.PlatformView;

		DateTime? GetNativeDate(DatePickerHandler datePickerHandler)
		{
			var dateString = GetNativeDatePicker(datePickerHandler).Text;
			DateTime.TryParse(dateString, out DateTime result);

			return result;
		}

		Color GetNativeTextColor(DatePickerHandler datePickerHandler)
		{
			int currentTextColorInt = GetNativeDatePicker(datePickerHandler).CurrentTextColor;
			AColor currentTextColor = new AColor(currentTextColorInt);
			return currentTextColor.ToColor();
		}

		long GetNativeMinimumDate(DatePickerHandler datePickerHandler)
		{
			var dialog = datePickerHandler.DatePickerDialog;
			var minDate = dialog.DatePicker.MinDate;

			return minDate;
		}

		long GetNativeMaximumDate(DatePickerHandler datePickerHandler)
		{
			var dialog = datePickerHandler.DatePickerDialog;
			var maxDate = dialog.DatePicker.MaxDate;

			return maxDate;
		}

		double GetNativeCharacterSpacing(DatePickerHandler datePickerHandler)
		{
			var mauiDatePicker = GetNativeDatePicker(datePickerHandler);
			return mauiDatePicker.LetterSpacing;
		}


		/// <summary>
		/// The DatePickerDialog is lazily created after PR #33687 — setting MinimumDate or
		/// MaximumDate calls DestroyDialog() which nulls the dialog reference. The dialog is
		/// only recreated when ShowPickerDialog() runs. This helper opens and immediately
		/// closes the picker to force dialog creation so that min/max values can be read
		/// from the native DatePicker widget.
		/// </summary>
		void EnsureDialogCreated(DatePickerStub datePicker, DatePickerHandler handler)
		{
			datePicker.IsOpen = true;
			handler.UpdateValue(nameof(IDatePicker.IsOpen));
			datePicker.IsOpen = false;
			handler.UpdateValue(nameof(IDatePicker.IsOpen));
		}
	}
}