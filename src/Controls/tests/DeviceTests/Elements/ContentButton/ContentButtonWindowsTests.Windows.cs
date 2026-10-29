#nullable enable
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Automation.Provider;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Button)]
	public class ContentButtonWindowsTests : ControlsHandlerTestBase
	{
		[Fact]
		public async Task SemanticDescriptionProducesSingleNativeButton()
		{
			var commandExecutions = 0;
			var clicked = 0;
			var contentButton = new ContentButton
			{
				Command = new Command(() => commandExecutions++),
				Content = new VerticalStackLayout
				{
					Children =
					{
						new Label { Text = "Presentation" },
						new Button { Text = "Nested presentation" }
					}
				}
			};
			SemanticProperties.SetDescription(contentButton, "Open details");
			contentButton.Clicked += (_, _) => clicked++;

			await AttachAndRun(contentButton, handler =>
			{
				var contentButtonHandler = Assert.IsType<ContentButtonHandler>(handler);
				var platformButton = Assert.IsType<MauiContentButton>(contentButtonHandler.PlatformView);
				var peer = new MauiButtonAutomationPeer(platformButton);
				var invokeProvider = Assert.IsAssignableFrom<IInvokeProvider>(peer.GetPattern(PatternInterface.Invoke));

				Assert.Equal("Open details", peer.GetName());
				Assert.Equal(AutomationControlType.Button, peer.GetAutomationControlType());
				Assert.Null(peer.GetChildren());

				invokeProvider.Invoke();

				Assert.Equal(1, commandExecutions);
				Assert.Equal(1, clicked);

				return Task.CompletedTask;
			});
		}
	}
}
