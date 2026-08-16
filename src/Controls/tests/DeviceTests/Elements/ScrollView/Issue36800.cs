#if IOS
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using UIKit;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	[Collection(ControlsHandlerTestBase.RunInNewWindowCollection)]
	public class Issue36800 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task UndersizedContentDoesNotCreateSafeAreaScrollRange()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddMauiControlsHandlers();
					handlers.AddHandler(typeof(Window), typeof(WindowHandlerStub));
				});
			});

			var scrollView = new ScrollView
			{
				SafeAreaEdges = SafeAreaEdges.Container,
				Content = new VerticalStackLayout
				{
					HeightRequest = 100,
					Children =
					{
						new Label { Text = "Small content" }
					}
				}
			};

			var page = new ContentPage
			{
				SafeAreaEdges = SafeAreaEdges.None,
				Content = scrollView
			};

			await CreateHandlerAndAddToWindow<IWindowHandler>(page, handler =>
			{
				var nativeScrollView = (UIScrollView)scrollView.Handler.PlatformView;
				nativeScrollView.SetNeedsLayout();
				nativeScrollView.LayoutIfNeeded();

				var adjustedContentHeight = nativeScrollView.ContentSize.Height
					+ nativeScrollView.AdjustedContentInset.Top
					+ nativeScrollView.AdjustedContentInset.Bottom;

				Assert.True(
					adjustedContentHeight <= nativeScrollView.Bounds.Height + 0.5,
					$"Native ScrollView must not include safe-area insets twice. " +
					$"Content height: {nativeScrollView.ContentSize.Height}; " +
					$"adjusted insets: {nativeScrollView.AdjustedContentInset.Top}, {nativeScrollView.AdjustedContentInset.Bottom}; " +
					$"bounds height: {nativeScrollView.Bounds.Height}.");
			});
		}
	}
}
#endif
