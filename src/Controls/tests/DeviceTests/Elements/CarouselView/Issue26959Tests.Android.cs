using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AndroidX.RecyclerView.Widget;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.CarouselView)]
	public class Issue26959 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task VisibleItemContainsItsBoundContentView()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CarouselView, CarouselViewHandler>();
					handlers.AddHandler<ContentPresenter, ContentViewHandler>();
					handlers.AddHandler<IContentView, ContentViewHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var slides = new ObservableCollection<ContentView>
			{
				new() { Content = new Label { Text = "First" } },
				new() { Content = new Label { Text = "Second" } },
				new() { Content = new Label { Text = "Third" } },
			};

			var carouselView = new CarouselView
			{
				HeightRequest = 300,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
				ItemsSource = slides,
				Loop = false,
				ItemTemplate = new DataTemplate(() =>
				{
					var presenter = new ContentPresenter();
					presenter.SetBinding(ContentPresenter.ContentProperty, ".");
					return new ContentView { Content = presenter };
				}),
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler>(carouselView, async handler =>
			{
				var recyclerView = handler.PlatformView;
				await recyclerView.WaitForLayoutOrNonZeroSize();

				recyclerView.ScrollToPosition(1);

				var layoutManager = Assert.IsType<LinearLayoutManager>(recyclerView.GetLayoutManager());
				await AssertEventually(
					() => layoutManager.FindFirstCompletelyVisibleItemPosition() == 1,
					message: "The native carousel did not finish scrolling to the second item.");

				ContentPresenter visiblePresenter = null;
				foreach (var descendant in carouselView.GetVisualTreeDescendants())
				{
					if (descendant is ContentPresenter presenter &&
						ReferenceEquals(presenter.BindingContext, slides[1]))
					{
						visiblePresenter = presenter;
						break;
					}
				}

				var visibleContent = visiblePresenter?.Content as ContentView;
				Assert.True(
					ReferenceEquals(visibleContent, slides[1]) &&
					visibleContent.Width > 0 &&
					visibleContent.Height > 0,
					"The visible carousel item must contain the matching non-zero-sized ContentView.");
			});
		}
	}
}
