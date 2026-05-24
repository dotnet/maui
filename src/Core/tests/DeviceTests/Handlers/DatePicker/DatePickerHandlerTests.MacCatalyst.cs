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
	}
}
#endif
