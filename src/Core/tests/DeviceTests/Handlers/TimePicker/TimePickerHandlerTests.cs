#if !MACCATALYST
using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TimePicker)]
	public partial class TimePickerHandlerTests : HandlerTestBase<TimePickerHandler, TimePickerStub>
	{
		[Fact(DisplayName = "Time Initializes Correctly")]
		public async Task TimeInitializesCorrectly()
		{
			var timePicker = new TimePickerStub
			{
				Format = "HH:mm"
			};

			var time = new TimeSpan(17, 0, 0);

			await ValidateTime(timePicker, () => timePicker.Time = time);
		}

		[Fact(DisplayName = "Null Text Color Doesn't Crash")]
		public async Task NullTextColorDoesntCrash()
		{
			var timePicker = new TimePickerStub()
			{
				Time = DateTime.Now.TimeOfDay,
				TextColor = null
			};

			await CreateHandlerAsync(timePicker);
		}
	}
}
#endif