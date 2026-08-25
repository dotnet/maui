using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Reuses every WindowTests fact/theory under FlyoutViewHandler instead of the base class's
	/// PhoneFlyoutPageRenderer, so tests that embed a FlyoutPage exercise both variants on
	/// iOS/MacCatalyst.
	/// </summary>
	[Category(TestCategory.Window)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.FlyoutViewVariantTraitName, RendererHandlerVariant.FlyoutViewHandler)] // See RendererHandlerVariant.cs
	public class WindowTests_FlyoutViewHandler : WindowTests
	{
		protected override void RegisterFlyoutPageHandler(IMauiHandlersCollection handlers)
		{
			handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
		}
	}
}
