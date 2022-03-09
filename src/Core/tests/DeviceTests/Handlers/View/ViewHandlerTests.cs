using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.View)]
	public partial class ViewHandlerTests : HandlerTestBase<StubBaseHandler, StubBase>
	{
		[Fact(DisplayName = "PlatformArrange triggers MapFrame")]
		public async Task PlatformArrangeTriggersMapFrame()
		{
			var didUpdateFrame = 0;

			var view = new StubBase();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = new StubBaseHandler();

				handler.CommandMapper.AppendToMapping(nameof(IView.Frame), (h, v, a) =>
				{
					didUpdateFrame++;
				});

				InitializeViewHandler(view, handler);
			});

			Assert.Equal(1, didUpdateFrame);
		}

		[Fact(DisplayName = "Subsequent PlatformArrange triggers MapFrame")]
		public async Task SubsequentPlatformArrangeTriggersMapFrame()
		{
			var didUpdateFrame = 0;

			var view = new StubBase();

			await InvokeOnMainThreadAsync(() =>
			{
				var handler = new StubBaseHandler();

				handler.CommandMapper.AppendToMapping(nameof(IView.Frame), (h, v, a) =>
				{
					didUpdateFrame++;
				});

				InitializeViewHandler(view, handler);

				handler.PlatformArrange(new Rect(0, 0, 100, 100));
			});

			Assert.Equal(2, didUpdateFrame);
		}
	}
}