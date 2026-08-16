using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public class Issue37306 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ContentDrawsThroughBottomSafeAreaPaddingWhileScrolling()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<IScrollView, ScrollViewHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var content = new VerticalStackLayout
			{
				Spacing = 12,
				SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.None)
			};

			for (var i = 0; i < 30; i++)
			{
				content.Add(new Label
				{
					Text = $"Item {i}",
					HeightRequest = 64
				});
			}

			var scrollView = new ScrollView
			{
				Content = content,
				HeightRequest = 400,
				SafeAreaEdges = new SafeAreaEdges(
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.None,
					SafeAreaRegions.Container)
			};

			await AttachAndRun<ScrollViewHandler>(scrollView, async handler =>
			{
				await scrollView.ScrollToAsync(0, 900, false);

				Assert.False(
					handler.PlatformView.ClipToPadding,
					"ScrollView content should draw through safe-area padding while scrolling.");
			});
		}
	}
}
