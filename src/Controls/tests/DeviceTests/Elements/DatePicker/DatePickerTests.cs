using System;
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
		[ClassData(typeof(DatePickerTextTransformCases))]
		public async Task TextTransformApplied(DateTime date, string format, TextTransform transform, string expected)
		{
			var datePicker = new DatePicker()
			{
				Date = date,
				Format = format,
				TextTransform = transform
			};

			var platformText = await GetPlatformText(await CreateHandlerAsync<DatePickerHandler>(datePicker));

			Assert.Equal(expected, platformText);
		}
	}
}