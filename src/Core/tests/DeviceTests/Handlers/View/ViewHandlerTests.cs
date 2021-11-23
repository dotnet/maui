using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui;
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
	}
}