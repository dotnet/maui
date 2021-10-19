using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.VisualElement)]
	public partial class VisualElementTests : HandlerTestBase
	{
		[Fact]
		public async Task CanCreateHandler()
		{
			var image = new Image();

			await CreateHandlerAsync<ImageHandler>(image);
		}

		[Fact]
		public async Task SettingHandlerDoesNotThrow()
		{
			var image = new Image();

			var handler = await CreateHandlerAsync<ImageHandler>(image);

			image.Handler = handler;
		}
	}
}