using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.DeviceTests.ImageAnalysis;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
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
		public Task RoundedRectangleBorderLayoutIsCorrect()
		{
			var expected = Colors.Red;

			var container = new Grid();
			container.WidthRequest = 100;
			container.HeightRequest = 100;

			var border = new Border()
			{
				Stroke = Colors.Red,
				StrokeThickness = 1,
				BackgroundColor = Colors.Red,
				HeightRequest = 100,
				WidthRequest = 100
			};

			return AssertColorAtPoint(border, expected, typeof(BorderHandler), 10, 10);
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

#if IOS
			// FIXME: iOS seems to have a white boarder around the Border stroke
#else
			await AssertColorAtPoint(grid, Colors.Black, typeof(LayoutHandler), 1, 1);
#endif
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

#if IOS
		[Fact("Ensures the border renders the expected size - Issue 15339")]
		public async Task BorderAndStrokeIsCorrectSize()
		{
			double borderThickness = 10;
			Border border = new Border() { WidthRequest = 200, HeightRequest = 100 };
			border.BackgroundColor = Colors.Red;
			border.Stroke = Colors.Blue;
			border.StrokeThickness = borderThickness;

			var bitmap = await GetRawBitmap(border, typeof(BorderHandler));
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
#endif
	}
}
