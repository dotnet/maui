using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CarouselViewTests
	{
		void SetupBuilderForDetachedItemsSourceReplacement()
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler(typeof(Toolbar), typeof(ToolbarHandler));
					handlers.AddHandler(typeof(NavigationPage), typeof(NavigationRenderer));
					handlers.AddHandler<Page, PageHandler>();
					handlers.AddHandler<Window, WindowHandlerStub>();
					handlers.AddHandler<CarouselView, CarouselViewHandler2>();
					handlers.AddHandler<IContentView, ContentViewHandler>();
					handlers.AddHandler<Grid, LayoutHandler>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});
		}

		// Reproduces https://github.com/dotnet/maui/issues/36602:
		// While a CarouselView using CarouselViewHandler2 is detached (navigated away from),
		// replacing its ItemsSource and setting a new CurrentItem must NOT be overridden by the
		// stale Position value once the CarouselView is reattached.
		[Fact(DisplayName = "CarouselView Honors CurrentItem Over Stale Position After Detached ItemsSource Replacement")]
		public async Task CarouselViewHonorsCurrentItemOverStalePositionAfterDetachedItemsSourceReplacement()
		{
			SetupBuilderForDetachedItemsSourceReplacement();

			var catalogA = new List<string> { "A0", "A1", "A2", "A3", "A4" };
			var catalogB = new List<string> { "B0", "B1", "B2", "B3", "B4" };

			var indicator = new IndicatorView();
			var carouselView = new CarouselView
			{
				ItemsSource = catalogA,
				ItemTemplate = new DataTemplate(() => new Label()),
				IndicatorView = indicator
			};

			var navPage = new NavigationPage(new ContentPage { Content = carouselView });

			await CreateHandlerAndAddToWindow<WindowHandlerStub>(new Window(navPage), async _ =>
			{
				// Establish the initial, valid selection: Position 3 / CurrentItem "A3".
				carouselView.Position = 3;
				carouselView.CurrentItem = "A3";

				await AssertEventually(
					() => carouselView.Position == 3 && (string)carouselView.CurrentItem == "A3",
					message: "Initial CarouselView state failed to synchronize to Position 3 / CurrentItem A3");

				// Navigate away — this detaches the CarouselView's native view from the window.
				await navPage.PushAsync(new ContentPage { Content = new Label { Text = "Away" } });

				// While detached, replace ItemsSource with catalog B and select "B1" via CurrentItem.
				// Position is intentionally left untouched (still 3), mirroring the real-world scenario.
				carouselView.ItemsSource = catalogB;
				carouselView.CurrentItem = "B1";

				// Navigate back — this reattaches the CarouselView's native view and triggers position sync.
				await navPage.PopAsync();

				await AssertEventually(
					() => carouselView.Position == 1 && (string)carouselView.CurrentItem == "B1",
					timeout: 3000,
					message: "CarouselView did not resolve CurrentItem 'B1' at Position 1 after reattaching; " +
						$"actual Position={carouselView.Position}, CurrentItem={carouselView.CurrentItem}");

				// Expected (fixed behavior): CurrentItem "B1" wins, Position resolves to 1, and the
				// linked IndicatorView stays in sync. Regressed behavior restores stale Position 3,
				// which would make CurrentItem resolve to "B3" instead.
				Assert.Equal("B1", carouselView.CurrentItem);
				Assert.Equal(1, carouselView.Position);
				Assert.Equal(1, indicator.Position);
			});
		}

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