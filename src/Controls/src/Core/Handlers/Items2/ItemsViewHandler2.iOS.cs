#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public abstract partial class ItemsViewHandler2<TItemsView> : ViewHandler<TItemsView, UIView> where TItemsView : ItemsView
	{
		public ItemsViewHandler2() : base(ItemsViewMapper)
		{

		}

		public ItemsViewHandler2(PropertyMapper mapper = null) : base(mapper ?? ItemsViewMapper)
		{

		}

		public static PropertyMapper<TItemsView, ItemsViewHandler2<TItemsView>> ItemsViewMapper = new(ViewMapper)
		{
			[Controls.ItemsView.ItemsSourceProperty.PropertyName] = MapItemsSource,
			[Controls.ItemsView.HorizontalScrollBarVisibilityProperty.PropertyName] = MapHorizontalScrollBarVisibility,
			[Controls.ItemsView.VerticalScrollBarVisibilityProperty.PropertyName] = MapVerticalScrollBarVisibility,
			[Controls.ItemsView.ItemTemplateProperty.PropertyName] = MapItemTemplate,
			[Controls.ItemsView.EmptyViewProperty.PropertyName] = MapEmptyView,
			[Controls.ItemsView.EmptyViewTemplateProperty.PropertyName] = MapEmptyViewTemplate,
			[Controls.ItemsView.FlowDirectionProperty.PropertyName] = MapFlowDirection,
			[Controls.ItemsView.IsVisibleProperty.PropertyName] = MapIsVisible,
			[Controls.ItemsView.ItemsUpdatingScrollModeProperty.PropertyName] = MapItemsUpdatingScrollMode,
			[nameof(IView.IsEnabled)] = MapIsEnabled
		};

		UICollectionViewLayout _layout;

		protected override void DisconnectHandler(UIView platformView)
		{
			ItemsView.ScrollToRequested -= ScrollToRequested;
			_layout = null;
			Controller?.DetachScrollAnchor();
			Controller?.DisposeItemsSource();
			Controller?.DisposeObserver();
			base.DisconnectHandler(platformView);
		}

		protected override void ConnectHandler(UIView platformView)
		{
			base.ConnectHandler(platformView);
			Controller.CollectionView.BackgroundColor = UIColor.Clear;
			ItemsView.ScrollToRequested += ScrollToRequested;
		}

		private protected override UIView OnCreatePlatformView()
		{
			UpdateLayout();
			Controller = CreateController(ItemsView, _layout);
			return base.OnCreatePlatformView();
		}

		protected TItemsView ItemsView => VirtualView;

		protected internal ItemsViewController2<TItemsView> Controller { get; private set; }

		protected abstract UICollectionViewLayout SelectLayout();

		protected abstract ItemsViewController2<TItemsView> CreateController(TItemsView newElement, UICollectionViewLayout layout);

		protected override UIView CreatePlatformView()
		{
			var controllerView = Controller?.View ?? throw new InvalidOperationException("ItemsViewController2's view should not be null at this point.");
			return controllerView;
		}

		public static void MapItemsSource(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			MapItemsUpdatingScrollMode(handler, itemsView);
#if MACCATALYST
			// ItemsSource replacement: saved index may now resolve to different content even
			// if still in range. Clear before applying the new source.
			ClearMacCatalystPendingScrollRestore(handler);
#endif
			handler.Controller?.UpdateItemsSource();
		}

		public static void MapHorizontalScrollBarVisibility(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.CollectionView?.UpdateHorizontalScrollBarVisibility(itemsView.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.CollectionView?.UpdateVerticalScrollBarVisibility(itemsView.VerticalScrollBarVisibility);
		}

		public static void MapItemTemplate(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		public static void MapEmptyView(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.UpdateEmptyView();
		}

		public static void MapEmptyViewTemplate(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.UpdateEmptyView();
		}

		public static void MapFlowDirection(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.UpdateFlowDirection();

			// UIKit does not automatically mirror or reflow UICollectionView layouts when the flow direction
			// (semanticContentAttribute) changes at runtime. To ensure correct RTL/LTR behavior, we explicitly
			// notify the controller to rebuild or reassign its layout. Without this, UICollectionViewCompositionalLayout
			// and other layouts will keep their previous geometry and ignore the new direction.
			handler.UpdateLayout();
		}

		public static void MapIsVisible(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.UpdateVisibility();
		}

		internal static void MapIsEnabled(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			(handler.Controller as SelectableItemsViewController2<ReorderableItemsView>)?.UpdateSelectionMode();

			// Funnel through the base handler's IsEnabled mapping so UserInteractionEnabled
			// stays correctly derived from both IsEnabled and InputTransparent.
			ViewHandler.MapIsEnabled(handler, itemsView);
		}

		public static void MapItemsUpdatingScrollMode(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			if (handler.ItemsView is StructuredItemsView structuredItemsView && structuredItemsView.ItemsLayout is ItemsLayout itemsLayout)
			{
				itemsLayout.ItemsUpdatingScrollMode = itemsView.ItemsUpdatingScrollMode;
			}
		}

		//TODO: this is being called 2 times on startup, one from OnCreatePlatformView and otehr from the mapper for the layout
		protected virtual void UpdateLayout()
		{
			_layout = SelectLayout();
			Controller?.UpdateLayout(_layout);

#if MACCATALYST
			// Layout swap (ItemTemplate / FlowDirection / etc.) may remap the saved
			// section/item to a different visual position. Clear to avoid stale restore.
			ClearMacCatalystPendingScrollRestore(this);
#endif
		}

#if MACCATALYST
		static void ClearMacCatalystPendingScrollRestore(ItemsViewHandler2<TItemsView> handler)
		{
			if (handler?.Controller?.CollectionView is MauiCollectionView mauiCV)
			{
				mauiCV.ClearPendingScrollRestore();
			}
		}
#endif

		protected virtual void ScrollToRequested(object sender, ScrollToRequestEventArgs args) => TryScrollTo(args);

		internal bool TryScrollTo(ScrollToRequestEventArgs args) => TryScrollTo(args, out _);

		// Scrolls to the item the request is for. Returns whether the request still stands once that scroll returns: not when
		// the view doesn't have the item, which it doesn't scroll for, nor when the app made a newer request from what the
		// scroll raised (its Scrolled event), which wins. It also says whether UIKit animates the scroll, and reports when the
		// animation ends (see ItemsViewController2.ScrollToItem).
		internal bool TryScrollTo(ScrollToRequestEventArgs args, out bool animating)
		{
			int section = 0, item = 0;
			int request;
			UICollectionViewScrollPosition scrollPosition = UICollectionViewScrollPosition.None;
			animating = false;

			using (var indexPath = DetermineIndex(args))
			{
				if (!IsIndexPathValid(indexPath))
				{
					// Specified path wasn't valid, or item wasn't found
					return false;
				}

				var scrollDirection = Controller.GetScrollDirection();
				scrollPosition = Items.ScrollToPositionExtensions.ToCollectionViewScrollPosition(args.ScrollToPosition, scrollDirection);

				// Capture section and item as ints before the using block disposes the indexPath
				section = (int)indexPath.Section;
				item = (int)indexPath.Item;

				// A newer request: what an earlier scroll left for when its animation ends, or for the next layout, no longer
				// applies. A request for an item the view doesn't have scrolls nowhere, and returned above, leaving it.
				request = Controller.StartScrollRequest();
				Controller.ScrollingToEdge(null);

				// Clear any previously armed restore before issuing the new scroll. Otherwise the
				// synchronous KVO contentOffset notification fired by this ScrollToItem call could be
				// evaluated against a stale armed target and queue a restore back to the old position.
#if MACCATALYST
				if (Controller?.CollectionView is MauiCollectionView mauiCVBeforeScroll)
				{
					mauiCVBeforeScroll.ClearPendingScrollRestore();
				}
#endif

				// A scroll the app requests is judged by where it lands: the anchor doesn't scroll it back to the end. An animated
				// one is judged once UIKit reports that it ended.
				var scrollAnchor = Controller.ScrollAnchor;
				scrollAnchor?.ScrollRequesting();
				try
				{
					animating = Controller.ScrollToItem(indexPath, scrollPosition, args.IsAnimated);
				}
				finally
				{
					scrollAnchor?.ScrollRequested(Controller.CollectionView, Controller.IsLatestScrollRequest(request), animating);
				}
			}

			// A newer request the app made from the Scrolled event the scroll raised wins: this one doesn't keep its item in
			// view. A ScrollToStart or ScrollToEnd scrolls now that this scroll returned (see ScrollToItem).
			if (!Controller.IsLatestScrollRequest(request))
			{
				animating = false;
				Controller.ScrolledToItem();
				return false;
			}

			// After non-animated scroll, arm KVO restore to recover from silent Mac Catalyst contentOffset shift
#if MACCATALYST
			if (Controller?.CollectionView is MauiCollectionView mauiCV)
			{
				if (!args.IsAnimated)
				{
					mauiCV.SetPendingScrollRestore(section, item, scrollPosition);
				}
			}
#endif

			return true;

			NSIndexPath DetermineIndex(ScrollToRequestEventArgs args)
			{
				if (args.Mode == ScrollToMode.Position)
				{
					if (args.GroupIndex == -1)
					{
						// When IsGrouped is set and no explicit group index is provided,
						// convert the flat index to the correct section/item index path.
						if (ItemsView is GroupableItemsView groupable && groupable.IsGrouped)
						{
							var itemsSource = Controller.ItemsSource;
							if (itemsSource is not null)
							{
								return ConvertFlatIndexToGroupedIndexPath(args.Index, itemsSource);
							}

							return null;
						}

						return NSIndexPath.Create(0, args.Index);
					}

					return NSIndexPath.Create(args.GroupIndex, args.Index);
				}

				return Controller.GetIndexForItem(args.Item);
			}

			static NSIndexPath ConvertFlatIndexToGroupedIndexPath(int flatIndex, Items.IItemsViewSource itemsSource)
			{
				if (flatIndex < 0)
				{
					return null;
				}

				int remaining = flatIndex;
				int groupCount = itemsSource.GroupCount;
				for (int section = 0; section < groupCount; section++)
				{
					int itemCount = itemsSource.ItemCountInGroup(section);
					if (remaining < itemCount)
					{
						return NSIndexPath.Create(section, remaining);
					}
					remaining -= itemCount;
				}

				return null;
			}
		}

		protected bool IsIndexPathValid(NSIndexPath indexPath)
		{
			return LayoutFactory2.IsIndexPathValid(indexPath, Controller.CollectionView);
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var contentSize = Controller.GetSize();
			contentSize = EnsureContentSizeForScrollDirection(widthConstraint, heightConstraint, contentSize);

			// Our target size is the smaller of it and the constraints
			var width = contentSize.Width <= widthConstraint ? contentSize.Width : widthConstraint;
			var height = contentSize.Height <= heightConstraint ? contentSize.Height : heightConstraint;

			IView virtualView = VirtualView;

			width = ViewHandlerExtensions.ResolveConstraints(width, virtualView.Width, virtualView.MinimumWidth, virtualView.MaximumWidth);
			height = ViewHandlerExtensions.ResolveConstraints(height, virtualView.Height, virtualView.MinimumHeight, virtualView.MaximumHeight);

			return new Size(width, height);
		}

		Size EnsureContentSizeForScrollDirection(double widthConstraint, double heightConstraint, Size contentSize)
		{
			// Get the CollectionView orientation
			var scrollDirection = Controller.GetScrollDirection();

			// If contentSize is zero in the relevant dimension (height for vertical, width for horizontal),
			// it means none of the content has been realized yet.
			if ((scrollDirection == UICollectionViewScrollDirection.Vertical && contentSize.Height == 0) ||
				(scrollDirection == UICollectionViewScrollDirection.Horizontal && contentSize.Width == 0))
			{
				// Empty CV should collapse to zero, not fire the expansive fallback.
				if (VirtualView is CollectionView && Controller.IsEmpty)
				{
					return contentSize;
				}

				var collectionView = Controller.CollectionView;

				// CV is mounted but its frame in the scroll direction is still 0
				// because it was previously collapsed by the IsEmpty guard above.
				// CompositionalLayout needs non-zero bounds to compute item sizes,
				// so we force a temporary layout pass in the block below.
				bool frameIsZeroInScrollDirection = VirtualView is CollectionView &&
					((scrollDirection == UICollectionViewScrollDirection.Vertical && collectionView.Frame.Height == 0) ||
					 (scrollDirection == UICollectionViewScrollDirection.Horizontal && collectionView.Frame.Width == 0));

				var measurementWidth = widthConstraint;
				var measurementHeight = heightConstraint;
				if (VirtualView is CollectionView and IView view)
				{
					static double ResolveMeasurementConstraint(double constraint, double exact, double minimum, double maximum)
					{
						// An explicit request or finite maximum can bound an otherwise unbounded probe.
						return double.IsFinite(exact) || double.IsFinite(maximum)
							? ViewHandlerExtensions.ResolveConstraints(constraint, exact, minimum, maximum)
							: constraint;
					}

					if (scrollDirection == UICollectionViewScrollDirection.Vertical &&
						!double.IsFinite(widthConstraint))
					{
						measurementWidth = ResolveMeasurementConstraint(widthConstraint, view.Width, view.MinimumWidth, view.MaximumWidth);
					}
					else if (scrollDirection == UICollectionViewScrollDirection.Horizontal &&
						!double.IsFinite(heightConstraint))
					{
						measurementHeight = ResolveMeasurementConstraint(heightConstraint, view.Height, view.MinimumHeight, view.MaximumHeight);
					}
				}

				// When the CollectionView has not yet been added to a window (pre-mount measurement),
				// UICollectionViewCompositionalLayout hasn't run a layout pass and therefore
				// CollectionViewContentSize is still zero. Force a layout pass with the given constraints
				// so the layout can compute actual content size from its items.
				// Skip unbounded CollectionView cross-axis probes to avoid measuring the synthetic frame.
				bool canMeasureContent = VirtualView is not CollectionView ||
					((scrollDirection != UICollectionViewScrollDirection.Horizontal || double.IsFinite(measurementHeight)) &&
					 (scrollDirection != UICollectionViewScrollDirection.Vertical || double.IsFinite(measurementWidth)));

				if (canMeasureContent && (collectionView.Window == null || frameIsZeroInScrollDirection))
				{
					// Local helper to clamp layout constraints to finite, non-negative nfloat values.
					nfloat ClampConstraint(double constraint, nfloat fallback)
					{
						// Treat NaN, infinity, and negative values as invalid and fall back.
						if (double.IsNaN(constraint) || double.IsInfinity(constraint) || constraint < 0)
							return fallback;

						var value = (nfloat)constraint;

						// Guard against overflow to infinity/NaN or negative after casting.
						var valueAsDouble = (double)value;
						if (double.IsNaN(valueAsDouble) || double.IsInfinity(valueAsDouble) || value < 0)
							return fallback;

						return value;
					}

					var previousFrame = collectionView.Frame;
					try
					{
						// Give the CollectionView a finite available size so the layout calculates correctly
						var frameWidth = ClampConstraint(measurementWidth, UIView.UILayoutFittingExpandedSize.Width);
						var frameHeight = ClampConstraint(measurementHeight, UIView.UILayoutFittingExpandedSize.Height);

						collectionView.Frame = new CoreGraphics.CGRect(0, 0, frameWidth, frameHeight);
						collectionView.SetNeedsLayout();
						collectionView.LayoutIfNeeded();

						// Re-read the content size now that the layout has run
						contentSize = Controller.GetSize();
					}
					finally
					{
						// Always restore the original frame
						collectionView.Frame = previousFrame;
					}

					// If the forced layout produced a valid size, return it directly
					if ((scrollDirection == UICollectionViewScrollDirection.Vertical && contentSize.Height > 0) ||
						(scrollDirection == UICollectionViewScrollDirection.Horizontal && contentSize.Width > 0))
					{
						return contentSize;
					}
				}

				// Fallback: return the expansive size the collection view wants by default
				// to get it to start measuring its content
				var desiredSize = base.GetDesiredSize(widthConstraint, heightConstraint);
				if (scrollDirection == UICollectionViewScrollDirection.Vertical)
				{
					contentSize.Height = desiredSize.Height;
				}
				else
				{
					contentSize.Width = desiredSize.Width;

					// Horizontal items fill the native frame, so its height is not an intrinsic measurement.
					// Bootstrap unbounded Auto rows from real item content rather than a synthetic frame.
					contentSize.Height = VirtualView is CollectionView { ItemsLayout: LinearItemsLayout or GridItemsLayout } &&
						!double.IsFinite(measurementHeight)
							? MeasureHorizontalContentHeight()
							: 0;
				}
			}

			return contentSize;
		}

		double MeasureHorizontalContentHeight()
		{
			var source = Controller.ItemsSource;
			for (int group = 0; group < source.GroupCount; group++)
			{
				if (source.ItemCountInGroup(group) == 0)
				{
					continue;
				}

				using var indexPath = NSIndexPath.Create(group, 0);
				var item = source[indexPath];
				double itemHeight;
				if (ItemsView.ItemTemplate is null)
				{
					using var cell = new HorizontalDefaultCell2(CGRect.Empty);
					cell.Label.Text = item.ToString();
					itemHeight = cell.Label.IntrinsicContentSize.Height;
				}
				else
				{
					using var measurementCell = new TemplatedCell2(CGRect.Empty);
					try
					{
						measurementCell.Bind(ItemsView.ItemTemplate, item, ItemsView);
						itemHeight = measurementCell.PlatformHandler.VirtualView.Measure(
							double.PositiveInfinity, double.PositiveInfinity).Height;
					}
					finally
					{
						var view = measurementCell.PlatformHandler?.VirtualView;
						measurementCell.Unbind();
						view?.DisconnectHandlers();
					}
				}

				return ItemsView is CollectionView { ItemsLayout: GridItemsLayout grid }
					? itemHeight * grid.Span + grid.VerticalItemSpacing * (grid.Span - 1)
					: itemHeight;
			}

			return 0;
		}
	}
}
