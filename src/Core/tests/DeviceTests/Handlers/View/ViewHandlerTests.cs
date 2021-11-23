using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.View)]
	public partial class ViewHandlerTests : HandlerTestBase<StubBase, StubBaseHandler>
	{
		[Fact(DisplayName = "NativeArrange triggers MapFrame")]
		public Task NativeArrangeTriggersMapFrame()
		{
			var commandMapperField = typeof(ElementHandler).GetField("_commandMapper");

			var view = new StubBase();

			return InvokeOnMainThreadAsync(() =>
			{
				var handler = new StubBaseHandler(CreateNativeView());

				var viewHandler = handler as ViewHandler;
				var commandMapper = commandMapperField.GetValue(handler) as CommandMapper;



				handler.Com

				InitializeViewHandler(view, handler);

				var handler = CreateHandler(view);


			});
		}
	}
}