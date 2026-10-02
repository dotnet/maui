using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Memory;

/// <summary>
/// Reuses every MemoryTests fact/theory under FlyoutViewHandler instead of the base class's
/// PhoneFlyoutPageRenderer, so PagesDoNotLeak(typeof(FlyoutPage)) exercises both variants on
/// iOS/MacCatalyst.
/// </summary>
[Category(TestCategory.Memory)]
[Trait(RendererHandlerVariant.FlyoutViewVariantTraitName, RendererHandlerVariant.FlyoutViewHandler)] // See RendererHandlerVariant.cs
public class MemoryTests_FlyoutViewHandler : MemoryTests
{
	protected override void RegisterFlyoutPageHandler(IMauiHandlersCollection handlers)
	{
		handlers.AddHandler<FlyoutPage, FlyoutViewHandler>();
	}
}
