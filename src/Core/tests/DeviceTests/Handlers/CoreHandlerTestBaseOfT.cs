using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	public abstract partial class CoreHandlerTestBase<THandler, TStub> : HandlerTestBase<THandler, TStub>
		where THandler : class, IViewHandler, new()
		where TStub : StubBase, IView, new()
	{
		protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder) =>
			mauiAppBuilder.ConfigureTestBuilder();
	}
}