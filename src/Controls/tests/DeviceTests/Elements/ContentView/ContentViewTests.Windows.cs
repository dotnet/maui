using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ContentViewTests
	{
		static int GetChildCount(ContentViewHandler contentViewHandler)
		{
			return contentViewHandler.PlatformView.Children.Count;
		}

		static int GetContentChildCount(ContentViewHandler contentViewHandler)
		{
			if (contentViewHandler.PlatformView.Children[0] is LayoutPanel childLayoutPanel)
				return childLayoutPanel.Children.Count;
			else
				return 0;
		}

		static FrameworkElementAutomationPeer GetOrCreateAutomationPeer(ContentPanel contentPanel)
		{
			var peer = FrameworkElementAutomationPeer.FromElement(contentPanel);
			
			if (peer == null)
			{
				peer = FrameworkElementAutomationPeer.CreatePeerForElement(contentPanel);
			}

			return (FrameworkElementAutomationPeer)peer;
		}

		[Fact(DisplayName = "ContentView With Description Prevents Duplicate Narrator Announcements")]
		public async Task ContentViewWithDescriptionHidesChildrenFromNarrator()
		{
			SetupBuilder();

			var label = new Label { Text = "Child Label Text" };
			var contentView = new ContentView { Content = label };
			SemanticProperties.SetDescription(contentView, "ContentView Description");

			await CreateHandlerAndAddToWindow<ContentViewHandler>(contentView, async (handler) =>
			{
				var contentPanel = handler.PlatformView as ContentPanel;
				Assert.NotNull(contentPanel);

				// Call UpdateSemantics manually because handler.UpdateValue uses handler.VirtualView
				// which doesn't have Semantics populated in the test context
				var view = contentView as IView;
				contentPanel.UpdateSemantics(view);

				var peer = GetOrCreateAutomationPeer(contentPanel);
				Assert.NotNull(peer);

				var name = Microsoft.UI.Xaml.Automation.AutomationProperties.GetName(contentPanel);
				Assert.Equal("ContentView Description", name);

				// ControlType should be Text (not Group) to prevent Narrator from saying "group"
				var controlType = peer.GetAutomationControlType();
				Assert.Equal(AutomationControlType.Text, controlType);

				// Children should be hidden to prevent duplicate announcements
				var children = peer.GetChildren();
				Assert.Null(children);

				// LocalizedControlType should be empty to prevent suffix
				var localizedControlType = peer.GetLocalizedControlType();
				Assert.Equal(string.Empty, localizedControlType);

				await Task.CompletedTask;
			});
		}

		[Fact(DisplayName = "ContentView Without Description Shows Children Normally")]
		public async Task ContentViewWithoutDescriptionShowsChildren()
		{
			SetupBuilder();

			var label = new Label { Text = "Child Label Text" };
			var contentView = new ContentView { Content = label };

			await CreateHandlerAndAddToWindow<ContentViewHandler>(contentView, async (handler) =>
			{
				var contentPanel = handler.PlatformView as ContentPanel;
				Assert.NotNull(contentPanel);

				var peer = GetOrCreateAutomationPeer(contentPanel);
				Assert.NotNull(peer);

				var controlType = peer.GetAutomationControlType();
				Assert.Equal(AutomationControlType.Custom, controlType);

				var children = peer.GetChildren();
				Assert.NotNull(children);
				Assert.NotEmpty(children);

				await Task.CompletedTask;
			});
		}
	}
}
