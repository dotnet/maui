using Xunit;
using System;
using Microsoft.Maui.Hosting;

namespace Microsoft.Maui.DeviceTests
{
	// Forces every TabbedPageTests test method to exercise NavigationViewHandler instead of the
	// default NavigationRenderer, so all TabbedPage tests run against both the NavigationPage
	// renderer and handler on iOS/MacCatalyst. See RendererHandlerVariant.cs.
	[Category(TestCategory.TabbedPage)]
	[Trait(RendererHandlerVariant.NavigationViewVariantTraitName, RendererHandlerVariant.NavigationViewHandler)] // See RendererHandlerVariant.cs
	public class TabbedPageNavigationHandlerTests : TabbedPageTests
	{
		protected override void SetupBuilder(Action<MauiAppBuilder> additionalCreationActions = null, bool includeNavigationViewHandler = false) =>
			base.SetupBuilder(additionalCreationActions, includeNavigationViewHandler: true);
	}
}
