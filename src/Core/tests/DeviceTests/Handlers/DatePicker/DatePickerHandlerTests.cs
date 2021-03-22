using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.DatePicker)]
	public partial class DatePickerHandlerTests : HandlerTestBase<DatePickerHandler, DatePickerStub>
	{
		[Fact(DisplayName = "Date Initializes Correctly")]
		public async Task DateInitializesCorrectly()
		{
			var datePicker = new DatePickerStub();

			datePicker.Date = DateTime.Today;

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, GetNativeDate, datePicker.Date);
		}
	}
}