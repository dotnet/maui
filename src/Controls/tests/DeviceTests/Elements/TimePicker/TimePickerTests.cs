using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TimePicker)]
	public partial class TimePickerTests : HandlerTestBase
	{
		[Theory]
		[ClassData(typeof(TimePickerTextTransformCases))]
		public async Task TextTransformApplied(TimeSpan time, string format, TextTransform transform, string expected)
		{
			var timePicker = new TimePicker()
			{ 
				Time = time,
				Format = format,
				TextTransform = transform 
			};
		
			var platformText = await GetPlatformText(await CreateHandlerAsync<TimePickerHandler>(timePicker));

			Assert.Equal(expected, platformText);
		}
	}
}