using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Automation.Peers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class BorderHandlerTests
	{
		ContentPanel GetNativeBorder(BorderHandler borderHandler) =>
			borderHandler.PlatformView;

		[Fact]
		public async Task AutomationPeerIsCreated()
		{
			var border = new BorderStub();
			
			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(border);
				var platformView = GetNativeBorder(handler);
				var automationPeer = FrameworkElementAutomationPeer.CreatePeerForElement(platformView);
				
				Assert.NotNull(automationPeer);
				Assert.IsType<ContentPanelAutomationPeer>(automationPeer);
			});
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task FocusabilityBasedOnSemantics(bool hasSemantics)
		{
			var border = new BorderStub();
			
			if (hasSemantics)
			{
				border.Semantics = new Semantics { Description = "Test Border" };
			}
			
			await InvokeOnMainThreadAsync(() =>
			{
				var handler = CreateHandler(border);
				var platformView = GetNativeBorder(handler);
				
				// Trigger semantic update
				platformView.UpdateSemantics(border);
				
				Assert.Equal(hasSemantics, platformView.Focusable);
				Assert.Equal(hasSemantics, platformView.IsTabStop);
			});
		}
	}
}