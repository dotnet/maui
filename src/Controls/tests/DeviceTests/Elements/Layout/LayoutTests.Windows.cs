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

		[Fact(DisplayName = "LayoutPanel AutomationPeer default control type is Custom with lowercase class name as localized type")]
		public async Task LayoutPanelAutomationPeerDefaultControlTypeIsCustom()
		{
			SetupLayoutBuilder();

			var grid = new Grid();

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.Equal(AutomationControlType.Custom, peer.GetAutomationControlType());
				// UIA spec requires Custom elements to have a non-empty LocalizedControlType.
				// For anonymous layouts, we return the lowercase cross-platform type name.
				Assert.Equal("grid", peer.GetLocalizedControlType());
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

		[Theory(DisplayName = "LayoutPanel AutomationId is exposed via the automation peer")]
		[InlineData(true)]
		[InlineData(false)]
		public async Task LayoutPanelAutomationIdIsExposedViaPeer(bool hasAutomationId)
		{
			SetupLayoutBuilder();

			var grid = new Grid();
			if (hasAutomationId)
				grid.AutomationId = "TestGrid";

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				var expected = hasAutomationId ? "TestGrid" : string.Empty;
				Assert.Equal(expected, peer.GetAutomationId());
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

		[Fact(DisplayName = "LayoutPanel without automation signals is excluded from Control and Content views")]
		public async Task LayoutPanelWithoutAutomationSignalsIsExcludedFromControlAndContentViews()
		{
			SetupLayoutBuilder();

			var grid = new Grid();

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.False(peer.IsControlElement());
				Assert.False(peer.IsContentElement());
			});
		}

		[Fact(DisplayName = "LayoutPanel with AutomationId is included in Control view only")]
		public async Task LayoutPanelWithAutomationIdIsIncludedInControlViewOnly()
		{
			SetupLayoutBuilder();

			var grid = new Grid { AutomationId = "TestGrid" };

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);

				Assert.Equal("TestGrid", peer.GetAutomationId());
				Assert.True(peer.IsControlElement());
				Assert.False(peer.IsContentElement());
				Assert.Equal(AutomationControlType.Custom, peer.GetAutomationControlType());
				// UIA spec requires Custom elements to have a non-empty LocalizedControlType.
				Assert.Equal("grid", peer.GetLocalizedControlType());
			});
		}

		[Fact(DisplayName = "LayoutPanel opts into Control view when AutomationProperties.IsInAccessibleTree is true")]
		public async Task LayoutPanelOptsIntoControlViewViaIsInAccessibleTree()
		{
			SetupLayoutBuilder();

			var grid = new Grid();
			AutomationProperties.SetIsInAccessibleTree(grid, true);

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.True(peer.IsControlElement());
				Assert.True(peer.IsContentElement());
				Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
			});
		}

		[Fact(DisplayName = "LayoutPanel opts into Control view when SemanticProperties.Description is set")]
		public async Task LayoutPanelOptsIntoControlViewWhenDescriptionIsSet()
		{
			SetupLayoutBuilder();

			var grid = new Grid();
			SemanticProperties.SetDescription(grid, "Welcome card");

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.True(peer.IsControlElement());
				Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
			});
		}

		[Fact(DisplayName = "LayoutPanel opts into Control view when SemanticProperties.Hint is set")]
		public async Task LayoutPanelOptsIntoControlViewWhenHintIsSet()
		{
			SetupLayoutBuilder();

			var grid = new Grid();
			SemanticProperties.SetHint(grid, "Contains welcome card actions");

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.True(peer.IsControlElement());
				Assert.Equal(AutomationControlType.Pane, peer.GetAutomationControlType());
			});
		}

		[Fact(DisplayName = "LayoutPanel explicit accessible-tree opt out overrides AutomationId and SemanticProperties.Description")]
		public async Task LayoutPanelAccessibleTreeOptOutOverridesAutomationIdAndDescription()
		{
			SetupLayoutBuilder();

			var grid = new Grid { AutomationId = "TestGrid" };
			SemanticProperties.SetDescription(grid, "Welcome card");
			AutomationProperties.SetIsInAccessibleTree(grid, false);

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.Equal("TestGrid", peer.GetAutomationId());
				Assert.False(peer.IsControlElement());
				Assert.False(peer.IsContentElement());
			});
		}

		[Fact(DisplayName = "LayoutPanel ignores whitespace-only SemanticProperties.Description")]
		public async Task LayoutPanelDoesNotOptIntoControlViewWhenDescriptionIsWhitespace()
		{
			SetupLayoutBuilder();

			var grid = new Grid();
			SemanticProperties.SetDescription(grid, "   ");

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.False(peer.IsControlElement());
			});
		}

		[Fact(DisplayName = "LayoutPanel ignores whitespace-only SemanticProperties.Hint")]
		public async Task LayoutPanelDoesNotOptIntoControlViewWhenHintIsWhitespace()
		{
			SetupLayoutBuilder();

			var grid = new Grid();
			SemanticProperties.SetHint(grid, "   ");

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
				Assert.False(peer.IsControlElement());
			});
		}

		[Fact(DisplayName = "LayoutPanel AutomationId is exposed via the peer when set at runtime")]
		public async Task LayoutPanelAutomationPeerUpdatesWhenAutomationIdChangesAtRuntime()
		{
			SetupLayoutBuilder();

			var grid = new Grid();

			await AttachAndRun(grid, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);

				// Initially no AutomationId.
				Assert.Equal(string.Empty, peer.GetAutomationId());

				// Set AutomationId at runtime -- peer should reflect the new value.
				// Note: MAUI AutomationId is write-once (Element.cs enforces this),
				// so we can only test the transition from unset -> set, not set -> cleared.
				grid.AutomationId = "DynamicGrid";
				Assert.Equal("DynamicGrid", peer.GetAutomationId());
			});
		}
	}
}
