using Xunit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	// Overrides ShellTests to exercise NavigationViewHandler instead of the default
	// NavigationRenderer, covering both variants on iOS/MacCatalyst.
	[Category(TestCategory.Shell)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationViewHandler)] // See RendererHandlerVariant.cs
	public class ShellNavigationHandlerTests : ShellTests
	{
		protected override void RegisterNavigationPageHandler(IMauiHandlersCollection handlers) =>
			handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
	}
}
