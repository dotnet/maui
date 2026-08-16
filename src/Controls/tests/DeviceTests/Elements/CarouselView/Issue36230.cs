#if MACCATALYST
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.CarouselView)]
	public class Issue36230 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ReplacingCurrentItemKeepsReplacementSelected()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CarouselView, CarouselViewHandler2>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var items = new ObservableCollection<string>
			{
				"1",
				"2",
				"3"
			};
			var carouselView = new CarouselView
			{
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() => new Label()),
				ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView,
				Loop = false
			};
			var layout = new Grid
			{
				carouselView
			};

			await CreateHandlerAndAddToWindow<LayoutHandler>(layout, async handler =>
			{
				var carouselHandler = Assert.IsType<CarouselViewHandler2>(carouselView.Handler);
				await carouselHandler.Controller.CollectionView.PerformBatchUpdatesAsync(() => { });

				carouselView.Position = 1;
				carouselView.CurrentItem = items[1];

				var replacementItem = "2b";
				carouselView.CurrentItem = replacementItem;
				items[1] = replacementItem;
				await carouselHandler.Controller.CollectionView.PerformBatchUpdatesAsync(() => { });

				Assert.True(
					ReferenceEquals(replacementItem, carouselView.CurrentItem),
					"CurrentItem should remain the replacement item.");
			});
		}
	}
}
#endif
