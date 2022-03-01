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
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformApplied(string text, TextTransform transform, string expected)
		{
			var timePicker = new TimePicker() { TextTransform = transform };
			((ITimePicker)timePicker).Text = text;
			var platformText = await GetPlatformText(await CreateHandlerAsync<TimePickerHandler>(timePicker));

			Assert.Equal(expected, platformText);
		}
	}
}