using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public partial class ButtonTests : HandlerTestBase
	{
		[Theory]
		[InlineData("Hello There", TextTransform.None, "Hello There")]
		[InlineData("Hello There", TextTransform.Uppercase, "HELLO THERE")]
		[InlineData("Hello There", TextTransform.Lowercase, "hello there")]
		public async Task TextTransformApplied(string text, TextTransform transform, string expected)
		{
			var control = new Button() { Text = text, TextTransform = transform };

			var handler = await CreateHandlerAsync<ButtonHandler>(control);

			var platformText = await GetNativeText(handler);

			Assert.Equal(expected, platformText);
		}
	}
}