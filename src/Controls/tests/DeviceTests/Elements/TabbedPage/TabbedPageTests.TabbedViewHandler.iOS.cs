using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Reuses every TabbedPageTests fact/theory under TabbedViewHandler instead of the base
	/// class's TabbedRenderer. Same pattern as ShellHandlerSubclasses.Android.cs.
	/// </summary>
	[Category(TestCategory.TabbedPage)]
	[Trait(RendererHandlerVariant.TabbedViewVariantTraitName, RendererHandlerVariant.TabbedViewHandler)] // See RendererHandlerVariant.cs
	public class TabbedPageTests_TabbedViewHandler : TabbedPageTests
	{
		protected override void RegisterTabbedPageHandler(IMauiHandlersCollection handlers)
		{
			handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
		}
	}
}
