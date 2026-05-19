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
		public async Task LayoutPanelClassNameReflectsCrossPlatformType(Type layoutType, string expectedClassName)
		{
			SetupLayoutBuilder();

			var layout = (Layout)Activator.CreateInstance(layoutType)!;

			await AttachAndRun(layout, (LayoutHandler handler) =>
			{
				var peer = FrameworkElementAutomationPeer.CreatePeerForElement(handler.PlatformView);
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
				Assert.Equal(hasAutomationId, peer.IsControlElement());
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
	}
}


