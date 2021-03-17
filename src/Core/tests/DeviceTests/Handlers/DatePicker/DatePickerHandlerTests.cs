using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.DatePicker)]
	public partial class DatePickerHandlerTests : HandlerTestBase<DatePickerHandler, DatePickerStub>
	{
		public DatePickerHandlerTests(HandlerTestFixture fixture) : base(fixture)
		{
		}

		[Fact(DisplayName = "Date Initializes Correctly")]
		public async Task TextInitializesCorrectly()
		{
			var datePicker = new DatePickerStub()
			{
				Date = DateTime.Today
			};

			await ValidatePropertyInitValue(datePicker, () => datePicker.Date, GetNativeDate, datePicker.Date);
		}
	}
}