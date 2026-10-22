using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests.Memory;

/// <summary>
/// Reuses every MemoryTests fact/theory under TabbedViewHandler instead of the base class's
/// TabbedRenderer, so PagesDoNotLeak(typeof(TabbedPage)) exercises both variants on iOS/MacCatalyst.
/// </summary>
[Category(TestCategory.Memory)]
[Trait(RendererHandlerVariant.TabbedViewVariantTraitName, RendererHandlerVariant.TabbedViewHandler)] // See RendererHandlerVariant.cs
public class MemoryTests_TabbedViewHandler : MemoryTests
{
	protected override void RegisterTabbedPageHandler(IMauiHandlersCollection handlers)
	{
		handlers.AddHandler<TabbedPage, TabbedViewHandler>();
	}
}
