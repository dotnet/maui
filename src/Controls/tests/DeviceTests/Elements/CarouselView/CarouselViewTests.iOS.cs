using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.Handlers;
using Xunit;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CarouselViewTests
	{
		[Fact(DisplayName = "CarouselView Does Not Leak With Default ItemsLayout")]
		public async Task CarouselViewDoesNotLeakWithDefaultItemsLayout()
		{
			SetupBuilder();

			WeakReference weakCarouselView = null;
			WeakReference weakHandler = null;

			await InvokeOnMainThreadAsync(async () =>
			{
				var carouselView = new CarouselView
				{
					ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
					ItemTemplate = new DataTemplate(() => new Label())
					// Note: Not setting ItemsLayout - using the default
				};

				weakCarouselView = new WeakReference(carouselView);

				var handler = await CreateHandlerAsync<CarouselViewHandler2>(carouselView);

				// Verify handler is created
				Assert.NotNull(handler);

				// Store weak reference to the handler
				weakHandler = new WeakReference(handler);

				// Disconnect the handler
				((IElementHandler)handler).DisconnectHandler();
			});

			// Force garbage collection
			await AssertionExtensions.WaitForGC(weakCarouselView, weakHandler);

			// Verify the CarouselView was collected
			Assert.False(weakCarouselView.IsAlive, "CarouselView should have been garbage collected");

			// Verify the handler was collected
			Assert.False(weakHandler.IsAlive, "CarouselViewHandler2 should have been garbage collected");
		}
	}
}