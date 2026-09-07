using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Reuses every ToolbarTests fact/theory under FlyoutViewHandler instead of the base class's
	/// PhoneFlyoutPageRenderer, so tests that embed a FlyoutPage exercise both variants on
	/// iOS/MacCatalyst.
	/// </summary>
	[Category(TestCategory.Toolbar)]
	[Trait(RendererHandlerVariant.FlyoutViewVariantTraitName, RendererHandlerVariant.FlyoutViewHandler)] // See RendererHandlerVariant.cs
	public class ToolbarTests_FlyoutViewHandler : ToolbarTests
	{
		protected override void RegisterFlyoutPageHandler(IMauiHandlersCollection handlers)
		{
			handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
		}
	}
}
