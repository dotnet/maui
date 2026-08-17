using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class ScrollViewTests
	{
		[Fact]
		public async Task EntryDoesNotReceiveFocusWhenWindowOpens()
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
			var focused = false;
			entry.Focused += (_, _) => focused = true;

			var scrollView = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					HeightRequest = 2000,
					Children =
					{
						entry
					}
				}
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(new Window(new ContentPage { Content = scrollView }), async _ =>
			{
				var platformScrollView = ((ScrollViewHandler)scrollView.Handler).PlatformView;
				await WaitAssert(() => !platformScrollView.IsTabStop);

				Assert.False(entry.IsFocused);
				Assert.False(focused);
			});
		}

		[Fact]
		public async Task ScrollViewerRestoresTabStopDefaultAcrossHandlerLifecycle()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<ScrollView, ScrollViewHandler>();
				});
			});

			await InvokeOnMainThreadAsync(() =>
			{
				var scrollView = new ScrollView();
				var handler = CreateHandler<ScrollViewHandler>(scrollView);
				var mauiContext = handler.MauiContext;
				var platformView = handler.PlatformView;

				Assert.True(platformView.IsTabStop);

				((IElementHandler)handler).DisconnectHandler();
				Assert.False(platformView.IsTabStop);

				((IElementHandler)handler).SetMauiContext(mauiContext);
				((IElementHandler)handler).SetVirtualView(scrollView);
				var reconnectedPlatformView = handler.PlatformView;
				Assert.True(reconnectedPlatformView.IsTabStop);

				((IElementHandler)handler).DisconnectHandler();
				Assert.False(reconnectedPlatformView.IsTabStop);
			});
		}
	}
}
