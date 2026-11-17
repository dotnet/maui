using Xunit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	// Forces every VisualElementTests.NewWindowCollection test method to exercise
	// NavigationViewHandler instead of the default NavigationRenderer, so those tests run against
	// both the NavigationPage renderer and handler on iOS/MacCatalyst. See RendererHandlerVariant.cs.
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Category(TestCategory.Lifecycle)]
	[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationViewHandler)] // See RendererHandlerVariant.cs
	public class VisualElementNewWindowNavigationHandlerTests : VisualElementTests.NewWindowCollection
	{
		protected override void RegisterNavigationPageHandler(IMauiHandlersCollection handlers) =>
			handlers.AddHandler<NavigationPage, NavigationViewHandler>();
	}
}
