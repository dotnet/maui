using Xunit;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
namespace Microsoft.Maui.DeviceTests
{
	// Forces every VisualElementTreeTests test method to exercise NavigationViewHandler instead
	// of the default NavigationRenderer, so all VisualElementTree tests run against both the
	// NavigationPage renderer and handler on iOS/MacCatalyst. See RendererHandlerVariant.cs.
	[Category(TestCategory.VisualElementTree)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationViewHandler)] // See RendererHandlerVariant.cs
	public class VisualElementTreeNavigationHandlerTests : VisualElementTreeTests
	{
		protected override void RegisterNavigationPageHandler(IMauiHandlersCollection handlers) =>
			handlers.AddHandler(typeof(Controls.NavigationPage), typeof(NavigationViewHandler));
	}
}
