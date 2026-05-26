#if MACCATALYST
using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
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
	}
}
#endif
