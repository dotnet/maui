using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Image)]
	public class Issue14305 : ControlsHandlerTestBase
	{
		[Fact]
		public async Task ImageRemainsWithinAssignedStarRow()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<BoxView, BoxViewHandler>();
					handlers.AddHandler<Image, ImageHandler>();
				});
			});

			var autoRowContent = new BoxView
			{
				HeightRequest = 36
			};
			var starCellProbe = new BoxView();
			var image = new Image
			{
				Source = "red.png",
				HeightRequest = 200,
				HorizontalOptions = LayoutOptions.Center
			};
			var grid = new Grid
			{
				HeightRequest = 320,
				MaximumHeightRequest = 320,
				WidthRequest = 320,
				RowDefinitions =
				{
					new RowDefinition(GridLength.Auto),
					new RowDefinition(GridLength.Star),
					new RowDefinition(new GridLength(150))
				}
			};

			Grid.SetRow(starCellProbe, 1);
			Grid.SetRow(image, 1);
			grid.Children.Add(autoRowContent);
			grid.Children.Add(starCellProbe);
			grid.Children.Add(image);

			await CreateHandlerAndAddToWindow<LayoutHandler>(grid, async _ =>
			{
				await image.WaitUntilLoaded();
				await AssertionExtensions.AssertEventually(
					() => starCellProbe.Bounds.Width > 0 && starCellProbe.Bounds.Height > 0,
					message: "The star-row probe should be laid out before its bounds are compared.");

				var starRowBounds = starCellProbe.Bounds;
				var imageBounds = image.Bounds;
				const double tolerance = 0.5;
				var isWithinStarRow =
					imageBounds.Top >= starRowBounds.Top - tolerance &&
					imageBounds.Bottom <= starRowBounds.Bottom + tolerance;

				Assert.True(
					isWithinStarRow,
					$"Image should remain within its assigned star row. Star row: {starRowBounds}; Image: {imageBounds}");
			});
		}
	}
}
