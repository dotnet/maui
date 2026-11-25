#if MACCATALYST
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.View)]
	public class ContextFlyoutInteractionTests : HandlerTestBasement
	{
		[Fact]
		public async Task MapContextFlyoutKeepsOneInteractionAttached()
		{
			EnsureHandlerCreated();

			await InvokeOnMainThreadAsync(() =>
			{
				var element = new ContextFlyoutElement { ContextFlyout = new FlyoutElement() };
				var handler = new CountingHandler();
				handler.SetMauiContext(MauiContext);
				handler.SetVirtualView(new StubBase());
				var platformView = handler.PlatformView;

				ViewHandler.MapContextFlyout(handler, element);
				var first = Assert.Single(platformView.ContextMenuInteractions());
				var readsAfterFirstMap = platformView.Reads;

				ViewHandler.MapContextFlyout(handler, element);

				Assert.Equal(readsAfterFirstMap, platformView.Reads);
				Assert.Same(first, Assert.Single(platformView.ContextMenuInteractions()));

				platformView.RemoveInteraction((IUIInteraction)first);
				ViewHandler.MapContextFlyout(handler, element);

				Assert.NotSame(first, Assert.Single(platformView.ContextMenuInteractions()));

				element.ContextFlyout = null;
				ViewHandler.MapContextFlyout(handler, element);

				Assert.Empty(platformView.ContextMenuInteractions());
			});
		}

		class ContextFlyoutElement : IContextFlyoutElement
		{
			public IFlyout ContextFlyout { get; set; }
		}

		class FlyoutElement : ElementStub, IFlyout
		{
		}

		class CountingPlatformView : UIView
		{
			public int Reads { get; private set; }

			public override IUIInteraction[] Interactions
			{
				get
				{
					Reads++;
					return base.Interactions;
				}
			}

			public MauiUIContextMenuInteraction[] ContextMenuInteractions() =>
				base.Interactions.OfType<MauiUIContextMenuInteraction>().ToArray();
		}

		class CountingHandler : ViewHandler<IView, CountingPlatformView>
		{
			public CountingHandler() : base(ViewMapper)
			{
			}

			protected override CountingPlatformView CreatePlatformView() => new CountingPlatformView();
		}
	}
}
#endif
