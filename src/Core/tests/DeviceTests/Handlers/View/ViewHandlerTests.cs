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
		[Fact(DisplayName = "NativeArrange triggers MapFrame")]
		public async Task NativeArrangeTriggersMapFrame()
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

		[Fact(DisplayName = "Subsequint NativeArrange triggers MapFrame")]
		public async Task SubsequintNativeArrangeTriggersMapFrame()
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

				handler.NativeArrange(new Rectangle(0, 0, 100, 100));
			});

			Assert.Equal(2, didUpdateFrame);
		}
	}
}