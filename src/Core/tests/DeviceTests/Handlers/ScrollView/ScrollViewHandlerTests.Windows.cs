using System.Threading.Tasks;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewHandlerTests
	{
		[Fact]
		public async Task ContentPanelHandlesFocus()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<EntryStub, EntryHandler>();
				});
			});

			await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollViewStub
				{
					Content = new EntryStub()
				};

				var handler = CreateHandler(scrollView);
				var contentPanel = Assert.IsType<ContentPanel>(handler.PlatformView.Content);

				Assert.True(contentPanel.IsTabStop);
				Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(contentPanel));

				scrollView.Content = null;
				handler.UpdateValue(nameof(IScrollView.Content));

				Assert.False(contentPanel.IsTabStop);

				scrollView.Content = new EntryStub();
				handler.UpdateValue(nameof(IScrollView.Content));

				Assert.True(contentPanel.IsTabStop);
				Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(contentPanel));

				((IElementHandler)handler).DisconnectHandler();
				handler.SetVirtualView(scrollView);

				contentPanel = Assert.IsType<ContentPanel>(handler.PlatformView.Content);
				Assert.True(contentPanel.IsTabStop);
				Assert.Equal(AccessibilityView.Raw, AutomationProperties.GetAccessibilityView(contentPanel));
			});
		}
	}
}
