using Xunit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	// Forces every CollectionViewTests test method to exercise NavigationViewHandler instead of
	// the default NavigationRenderer, so all CollectionView tests run against both the
	// NavigationPage renderer and handler on iOS/MacCatalyst. See RendererHandlerVariant.cs.
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Category(TestCategory.CollectionView)]
	[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationViewHandler)] // See RendererHandlerVariant.cs
	public class CollectionViewNavigationHandlerTests : CollectionViewTests
	{
		protected override void RegisterNavigationPageHandler(IMauiHandlersCollection handlers) =>
			handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
	}
}
