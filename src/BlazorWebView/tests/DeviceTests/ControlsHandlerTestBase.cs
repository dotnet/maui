using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ControlsHandlerTestBase : HandlerTestBase
	{
		protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder)
		{
			mauiAppBuilder.Services.AddSingleton<IApplication>((_) => new Application());
			return mauiAppBuilder.ConfigureTestBuilder();
		}

		protected THandler CreateHandler<THandler>(IElement view)
			where THandler : IElementHandler, new()
		{
			return CreateHandler<THandler>(view, MauiContext);
		}
	}
}
