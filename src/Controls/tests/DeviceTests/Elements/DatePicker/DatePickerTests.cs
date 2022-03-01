using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.DatePicker)]
	public partial class DatePickerTests : HandlerTestBase
	{
		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformApplied(string text, TextTransform transform, string expected)
		{
			var datePicker = new DatePicker() { TextTransform = transform };
			((IDatePicker)datePicker).Text = text;
			var platformText = await GetPlatformText(await CreateHandlerAsync<DatePickerHandler>(datePicker));

			Assert.Equal(expected, platformText);
		}
	}
}