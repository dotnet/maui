using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers;
using Microsoft.Maui.DeviceTests.ImageAnalysis;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;


namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.BoxView)]
	public partial class BoxViewTests : ControlsHandlerTestBase
	{
		[Theory(DisplayName = "BoxView Initializes Correctly")]
		[InlineData(0xFFFF0000)]
		[InlineData(0xFF00FF00)]
		[InlineData(0xFF0000FF)]
		public async Task BoxViewInitializesCorrectly(uint color)
		{
			var expected = Color.FromUint(color);

			var boxView = new BoxView()
			{
				Color = expected,
				HeightRequest = 100,
				WidthRequest = 200
			};

			await ValidateHasColor(boxView, expected, typeof(ShapeViewHandler));
		}

		[Fact("Ensures grid rows renders the correct size - Issue 15330")]
		public async Task Issue15330()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<BoxView, BoxViewHandler>();
				});
			});

			Grid grid = new Grid()
			{
				BackgroundColor = Colors.Cyan,
				WidthRequest = 200,  // TODO: I really don't want to set size - need to hit iOS safe area too
				HeightRequest = 500
			};
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			grid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));
			grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
			grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
			grid.RowDefinitions.Add(new RowDefinition(GridLength.Star));
			BoxView boxView1 = new BoxView() { Color = Colors.Red };
			Grid.SetColumn(boxView1, 1);
			grid.Children.Add(boxView1);
			var boxView2 = new BoxView() { Color = Colors.Lime };
			Grid.SetRow(boxView2, 1);
			grid.Children.Add(boxView2);
			var boxView3 = new BoxView() { Color = Colors.Violet };
			Grid.SetColumn(boxView3, 1);
			Grid.SetRow(boxView3, 1);
			Grid.SetRowSpan(boxView3, 2);
			grid.Children.Add(boxView3);
			var boxView4 = new BoxView() { Color = Colors.Yellow };
			grid.Children.Add(boxView4);

			await CreateHandlerAsync<BoxViewHandler>(boxView1);
			await CreateHandlerAsync<BoxViewHandler>(boxView2);
			await CreateHandlerAsync<BoxViewHandler>(boxView3);
			await CreateHandlerAsync<BoxViewHandler>(boxView4);

			var bitmap = await GetRawBitmap(grid, typeof(LayoutHandler));
			var yellowBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Yellow).Single();
			Assert.Equal(bitmap.Width / 2, yellowBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3, yellowBlob.Height, 2d);
			Assert.Equal(0, yellowBlob.MinColumn, 2d);
			Assert.Equal(0, yellowBlob.MinRow, 2d);

			var redBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Red).Single();
			Assert.Equal(bitmap.Width / 2, redBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3, redBlob.Height, 2d);
			Assert.Equal(bitmap.Width / 2, redBlob.MinColumn, 2d);
			Assert.Equal(0, redBlob.MinRow);

			var limeBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Lime).Single();
			Assert.Equal(bitmap.Width / 2, limeBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3, limeBlob.Height, 2d);
			Assert.Equal(0, limeBlob.MinColumn, 2d);
			Assert.Equal(bitmap.Height / 3, limeBlob.MinRow, 2d);

			var violetBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Violet).Single();
			Assert.Equal(bitmap.Width / 2, violetBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3 * 2, violetBlob.Height, 2d);
			Assert.Equal(bitmap.Width / 2, violetBlob.MinColumn, 2d);
			Assert.Equal(bitmap.Height / 3, violetBlob.MinRow, 2d);

			var cyanBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, Colors.Cyan).Single();
			Assert.Equal(bitmap.Width / 2, cyanBlob.Width, 2d);
			Assert.Equal(bitmap.Height / 3, cyanBlob.Height, 2d);
			Assert.Equal(0, cyanBlob.MinColumn, 2d);
			Assert.Equal(bitmap.Height / 3 * 2, cyanBlob.MinRow, 2d);
		}
	}
}