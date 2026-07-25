#if MACCATALYST
using System;
using System.Threading.Tasks;
using Foundation;
using Microsoft.Maui.DeviceTests.Stubs;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.DatePicker)]
	public partial class DatePickerHandlerTests : CoreHandlerTestBase
	{
		[Fact(DisplayName = "Default Handler Uses Public Command Mapper")]
		public void DefaultHandlerUsesPublicCommandMapper()
		{
			var handler = new DatePickerHandler();

			Assert.Same(DatePickerHandler.CommandMapper, handler._commandMapper);
		}

		[Fact(DisplayName = "Focus Opens DatePicker When Virtual Focus Is Stale")]
		public async Task FocusOpensDatePickerWhenVirtualFocusIsStale()
		{
			var datePicker = new DatePickerStub
			{
				Date = new DateTime(2026, 5, 20),
				Width = 200,
				Height = 44
			};

			await AttachAndRun<DatePickerHandler>(datePicker, async handler =>
			{
				datePicker.IsFocused = true;
				datePicker.IsOpen = false;

				var focusResult = handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());

				Assert.True(focusResult);
				await AssertEventually(
					() => datePicker.IsFocused && datePicker.IsOpen,
					message: "DatePicker focus did not open when virtual focus was stale.");
			});
		}

		[Fact(DisplayName = "Focus Can Be Repeated After Unfocus")]
		public async Task FocusCanBeRepeatedAfterUnfocus()
		{
			var datePicker = new DatePickerStub
			{
				Date = new DateTime(2026, 5, 20),
				Width = 200,
				Height = 44
			};

			await AttachAndRun<DatePickerHandler>(datePicker, async handler =>
			{
				await AssertFocusCycle(datePicker, handler);
				await AssertFocusCycle(datePicker, handler);
			});
		}

		[Fact(DisplayName = "Focus Can Be Repeated While Open")]
		public async Task FocusCanBeRepeatedWhileOpen()
		{
			var datePicker = new DatePickerStub
			{
				Date = new DateTime(2026, 5, 20),
				Width = 200,
				Height = 44
			};

			await AttachAndRun<DatePickerHandler>(datePicker, async handler =>
			{
				var firstFocusResult = handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());

				Assert.True(firstFocusResult);
				await AssertEventually(
					() => datePicker.IsFocused && datePicker.IsOpen,
					message: "DatePicker did not enter the focused/open state.");

				var secondFocusResult = handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());

				Assert.True(secondFocusResult);
				await AssertEventually(
					() => datePicker.IsFocused && datePicker.IsOpen,
					message: "DatePicker did not remain in the focused/open state after repeated focus.");
			});
		}

		[Fact(DisplayName = "IsOpen Can Reopen DatePicker")]
		public async Task IsOpenCanReopenDatePicker()
		{
			var datePicker = new DatePickerStub
			{
				Date = new DateTime(2026, 5, 20),
				Width = 200,
				Height = 44
			};

			await AttachAndRun<DatePickerHandler>(datePicker, async handler =>
			{
				await AssertOpenState(datePicker, handler, true);
				await AssertOpenState(datePicker, handler, false);
				await AssertOpenState(datePicker, handler, true);
			});
		}

		[Fact(DisplayName = "Native Focus Refusal Keeps DatePicker Closed")]
		public async Task NativeFocusRefusalKeepsDatePickerClosed()
		{
			var datePicker = new DatePickerStub
			{
				Date = new DateTime(2026, 5, 20),
				Width = 200,
				Height = 44
			};

			DatePickerHandler.PlatformViewFactory = _ => new NonFocusableDatePicker();

			try
			{
				await AttachAndRun<DatePickerHandler>(datePicker, async handler =>
				{
					var focusResult = handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());

					Assert.False(focusResult);
					Assert.False(handler.PlatformView.IsFirstResponder);
					Assert.False(datePicker.IsFocused);
					Assert.False(datePicker.IsOpen);

					datePicker.IsOpen = true;
					handler.UpdateValue(nameof(IDatePicker.IsOpen));

					await AssertEventually(
						() => !handler.PlatformView.IsFirstResponder && !datePicker.IsFocused && !datePicker.IsOpen,
						message: "DatePicker should remain closed when native focus is refused.");
				});
			}
			finally
			{
				DatePickerHandler.PlatformViewFactory = null;
			}
		}

		[Fact(DisplayName = "Native Close Runs After Accepted Open Without First Responder")]
		public async Task NativeCloseRunsAfterAcceptedOpenWithoutFirstResponder()
		{
			var datePicker = new DatePickerStub
			{
				Date = new DateTime(2026, 5, 20),
				Width = 200,
				Height = 44
			};
			var platformView = new NonFirstResponderOpenDatePicker();

			DatePickerHandler.PlatformViewFactory = _ => platformView;

			try
			{
				await AttachAndRun<DatePickerHandler>(datePicker, async handler =>
				{
					await AssertOpenState(datePicker, handler, true);

					Assert.Equal(1, platformView.BecomeFirstResponderCount);
					Assert.False(handler.PlatformView.IsFirstResponder);
					Assert.Equal(0, platformView.ResignFirstResponderCount);

					await AssertOpenState(datePicker, handler, false);

					Assert.False(datePicker.IsFocused);
					Assert.False(datePicker.IsOpen);
					Assert.Equal(1, platformView.ResignFirstResponderCount);
				});
			}
			finally
			{
				DatePickerHandler.PlatformViewFactory = null;
			}
		}

		static async Task AssertFocusCycle(DatePickerStub datePicker, DatePickerHandler handler)
		{
			var focusResult = handler.InvokeWithResult(nameof(IView.Focus), new FocusRequest());

			Assert.True(focusResult);
			await AssertEventually(
				() => datePicker.IsFocused && datePicker.IsOpen,
				message: "DatePicker did not enter the focused/open state.");

			handler.Invoke(nameof(IView.Unfocus), null);

			await AssertEventually(
				() => !datePicker.IsFocused && !datePicker.IsOpen,
				message: "DatePicker did not leave the focused/open state.");
		}

		static async Task AssertOpenState(DatePickerStub datePicker, DatePickerHandler handler, bool isOpen)
		{
			datePicker.IsOpen = isOpen;
			handler.UpdateValue(nameof(IDatePicker.IsOpen));

			await AssertEventually(
				() => datePicker.IsFocused == isOpen && datePicker.IsOpen == isOpen,
				message: $"DatePicker did not update focused/open state to {isOpen}.");
		}

		class NonFocusableDatePicker : UIDatePicker
		{
			public NonFocusableDatePicker()
			{
				Mode = UIDatePickerMode.Date;
				TimeZone = new NSTimeZone("UTC");
			}

			public override bool CanBecomeFirstResponder => false;

			public override bool BecomeFirstResponder() => false;
		}

		class NonFirstResponderOpenDatePicker : UIDatePicker
		{
			public NonFirstResponderOpenDatePicker()
			{
				Mode = UIDatePickerMode.Date;
				TimeZone = new NSTimeZone("UTC");
			}

			public int BecomeFirstResponderCount { get; private set; }

			public int ResignFirstResponderCount { get; private set; }

			public override bool BecomeFirstResponder()
			{
				BecomeFirstResponderCount++;
				return true;
			}

			public override bool ResignFirstResponder()
			{
				ResignFirstResponderCount++;
				return true;
			}
		}
	}
}
#endif
