using Xunit;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
namespace Microsoft.Maui.DeviceTests
{
	// Forces every ModalTests test method to exercise NavigationViewHandler instead of the
	// default NavigationRenderer, so all Modal tests run against both the NavigationPage renderer
	// and handler on iOS/MacCatalyst. See RendererHandlerVariant.cs. (ModalTests already carries a
	// [Trait] for an unrelated Android Shell renderer/handler axis, so it is intentionally not
	// repeated here to avoid conflating the two axes.)
	[Category(TestCategory.Modal)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationViewHandler)] // See RendererHandlerVariant.cs
	public class ModalNavigationHandlerTests : ModalTests
	{
		protected override void RegisterNavigationPageHandler(IMauiHandlersCollection handlers) =>
			handlers.AddHandler(typeof(NavigationPage), typeof(NavigationViewHandler));
	}
}
