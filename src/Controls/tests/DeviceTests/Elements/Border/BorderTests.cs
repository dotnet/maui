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
					handlers.AddHandler<Border, BorderHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		[Fact(DisplayName = "Rounded Rectangle Border occupies correct space")]
		public async Task RoundedRectangleBorderLayoutIsCorrect()
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

			await AssertColorAtPoint(border, expected, typeof(BorderHandler), 10, 10);
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

			Assert.NotNull(handlerReference);
			Assert.NotNull(platformViewReference);

			// Several GCs required on iOS
			for (int i = 0; i < 5; i++)
			{
				if (!handlerReference.IsAlive && !platformViewReference.IsAlive)
					break;
				await Task.Yield();
				GC.Collect();
				GC.WaitForPendingFinalizers();
			}

			Assert.False(handlerReference.IsAlive, "Handler should not be alive!");
			Assert.False(platformViewReference.IsAlive, "PlatformView should not be alive!");
		}
	}
}
