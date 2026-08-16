#if !MACCATALYST
using System;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TimePicker)]
	public class Issue37407 : CoreHandlerTestBase<TimePickerHandler, TimePickerStub>
	{
		[Fact]
		public async Task DefaultFormatUsesCurrentCulture()
		{
			var timePicker = new TimePickerStub
			{
				Format = "t",
				Time = new TimeSpan(7, 30, 0)
			};

			var nativeText = await GetValueAsync(timePicker, handler => handler.PlatformView.Text);

			Assert.True(
				string.Equals(nativeText, "07:30", StringComparison.Ordinal),
				$"Issue37407: expected fr-FR default TimePicker text '07:30'; actual '{nativeText}'");
		}
	}
}
#endif
