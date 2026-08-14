using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.UI.Xaml;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewTests
	{
		[Fact]
		public async Task ScrollViewerReceivesFocusBeforeContent()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Entry, EntryHandler>();
					handlers.AddHandler<ScrollView, ScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var entry = new Entry();
			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					entry
				}
			};

			await AttachAndRun<ScrollViewHandler>(scrollView, async handler =>
			{
				Assert.True(handler.PlatformView.IsTabStop);

				var platformEntry = Assert.IsAssignableFrom<Microsoft.UI.Xaml.Controls.Control>(entry.Handler.PlatformView);
				Assert.Equal(FocusState.Unfocused, platformEntry.FocusState);

				Assert.True(handler.PlatformView.Focus(FocusState.Programmatic));
				Assert.NotEqual(FocusState.Unfocused, handler.PlatformView.FocusState);
				Assert.Equal(FocusState.Unfocused, platformEntry.FocusState);

				var contentPanel = Assert.IsAssignableFrom<UIElement>(handler.PlatformView.Content);
				Assert.False(contentPanel.IsTabStop);

				Assert.True(platformEntry.Focus(FocusState.Programmatic));
				Assert.NotEqual(FocusState.Unfocused, platformEntry.FocusState);
				Assert.Equal(FocusState.Unfocused, handler.PlatformView.FocusState);

				await Task.CompletedTask;
			});
		}
	}
}
