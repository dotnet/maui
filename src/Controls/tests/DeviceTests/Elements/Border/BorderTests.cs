using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
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

			var points = new Point[4];
			var colors = new Color[4];

			// To calculate the x and y offsets (from the center) for a 45-45-90 triangle, we can use the radius as the hypotenuse
			// which means that the x and y offsets would be radius / sqrt(2).
			var xy = radius - (radius / Math.Sqrt(2));

			// This marks the outside edge of the rounded corner.
			var outerXY = xy;

			// Add half the stroke thickness to find the inner edge of the rounded corner.
			var innerXY = xy + (strokeThickness / 2);

			// Verify that the color outside of the rounded corner is the parent's color (White)
			points[0] = new Point(outerXY - 1, outerXY - 1);
			colors[0] = Colors.White;

			// Verify that the rounded corner stroke is where we expect it to be
			points[1] = new Point(outerXY, outerXY);
			colors[1] = stroke;
			points[2] = new Point(innerXY, innerXY);
			colors[2] = stroke;

			// Verify that the background color starts where we'd expect it to start
			points[3] = new Point(innerXY + 1, innerXY + 1);
			colors[3] = border.BackgroundColor;

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
			points[0] = new Point(1 + offset, 1 + offset);
			colors[0] = Colors.Black;

			// Verify that the stroke is only as thick as we expect
			points[1] = new Point(10 + offset, 10 + offset);
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
			Assert.False(handlerReference.IsAlive, "Handler should not be alive!");
			Assert.False(platformViewReference.IsAlive, "PlatformView should not be alive!");
		}
	}
}
