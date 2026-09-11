using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Reuses every FlyoutPageTests fact/theory under FlyoutViewHandler instead of the base
	/// class's PhoneFlyoutPageRenderer, so all FlyoutPage tests run against both variants on
	/// iOS/MacCatalyst.
	/// </summary>
	[Category(TestCategory.FlyoutPage)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.FlyoutViewVariantTraitName, RendererHandlerVariant.FlyoutViewHandler)] // See RendererHandlerVariant.cs
	public class FlyoutPageHandlerTests : FlyoutPageTests
	{
		protected override void RegisterFlyoutPageHandler(IMauiHandlersCollection handlers)
		{
			handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
		}

		protected override UIView FindPlatformFlyoutView(UIView uiView) =>
			uiView.FindResponder<FlyoutContainerViewController>()?.View;
	}
}
