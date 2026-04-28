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
			[Controls.ItemsView.ItemsUpdatingScrollModeProperty.PropertyName] = MapItemsUpdatingScrollMode
		};

		UICollectionViewLayout _layout;

		protected override void DisconnectHandler(UIView platformView)
		{
			ItemsView.ScrollToRequested -= ScrollToRequested;
			_layout = null;
			Controller?.DisposeItemsSource();
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
		}

		protected virtual void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			int section = 0, item = 0;
			UICollectionViewScrollPosition scrollPosition = UICollectionViewScrollPosition.None;

			using (var indexPath = DetermineIndex(args))
			{
				if (!IsIndexPathValid(indexPath))
				{
					// Specified path wasn't valid, or item wasn't found
					return;
				}

				var scrollDirection = Controller.GetScrollDirection();
				scrollPosition = Items.ScrollToPositionExtensions.ToCollectionViewScrollPosition(args.ScrollToPosition, scrollDirection);

				// Capture section and item as ints before the using block disposes the indexPath
				section = (int)indexPath.Section;
				item = (int)indexPath.Item;

				Controller.CollectionView.ScrollToItem(indexPath,
					scrollPosition, args.IsAnimated);
			}

			// After non-animated scroll, arm KVO restore to recover from silent Mac Catalyst contentOffset shift
#if MACCATALYST
			if (Controller?.CollectionView is MauiCollectionView mauiCV)
			{
				mauiCV.ClearPendingScrollRestore();

				if (!args.IsAnimated)
				{
					mauiCV.SetPendingScrollRestore(section, item, scrollPosition);
				}
			}
#endif

			NSIndexPath DetermineIndex(ScrollToRequestEventArgs args)
			{
				if (args.Mode == ScrollToMode.Position)
				{
					if (args.GroupIndex == -1)
					{
						return NSIndexPath.Create(0, args.Index);
					}

					return NSIndexPath.Create(args.GroupIndex, args.Index);
				}

				return Controller.GetIndexForItem(args.Item);
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
				var collectionView = Controller.CollectionView;

				// When the CollectionView has not yet been added to a window (pre-mount measurement),
				// UICollectionViewCompositionalLayout hasn't run a layout pass and therefore
				// CollectionViewContentSize is still zero. Force a layout pass with the given constraints
				// so the layout can compute actual content size from its items.
				if (collectionView.Window == null)
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
						var frameWidth = ClampConstraint(widthConstraint, UIView.UILayoutFittingExpandedSize.Width);
						var frameHeight = ClampConstraint(heightConstraint, UIView.UILayoutFittingExpandedSize.Height);

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

					// For horizontal layouts, items use FractionalHeight(1f), meaning their height equals
					// the CollectionView's current frame height. When no items are loaded (Width == 0),
					// contentSize.Height reflects the container's frame height rather than actual content.
					// This creates a circular sizing issue in Auto-height containers: the frame grows based
					// on the incorrect content height and stays locked in even after items load.
					// Reset to 0 so that MinimumHeight / HeightRequest can determine the correct size.
					contentSize.Height = 0;
				}
			}

			return contentSize;
		}
	}
}
