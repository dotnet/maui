using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	/// <summary>
	/// Reuses every ModalTests fact/theory under FlyoutViewHandler instead of the base class's
	/// PhoneFlyoutPageRenderer, so tests that embed a FlyoutPage exercise both variants on
	/// iOS/MacCatalyst.
	/// </summary>
	[Category(TestCategory.Modal)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	[Trait(RendererHandlerVariant.FlyoutViewVariantTraitName, RendererHandlerVariant.FlyoutViewHandler)] // See RendererHandlerVariant.cs
	public class ModalTests_FlyoutViewHandler : ModalTests
	{
		protected override void RegisterFlyoutPageHandler(IMauiHandlersCollection handlers)
		{
			handlers.AddHandler(typeof(FlyoutPage), typeof(FlyoutViewHandler));
		}
	}
}
