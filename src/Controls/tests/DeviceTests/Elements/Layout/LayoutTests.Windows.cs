using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Automation.Peers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class LayoutTests
	{
		void ValidateInputTransparentOnPlatformView(IView view)
		{
			var handler = (IPlatformViewHandler)view.ToHandler(MauiContext);

			if (handler.PlatformView is LayoutPanel lp)
			{
				Assert.True(lp.IsHitTestVisible);
			}
			else
			{
				Assert.Equal(view.InputTransparent, !handler.PlatformView.IsHitTestVisible);
			}
		}

		void SetupLayoutBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
					handlers.AddHandler<HorizontalStackLayout, LayoutHandler>();
					handlers.AddHandler<AbsoluteLayout, LayoutHandler>();
					handlers.AddHandler<FlexLayout, LayoutHandler>();
					handlers.AddHandler<StackLayout, LayoutHandler>();
				});
			});
		}

		[Fact(DisplayName = "LayoutPanel creates a MauiLayoutAutomationPeer")]
		public async Task LayoutPanelCreatesMauiLayoutAutomationPeer()
		{
			SetupLayoutBuilder();

			var grid = new Grid();

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.IsType<MauiLayoutAutomationPeer>(peer);
			});
		}

		[Fact(DisplayName = "LayoutPanel AutomationPeer control type is Pane")]
		public async Task LayoutPanelAutomationPeerControlTypeIsPane()
		{
			SetupLayoutBuilder();

			var grid = new Grid();

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
			});
		}

		[Theory(DisplayName = "LayoutPanel class name reflects cross-platform layout type")]
		[InlineData(typeof(Grid), "Grid")]
		[InlineData(typeof(VerticalStackLayout), "VerticalStackLayout")]
		[InlineData(typeof(HorizontalStackLayout), "HorizontalStackLayout")]
		[InlineData(typeof(AbsoluteLayout), "AbsoluteLayout")]
		[InlineData(typeof(FlexLayout), "FlexLayout")]
		[InlineData(typeof(StackLayout), "StackLayout")]
		public async Task LayoutPanelClassNameReflectsCrossPlatformType(Type layoutType, string expectedClassName)
		{
			SetupLayoutBuilder();

			var layout = (Layout)Activator.CreateInstance(layoutType)!;

			// FlexLayout._root is only initialized when it has a MAUI parent.
			// Wrap it in a VerticalStackLayout so OnParentSet() fires before layout runs.
			Layout root = layout is FlexLayout
				? new VerticalStackLayout { layout }
				: layout;

			await AttachAndRun(root, (LayoutHandler handler) =>
			{
				// For FlexLayout, find the inner FlexLayout's platform view via its handler
				var targetView = layout is FlexLayout
					? (layout.Handler as LayoutHandler)?.PlatformView ?? handler.PlatformView
					: handler.PlatformView;

				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(targetView);
				Assert.Equal(expectedClassName, peer.GetClassName());
			});
		}

		[Theory(DisplayName = "LayoutPanel with AutomationId is exposed in automation tree")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task LayoutPanelWithAutomationIdIsExposedInTree(bool hasAutomationId)
		{
			SetupLayoutBuilder();

			var grid = new Grid();
			if (hasAutomationId)
				grid.AutomationId = "TestGrid";

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.Equal(hasAutomationId, peer.IsContentElement());
			});
		}

		[Fact(DisplayName = "LayoutPanel AutomationPeer is not keyboard focusable")]
		public async Task LayoutPanelAutomationPeerIsNotKeyboardFocusable()
		{
			SetupLayoutBuilder();

			var grid = new Grid();

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.False(peer.IsKeyboardFocusable());
			});
		}

		[Fact(DisplayName = "LayoutPanel IsControlElement and IsContentElement update when AutomationId is set at runtime")]
		public async Task LayoutPanelAutomationPeerUpdatesWhenAutomationIdChangesAtRuntime()
		{
			SetupLayoutBuilder();

			var grid = new Grid();

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);

				// Initially no AutomationId — should NOT be exposed
				Assert.False(peer.IsContentElement());

				// Set AutomationId at runtime — should now be exposed.
				// Note: MAUI AutomationId is write-once (Element.cs enforces this),
				// so we can only test the transition from unset → set, not set → cleared.
				grid.AutomationId = "DynamicGrid";
				Assert.True(peer.IsContentElement());
			});
		}
	}
}


