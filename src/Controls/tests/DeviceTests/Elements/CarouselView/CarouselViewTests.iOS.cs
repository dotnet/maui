using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CoreGraphics;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Handlers.Items2;
using Microsoft.Maui.DeviceTests.Stubs;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using UIKit;
using Xunit;
using static Microsoft.Maui.DeviceTests.AssertHelpers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class CarouselViewTests
	{
		[Theory]
		[InlineData(3, false)]
		[InlineData(4, false)]
		[InlineData(3, true)]
		[InlineData(4, true)]
		public async Task BoundsOnlyResizesPreserveIdleSelectionWithoutSelectionEvents(int position, bool loop)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new CarouselView
			{
				Loop = loop,
				Position = position,
				WidthRequest = 300,
				HeightRequest = 200,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center,
				ItemsSource = items,
				IndicatorView = indicator,
				IsScrollAnimated = false,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				using var selectedIndexPath = controller.GetScrollToIndexPath(position);

				bool IsSelectedItemVisible()
				{
					var cell = collectionView.CellForItem(selectedIndexPath);
					if (cell is null || cell.Hidden || cell.Bounds.Width <= 0 || cell.Bounds.Height <= 0)
					{
						return false;
					}

					var frame = cell.ConvertRectToView(cell.Bounds, collectionView);
					var bounds = collectionView.Bounds;
					var centerX = frame.X + frame.Width / 2;
					var centerY = frame.Y + frame.Height / 2;
					return centerX >= bounds.X && centerX <= bounds.X + bounds.Width &&
						centerY >= bounds.Y && centerY <= bounds.Y + bounds.Height;
				}

				await AssertEventually(() => controller.InitialPositionSet &&
					ReferenceEquals(carousel.CurrentItem, items[position]) &&
					IsSelectedItemVisible());
				collectionView.LayoutIfNeeded();
				controller.ViewWillLayoutSubviews();
				controller.ViewDidLayoutSubviews();
				Assert.False(controller.IsRotatingOrResizing());

				int positionChanges = 0;
				int currentItemChanges = 0;
				carousel.PositionChanged += (_, _) => positionChanges++;
				carousel.CurrentItemChanged += (_, _) => currentItemChanges++;

				var sizes = new[] { new CGSize(240, 160), new CGSize(340, 240), new CGSize(300, 200) };
				foreach (var size in sizes)
				{
					Assert.False(controller.IsScrollInteractionActive());
					carousel.WidthRequest = (double)size.Width;
					carousel.HeightRequest = (double)size.Height;
					var bounds = collectionView.Bounds;
					collectionView.Bounds = new CGRect(bounds.X, bounds.Y, size.Width, size.Height);

					// Observe capture before layout finishes, so removing bounds detection cannot silently pass.
					controller.ViewWillLayoutSubviews();
					Assert.True(controller.IsRotatingOrResizing());
					Assert.Equal(position, controller.PositionBeforeBoundsChange);
					collectionView.SetNeedsLayout();
					collectionView.LayoutIfNeeded();
					controller.ViewDidLayoutSubviews();

					await AssertEventually(() => collectionView.Bounds.Size == size &&
						IsSelectedItemVisible() &&
						!controller.IsRotatingOrResizing(),
						message: "The selected item was not visible after the bounds-only resize.");

					Assert.Equal(position, carousel.Position);
					Assert.Same(items[position], carousel.CurrentItem);
					Assert.Equal(position, indicator.Position);
					Assert.Equal(0, positionChanges);
					Assert.Equal(0, currentItemChanges);

					var cell = Assert.IsAssignableFrom<TemplatedCell2>(collectionView.CellForItem(selectedIndexPath));
					var label = Assert.IsType<Label>(cell.PlatformHandler.VirtualView);
					Assert.Same(items[position], label.BindingContext);
					var visibleFrame = cell.ConvertRectToView(cell.Bounds, collectionView);
					var visibleBounds = collectionView.Bounds;
					Assert.InRange((double)(visibleFrame.X + visibleFrame.Width / 2),
						(double)visibleBounds.X, (double)(visibleBounds.X + visibleBounds.Width));
					Assert.InRange((double)(visibleFrame.Y + visibleFrame.Height / 2),
						(double)visibleBounds.Y, (double)(visibleBounds.Y + visibleBounds.Height));
				}
			});
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public async Task ConfirmedBoundsRestorationCompletesPendingAnimatedPositionChange(bool loop)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third", "Fourth", "Fifth" };
			var indicator = new IndicatorView();
			var carousel = new CarouselView
			{
				Loop = loop,
				WidthRequest = 300,
				HeightRequest = 200,
				ItemsSource = items,
				IndicatorView = indicator,
				IsScrollAnimated = true,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				await AssertEventually(() => controller.InitialPositionSet && carousel.CurrentItem is "First");

				var pendingScroll = typeof(CarouselViewController2).GetField("_gotoPosition", BindingFlags.Instance | BindingFlags.NonPublic);
				Assert.NotNull(pendingScroll);

				carousel.Position = 3;
				// Observe the actual in-flight scroll, including the deferred start on iOS 26.
				await AssertEventually(() => (int)pendingScroll.GetValue(controller) == 3 && carousel.CurrentItem is "First",
					interval: 1, message: "The animated Position change did not enter the pending-scroll state.");

				controller.UpdateResizeState();
				var bounds = collectionView.Bounds;
				collectionView.Bounds = new CGRect(bounds.X, bounds.Y, bounds.Width - 40, bounds.Height);
				controller.UpdateResizeState();
				Assert.Equal(3, controller.PositionBeforeBoundsChange);
				Assert.Equal("First", carousel.CurrentItem);
				Assert.Equal(3, (int)pendingScroll.GetValue(controller));

				Assert.True(controller.ShouldIgnorePositionUpdateAfterBoundsChange(2, true));
				Assert.True(controller.ShouldIgnorePositionUpdateAfterBoundsChange(3, false));
				Assert.Equal(3, controller.PositionBeforeBoundsChange);

				// Deliver the restoration confirmation before UIKit's animation can finish independently.
				Assert.False(controller.ShouldIgnorePositionUpdateAfterBoundsChange(3, true));
				controller.SetPosition(3);

				Assert.Equal(-1, controller.PositionBeforeBoundsChange);
				Assert.Equal(-1, (int)pendingScroll.GetValue(controller));
				Assert.Equal(3, carousel.Position);
				Assert.Equal("Fourth", carousel.CurrentItem);
				Assert.Equal(3, indicator.Position);

				controller.ViewDidLayoutSubviews();
				using var restoredIndexPath = controller.GetScrollToIndexPath(3);
				await AssertEventually(() => collectionView.CellForItem(restoredIndexPath) is not null);

				carousel.IsScrollAnimated = false;
				carousel.Position = 1;
				using var nextIndexPath = controller.GetScrollToIndexPath(1);
				await AssertEventually(() => carousel.Position == 1 && carousel.CurrentItem is "Second" &&
					collectionView.CellForItem(nextIndexPath) is not null,
					message: "A subsequent Position request was blocked after restoration.");
				Assert.Equal(1, indicator.Position);

				carousel.CurrentItem = items[4];
				using var finalIndexPath = controller.GetScrollToIndexPath(4);
				await AssertEventually(() => carousel.Position == 4 && carousel.CurrentItem is "Fifth" &&
					collectionView.CellForItem(finalIndexPath) is not null,
					message: "A subsequent CurrentItem request was blocked after restoration.");
				Assert.Equal(4, indicator.Position);
			});
		}

		[Theory]
		[InlineData(false, "Tracking")]
		[InlineData(false, "Dragging")]
		[InlineData(false, "Decelerating")]
		[InlineData(true, "Tracking")]
		[InlineData(true, "Dragging")]
		[InlineData(true, "Decelerating")]
		public async Task ResizeDuringOrthogonalScrollCancelsPositionRestoration(bool loop, string interactionState)
		{
			SetupBuilder();

			var items = new[] { "First", "Second", "Third" };
			var carousel = new CarouselView
			{
				Loop = loop,
				Position = 1,
				WidthRequest = 300,
				HeightRequest = 200,
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				await AssertEventually(() => controller.InitialPositionSet && carousel.CurrentItem is "Second");

				using var container = new UIView();
				using var scroller = new InteractingCarouselScrollView();
				container.AddSubview(scroller);
				collectionView.AddSubview(container);

				try
				{
					void BeginResize()
					{
						scroller.InteractionState = null;
						controller.UpdateResizeState();
						var bounds = collectionView.Bounds;
						collectionView.Bounds = new CGRect(bounds.X, bounds.Y, bounds.Width - 10, bounds.Height);
						controller.UpdateResizeState();
						Assert.Equal(1, controller.PositionBeforeBoundsChange);
						Assert.True(controller.IsRotatingOrResizing());
						scroller.InteractionState = interactionState;
						Assert.False(collectionView.Tracking || collectionView.Dragging || collectionView.Decelerating);
					}

					BeginResize();
					Assert.False(controller.IsRotatingOrResizing());
					Assert.Equal(-1, controller.PositionBeforeBoundsChange);

					BeginResize();
					Assert.False(controller.ShouldIgnorePositionUpdateAfterBoundsChange(2, false));
					Assert.Equal(-1, controller.PositionBeforeBoundsChange);

					BeginResize();
					controller.ViewDidLayoutSubviews();
					Assert.Equal(-1, controller.PositionBeforeBoundsChange);

					// Cancellation must survive finger-up/deceleration ending, not just bypass one callback.
					scroller.InteractionState = null;
					Assert.False(controller.ShouldIgnorePositionUpdateAfterBoundsChange(2, false));
					controller.SetPosition(2);
					Assert.Equal(2, carousel.Position);
					Assert.Equal("Third", carousel.CurrentItem);
				}
				finally
				{
					container.RemoveFromSuperview();
				}
			});
		}

		[Fact]
		public async Task ScrollingItemContentDoesNotCancelPositionRestoration()
		{
			SetupBuilder();

			var carousel = new CarouselView
			{
				Loop = false,
				WidthRequest = 300,
				HeightRequest = 200,
				ItemsSource = new[] { "First", "Second" },
				ItemTemplate = new DataTemplate(() => new Label())
			};

			await CreateHandlerAndAddToWindow<CarouselViewHandler2>(carousel, async handler =>
			{
				var controller = Assert.IsType<CarouselViewController2>(handler.Controller);
				var collectionView = controller.CollectionView;
				using var indexPath = Foundation.NSIndexPath.FromItemSection(0, 0);
				await AssertEventually(() => controller.InitialPositionSet && collectionView.CellForItem(indexPath) is not null);
				var cell = collectionView.CellForItem(indexPath);
				using var scroller = new InteractingCarouselScrollView { InteractionState = "Dragging" };
				cell.ContentView.AddSubview(scroller);

				try
				{
					controller.UpdateResizeState();
					var bounds = collectionView.Bounds;
					collectionView.Bounds = new CGRect(bounds.X, bounds.Y, bounds.Width - 10, bounds.Height);
					controller.UpdateResizeState();

					Assert.True(controller.IsRotatingOrResizing());
					Assert.True(controller.ShouldIgnorePositionUpdateAfterBoundsChange(1, false));
					Assert.Equal(0, controller.PositionBeforeBoundsChange);
				}
				finally
				{
					scroller.RemoveFromSuperview();
				}
			});
		}

		sealed class InteractingCarouselScrollView : UIScrollView
		{
			public string InteractionState { get; set; }
			public override bool Tracking => InteractionState == "Tracking";
			public override bool Dragging => InteractionState == "Dragging";
			public override bool Decelerating => InteractionState == "Decelerating";
		}

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

			// Allow UIKit to release controller-owned references before forcing managed GC.
			await Task.Delay(100);

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