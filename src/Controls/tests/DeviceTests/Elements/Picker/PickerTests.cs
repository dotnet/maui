using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Picker)]
	public partial class PickerTests : HandlerTestBase
	{
		[Theory]
		[ClassData(typeof(TextTransformCases))]
		public async Task TextTransformApplied(string text, TextTransform transform, string expected)
		{
			var picker = new Picker() { TextTransform = transform };
			((IPicker)picker).Text = text;
			var platformText = await GetPlatformText(await CreateHandlerAsync<PickerHandler>(picker));

			Assert.Equal(expected, platformText);
		}
	}
}
