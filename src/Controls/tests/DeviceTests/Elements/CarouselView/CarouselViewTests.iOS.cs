using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CarouselViewTests
	{
		[Theory]
		[InlineData(false, false)]
		[InlineData(false, true)]
		[InlineData(true, false)]
		[InlineData(true, true)]
		public async Task TemplateSelectorPreservesCellWhenMeasureIsInvalidated(bool loop, bool createsNewTemplates)
		{
			EnsureHandlerCreated(builder =>
			{
				builder.ConfigureMauiHandlers(handlers =>
				{
					handlers.AddHandler<CarouselView, CarouselViewHandler2>();
					handlers.AddHandler<Label, LabelHandler>();
				});
			});

			var carouselView = new CarouselView
			{
				Loop = loop,
				HeightRequest = 200,
				WidthRequest = 300,
				ItemsSource = new[] { 1, 2 },
				ItemTemplate = createsNewTemplates
					? new NewCarouselTemplateSelector()
					: new CustomDataTemplateSelectorSelector
					{
						Template1 = new DataTemplate(() => new Label { Text = "Item 1" }),
						Template2 = new DataTemplate(() => new Label { Text = "Item 2" })
					}
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carouselView, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				using var indexPath = Foundation.NSIndexPath.FromItemSection(loop ? 1 : 0, 0);

				await AssertEventually(() => controller.InitialPositionSet &&
					collectionView.CellForItem(indexPath) is TemplatedCell2,
					message: "The first carousel item was not displayed.");

				var cell = Assert.IsAssignableFrom<TemplatedCell2>(collectionView.CellForItem(indexPath));
				var label = Assert.IsType<Label>(cell.PlatformHandler.VirtualView);
				Assert.Equal("Item 1", label.Text);
				var reuseIdentifier = cell.ReuseIdentifier;

				label.Text = "Item 1 with updated content";
				((IPlatformMeasureInvalidationController)cell).InvalidateMeasure();
				((IPlatformMeasureInvalidationController)collectionView).InvalidateMeasure(isPropagating: true);
				Assert.True(cell.MeasureInvalidated);

				controller.ViewWillLayoutSubviews();
				collectionView.LayoutIfNeeded();

				Assert.Same(cell, collectionView.CellForItem(indexPath));
				Assert.Equal(reuseIdentifier, cell.ReuseIdentifier);
				Assert.Equal("Item 1 with updated content", label.Text);
				Assert.Equal(1, label.BindingContext);

				carouselView.Position = 1;
				using var nextIndexPath = Foundation.NSIndexPath.FromItemSection(loop ? 2 : 1, 0);
				await AssertEventually(() =>
					collectionView.CellForItem(nextIndexPath) is TemplatedCell2 nextCell &&
					nextCell.PlatformHandler?.VirtualView is Label { Text: "Item 2" } &&
					carouselView.CurrentItem is 2,
					message: "The second carousel template was not displayed after scrolling.");
			});
		}

		sealed class NewCarouselTemplateSelector : DataTemplateSelector
		{
			protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
				=> new DataTemplate(() => new Label { Text = $"Item {item}" });
		}

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

		[Fact(DisplayName = "IsEnabled Updates Native Interaction State On Both Container And CollectionView")]
		public async Task IsEnabledUpdatesNativeInteractionStateOnBothContainerAndCollectionView()
		{
			SetupBuilder();

			var carouselView = new CarouselView
			{
				IsEnabled = false,
				ItemsSource = new List<string> { "Item 1", "Item 2", "Item 3" },
				ItemTemplate = new DataTemplate(() => new Label())
			};

			var handler = await CreateHandlerAsync<CarouselViewHandler2>(carouselView);

			await InvokeOnMainThreadAsync(() =>
			{
				Assert.False(handler.Controller.CollectionView.UserInteractionEnabled);
				Assert.False(handler.PlatformView.UserInteractionEnabled);

				carouselView.IsEnabled = true;
				handler.UpdateValue(nameof(IView.IsEnabled));

				// Both the inner CollectionView and the outer Controller.View (handler.PlatformView)
				// must become interactive - if only the inner one updates, swiping stays blocked
				// because Controller.View is the outer view that gates touch delivery.
				Assert.True(handler.Controller.CollectionView.UserInteractionEnabled);
				Assert.True(handler.PlatformView.UserInteractionEnabled);
			});
		}
	}
}