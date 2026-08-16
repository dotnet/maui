using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.ScrollView)]
	public class Issue36042 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ScrollViewInsideVerticalStackLayoutScrollsToRequestedOffset()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<IScrollView, ScrollViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<VerticalStackLayout, LayoutHandler>();
				});
			});

			var scrollView = new ScrollView
			{
				Content = new Grid
				{
					HeightRequest = 1800
				}
			};

			var stackLayout = new VerticalStackLayout
			{
				scrollView
			};

			var rootLayout = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition(GridLength.Star)
				}
			};
			rootLayout.Add(stackLayout);

			var page = new ContentPage
			{
				Content = rootLayout
			};

			await CreateHandlerAndAddToWindow(page, async () =>
			{
				await scrollView.ScrollToAsync(0, 900, false);

				Assert.True(scrollView.ScrollY > 0, "ScrollView should move away from the top after ScrollToAsync.");
			});
		}
	}
}
