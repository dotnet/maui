using System.Threading.Tasks;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Automation.Peers;
using Xunit;
using UIAutomationProperties = Microsoft.UI.Xaml.Automation.AutomationProperties;

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

		[Fact("ContentView without AutomationId is not promoted in accessibility tree")]
		public async Task ContentViewWithoutAutomationIdIsNotPromoted()
		{
			SetupBuilder();

			var contentView = new ContentView
			{
				Content = new Label { Text = "content" }
			};

			var handler = await CreateHandlerAsync<ContentViewHandler>(contentView);

			await InvokeOnMainThreadAsync(() =>
			{
				// Without AutomationId, the platform default should remain (not promoted to Content)
				var accessibilityView = UIAutomationProperties.GetAccessibilityView(handler.PlatformView);
				Assert.NotEqual(AccessibilityView.Content, accessibilityView);
			});
		}

		[Fact("AutomationId makes ContentView visible in accessibility tree")]
		public async Task ContentViewWithAutomationIdIsInAccessibilityTree()
		{
			SetupBuilder();

			var contentView = new ContentView
			{
				AutomationId = "contentViewAutomationId",
				Content = new Label { Text = "content" }
			};

			var handler = await CreateHandlerAsync<ContentViewHandler>(contentView);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(
					AccessibilityView.Content,
					UIAutomationProperties.GetAccessibilityView(handler.PlatformView));
			});
		}

		[Fact("AutomationId does not override explicit IsInAccessibleTree false")]
		public async Task ContentViewWithAutomationIdHonorsIsInAccessibleTreeFalse()
		{
			SetupBuilder();

			var contentView = new ContentView
			{
				AutomationId = "contentViewAutomationId",
				Content = new Label { Text = "content" }
			};

			Microsoft.Maui.Controls.AutomationProperties.SetIsInAccessibleTree(contentView, false);
			var handler = await CreateHandlerAsync<ContentViewHandler>(contentView);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.Equal(
					AccessibilityView.Raw,
					UIAutomationProperties.GetAccessibilityView(handler.PlatformView));
			});
		}
	}
}
