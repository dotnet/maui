using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TimePicker)]
	public partial class TimePickerHandlerTests : HandlerTestBase<TimePickerHandler, TimePickerStub>
	{
		[Fact(DisplayName = "Time Initializes Correctly")]
		public async Task IsToggledInitializesCorrectly()
		{
			var timePickerStub = new TimePickerStub();

			var time = new TimeSpan(17, 0, 0);

			await ValidateTime(timePickerStub, () => timePickerStub.Time = time);
		}
	}
}
