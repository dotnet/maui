using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Reuses every ModalTests fact/theory under TabbedViewHandler instead of the base class's
	/// TabbedRenderer, so tests that embed a TabbedPage exercise both variants on iOS/MacCatalyst.
	/// </summary>
	[Category(TestCategory.Modal)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.TabbedViewVariantTraitName, RendererHandlerVariant.TabbedViewHandler)] // See RendererHandlerVariant.cs
	public class ModalTests_TabbedViewHandler : ModalTests
	{
		protected override void RegisterTabbedPageHandler(IMauiHandlersCollection handlers)
		{
			handlers.AddHandler(typeof(TabbedPage), typeof(TabbedViewHandler));
		}
	}
}
