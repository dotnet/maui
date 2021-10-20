using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Label)]
	public partial class LabelTests : HandlerTestBase
	{
		[Theory]
		[InlineData("Hello There", TextTransform.None, "Hello There")]
		[InlineData("Hello There", TextTransform.Uppercase, "HELLO THERE")]
		[InlineData("Hello There", TextTransform.Lowercase, "hello there")]
		public async Task TextTransformApplied(string text, TextTransform transform, string expected)
		{
			var label = new Label() { Text = text, TextTransform = transform };

			var handler = await CreateHandlerAsync<LabelHandler>(label);

			var nativeText = await InvokeOnMainThreadAsync(() =>
			{
				return handler.NativeView.Text;
			});

			Assert.Equal(expected, nativeText);
		}
	}
}