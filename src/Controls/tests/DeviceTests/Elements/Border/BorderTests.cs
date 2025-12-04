using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.DeviceTests.ImageAnalysis;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	[Category(TestCategory.Border)]
	public partial class BorderTests : ControlsHandlerTestBase
	{
		void SetupBuilder()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

#if ANDROID
		[Fact("Checks that the default background is transparent", Skip = "Android Helix is failing this test")]
		public async Task DefaultBackgroundIsTransparent ()
		{
			// We use a Grid container to set a background color and then make sure that the Border background
			// is transparent by making sure that the parent Grid's Blue background shows through.
			var grid = new Grid
			{
				ColumnDefinitions = new ColumnDefinitionCollection()
				{
					new ColumnDefinition(GridLength.Star)
				},
				RowDefinitions = new RowDefinitionCollection()
				{
					new RowDefinition(GridLength.Star)
				},
				BackgroundColor = Colors.Blue,
				WidthRequest = 200,
				HeightRequest = 200
			};

			var border = new Border {
				WidthRequest = 200,
				HeightRequest = 200,
				Stroke = Colors.Red,
				StrokeThickness = 10
			};

			grid.Add(border, 0, 0);

			await CreateHandlerAsync<BorderHandler>(border);
			await CreateHandlerAsync<LayoutHandler>(grid);

			var bitmap = await GetRawBitmap(grid, typeof(LayoutHandler));
			Assert.Equal(200, bitmap.Width, 2d);
			Assert.Equal(200, bitmap.Height, 2d);

			// Analyze red border - we expect it to fill a 200x200 area
			var redBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, (c) => c.Red > .5).Single();
			Assert.Equal(200, redBlob.Width, 2d);
			Assert.Equal(200, redBlob.Height, 2d);

			// Analyze the blue blob - it should fill the inside of the border (minus the stroke thickness)
			var blueBlobs = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, (c) => c.Blue > .5);

			// Note: There is a 1px blue border around the red border, so we need to find the one
			// that represents the inner bounds of the border.
			var innerBlob = blueBlobs[0];

			for (int i = 1; i < blueBlobs.Count; i++)
			{
				if (blueBlobs[i].MinColumn > innerBlob.MinColumn && blueBlobs[i].MaxColumn < innerBlob.MaxColumn)
					innerBlob = blueBlobs[i];
			}

			Assert.Equal(180, innerBlob.Width, 2d);
			Assert.Equal(180, innerBlob.Height, 2d);
		}
#endif

		[Fact(DisplayName = "Rounded Rectangle Border occupies correct space")]
		public async Task RoundedRectangleBorderLayoutIsCorrect()
		{
			Color stroke = Colors.Black;
			const int strokeThickness = 4;
			const int radius = 20;

			var grid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitionCollection()
				{
					new ColumnDefinition(GridLength.Star),
					new ColumnDefinition(GridLength.Star)
				},
				RowDefinitions = new RowDefinitionCollection()
				{
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Star)
				},
				BackgroundColor = Colors.White
			};

			var shape = new RoundRectangle()
			{
				CornerRadius = new CornerRadius(radius),
			};

			var border = new Border()
			{
				StrokeShape = shape,
				Stroke = stroke,
				StrokeThickness = strokeThickness,
				BackgroundColor = Colors.Red,
			};

			grid.Add(border, 0, 0);
			grid.WidthRequest = 200;
			grid.HeightRequest = 200;

			await CreateHandlerAsync<BorderHandler>(border);
			await CreateHandlerAsync<LayoutHandler>(grid);

			// This test validates both PR #17087 (Android RoundRectangle fix) and Issue #31330 fix:
			// - PR #17087: RoundRectangle.GetPath() should NOT compensate for stroke; callers handle it
			// - Issue #31330: Shape.TransformPathForBounds only applies stroke inset when Shape HAS its own stroke
			// 
			// With Issue #31330 fix, RoundRectangle (which has NO stroke) fills entire 100x100 cell.
			// Border renders its 4px stroke on top of this path.
			//
			// Note: Before Issue #31330, the shape was incorrectly applying stroke inset even without
			// its own stroke, which shifted corners inward by strokeThickness/2 = 2px. The test points
			// need to account for this change.

			var points = new Point[16];
			var colors = new Color[16];

			// For rounded corners, test along the 45-degree diagonal
			// At 45°, corner curve is at: radius - (radius / sqrt(2)) ≈ 5.86 for radius=20
			var outerXY = radius - (radius / Math.Sqrt(2));
			var innerXY = outerXY + strokeThickness;

			// Test all 4 corners with 4 test points each
			Point[] corners = new Point[4]
			{
				new Point(0, 0),    // upper-left
				new Point(100, 0),  // upper-right
				new Point(0, 100),  // lower-left
				new Point(100, 100) // lower-right
			};

			for (int i = 0; i < corners.Length; i++)
			{
				int xdir = (i == 0 || i == 2) ? 1 : -1;  // +1 for left, -1 for right
				int ydir = (i == 0 || i == 1) ? 1 : -1;  // +1 for top, -1 for bottom

				var baseX = corners[i].X;
				var baseY = corners[i].Y;

				// With the new fix, corners are 2px further out than before (no incorrect inset)
				// Adjust test points to account for the shape now filling the entire cell
				// Original test used: outside at -0.25, stroke at +1.25, stroke at -1.25 from inner, bg at +0.25 from inner

				// Point 1: Outside corner (in grid's white background)
				points[i * 4] = new Point(baseX + xdir * (outerXY - 2), baseY + ydir * (outerXY - 2));
				colors[i * 4] = Colors.White;

				// Point 2: In stroke, near outer edge
				points[i * 4 + 1] = new Point(baseX + xdir * (outerXY + 0.25), baseY + ydir * (outerXY + 0.25));
				colors[i * 4 + 1] = stroke;

				// Point 3: In stroke, near inner edge  
				points[i * 4 + 2] = new Point(baseX + xdir * (outerXY + 2), baseY + ydir * (outerXY + 2));
				colors[i * 4 + 2] = stroke;

				// Point 4: Inside border (in red background)
				points[i * 4 + 3] = new Point(baseX + xdir * (innerXY + 0.25), baseY + ydir * (innerXY + 0.25));
				colors[i * 4 + 3] = border.BackgroundColor;
			}

			await AssertColorsAtPoints(grid, typeof(LayoutHandler), colors, points);
		}

		[Fact(DisplayName = "StrokeThickness does not inset stroke path")]
		public async Task BorderStrokeThicknessDoesNotInsetStrokePath()
		{
			var grid = new Grid()
			{
				ColumnDefinitions = new ColumnDefinitionCollection()
				{
					new ColumnDefinition(GridLength.Star),
					new ColumnDefinition(GridLength.Star)
				},
				RowDefinitions = new RowDefinitionCollection()
				{
					new RowDefinition(GridLength.Star),
					new RowDefinition(GridLength.Star)
				},
				BackgroundColor = Colors.White
			};

			var border = new Border()
			{
				Stroke = Colors.Black,
				StrokeThickness = 10,
				BackgroundColor = Colors.Red
			};

			grid.Add(border, 0, 0);
			grid.WidthRequest = 200;
			grid.HeightRequest = 200;

			await CreateHandlerAsync<BorderHandler>(border);
			await CreateHandlerAsync<LayoutHandler>(grid);

			var points = new Point[2];
			var colors = new Color[2];

#if IOS
			// FIXME: iOS seems to have a white boarder around the Border stroke
			int offset = 1;
#else
			int offset = 0;
#endif

			// Verify that the stroke is where we expect
			points[0] = new Point(offset + 1, offset + 1);
			colors[0] = Colors.Black;

			// Verify that the stroke is only as thick as we expect
			points[1] = new Point(offset + border.StrokeThickness + 1, offset + border.StrokeThickness + 1);
			colors[1] = Colors.Red;

			await AssertColorsAtPoints(grid, typeof(LayoutHandler), colors, points);
		}

		// NOTE: this test is slightly different than MemoryTests.HandlerDoesNotLeak
		// It adds a child to the Border, which is a worthwhile test case.
		[Fact("Border Does Not Leak")]
		public async Task DoesNotLeak()
		{
			SetupBuilder();
			WeakReference platformViewReference = null;
			WeakReference handlerReference = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid();
				var border = new Border();
				var label = new Label();
				border.Content = label;
				layout.Add(border);

				var handler = CreateHandler<LayoutHandler>(layout);
				handlerReference = new WeakReference(label.Handler);
				platformViewReference = new WeakReference(label.Handler.PlatformView);
			});

			await AssertionExtensions.WaitForGC(handlerReference, platformViewReference);
		}

		[Fact(DisplayName = "Border With Stroke Shape And Name Does Not Leak")]
		public async Task DoesNotLeakWithStrokeShape()
		{
			SetupBuilder();
			WeakReference platformViewReference = null;
			WeakReference handlerReference = null;

			await InvokeOnMainThreadAsync(() =>
			{
				var layout = new Grid();
				var border = new Border();
				var rect = new RoundRectangle();
				border.StrokeShape = rect;
				layout.Add(border);

				var nameScope = new NameScope();
				((INameScope)nameScope).RegisterName("Border", border);
				layout.transientNamescope = nameScope;
				border.transientNamescope = nameScope;
				rect.transientNamescope = nameScope;

				var handler = CreateHandler<LayoutHandler>(layout);
				handlerReference = new WeakReference(border.Handler);
				platformViewReference = new WeakReference(border.Handler.PlatformView);
			});

			await AssertionExtensions.WaitForGC(handlerReference, platformViewReference);
		}

		[Fact("Ensures the border renders the expected size - Issue 15339")]
		public async Task BorderAndStrokeIsCorrectSize()
		{
			double borderThickness = 10;
			Border border = new Border() { WidthRequest = 200, HeightRequest = 100 };
			border.BackgroundColor = Colors.Red;
			border.Stroke = Colors.Blue;
			border.StrokeThickness = borderThickness;

			// This is randomly failing on iOS, so let's add a timeout to avoid device tests running for hours
			var bitmap = await GetRawBitmap(border, typeof(BorderHandler)).WaitAsync(TimeSpan.FromSeconds(5));
			Assert.Equal(200, bitmap.Width, 2d);
			Assert.Equal(100, bitmap.Height, 2d);

			// Analyze blue border - we expect it to fill the 200x100 area
			var blueBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, (c) => c.Blue > .5).Single();
			Assert.Equal(200, blueBlob.Width, 2d);
			Assert.Equal(100, blueBlob.Height, 2d);

			// Analyze red inside- we expect it to fill the area minus the stroke thickness
			var redBlob = ConnectedComponentAnalysis.FindConnectedPixels(bitmap, (c) => c.Red > .5).Single();
			Assert.Equal(180, redBlob.Width, 2d);
			Assert.Equal(80, redBlob.Height, 2d);
		}
	}
}
