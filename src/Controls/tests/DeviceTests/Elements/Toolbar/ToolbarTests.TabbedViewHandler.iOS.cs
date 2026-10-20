using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Reuses every ToolbarTests fact/theory under TabbedViewHandler instead of the base class's
	/// TabbedRenderer, so tests that embed a TabbedPage exercise both variants on iOS/MacCatalyst.
	/// </summary>
	[Category(TestCategory.Toolbar)]
	[Trait(RendererHandlerVariant.TabbedViewVariantTraitName, RendererHandlerVariant.TabbedViewHandler)] // See RendererHandlerVariant.cs
	public class ToolbarTests_TabbedViewHandler : ToolbarTests
	{
		protected override void RegisterTabbedPageHandler(IMauiHandlersCollection handlers)
		{
			handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
		}
	}
}
