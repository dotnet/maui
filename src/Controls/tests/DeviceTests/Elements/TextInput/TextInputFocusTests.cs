using Microsoft.Maui.Controls;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.TextInput)]
	public abstract partial class TextInputFocusTests<THandler, TView> : ControlsHandlerTestBase
		where THandler : class, IViewHandler, IPlatformViewHandler, new()
		where TView : VisualElement, ITextInput, new()
	{
		protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder) =>
			base.ConfigureBuilder(mauiAppBuilder)
				.ConfigureMauiHandlers(handlers =>
					handlers.AddHandler<TView, THandler>());
	}
}
