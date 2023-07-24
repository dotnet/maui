using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
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
					handlers.AddHandler<Label, LabelHandler>();
					handlers.AddHandler<Border, BorderHandler>();
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
