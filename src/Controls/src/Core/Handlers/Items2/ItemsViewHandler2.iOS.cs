#nullable disable
using System;
using System.Collections.Generic;
using System.Text;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	internal struct LayoutCacheKey : IEquatable<LayoutCacheKey>
	{
		public readonly bool IsGrouped;
		public readonly bool HasGroupHeader;
		public readonly bool HasGroupFooter;
		public readonly bool HasHeader;
		public readonly bool HasFooter;
		public readonly ItemsLayoutOrientation Orientation;
		public readonly int Span;
		public readonly double VerticalItemSpacing;
		public readonly double HorizontalItemSpacing;
		public readonly double ItemSpacing;
		public readonly Type LayoutType;
		public readonly ItemSizingStrategy SizingStrategy;
		public readonly SnapPointsType SnapType;
		public readonly SnapPointsAlignment SnapAlignment;

		public LayoutCacheKey(ItemsView itemsView)
		{
			var itemsLayout = (itemsView as StructuredItemsView)?.ItemsLayout;
			var sizingStrategy = (itemsView as StructuredItemsView)?.ItemSizingStrategy ??
			                     ItemSizingStrategy.MeasureFirstItem;

			LayoutType = itemsLayout?.GetType();
			SizingStrategy = sizingStrategy;

			if (itemsLayout is GridItemsLayout gridLayout)
			{
				Orientation = gridLayout.Orientation;
				Span = gridLayout.Span;
				VerticalItemSpacing = gridLayout.VerticalItemSpacing;
				HorizontalItemSpacing = gridLayout.HorizontalItemSpacing;
				ItemSpacing = 0;
				SnapType = gridLayout.SnapPointsType;
				SnapAlignment = gridLayout.SnapPointsAlignment;
			}
			else if (itemsLayout is LinearItemsLayout linearLayout)
			{
				Orientation = linearLayout.Orientation;
				Span = 1;
				VerticalItemSpacing = 0;
				HorizontalItemSpacing = 0;
				ItemSpacing = linearLayout.ItemSpacing;
				SnapType = linearLayout.SnapPointsType;
				SnapAlignment = linearLayout.SnapPointsAlignment;
			}
			else
			{
				Orientation = ItemsLayoutOrientation.Vertical;
				Span = 1;
				VerticalItemSpacing = 0;
				HorizontalItemSpacing = 0;
				ItemSpacing = 0;
				SnapType = SnapPointsType.None;
				SnapAlignment = SnapPointsAlignment.Start;
			}

			if (itemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped)
			{
				IsGrouped = true;
				HasGroupHeader = groupableItemsView.GroupHeaderTemplate is not null;
				HasGroupFooter = groupableItemsView.GroupFooterTemplate is not null;
			}
			else
			{
				IsGrouped = false;
				HasGroupHeader = false;
				HasGroupFooter = false;
			}

			if (itemsView is StructuredItemsView structuredItemsView)
			{
				HasHeader = structuredItemsView.Header is not null || structuredItemsView.HeaderTemplate is not null;
				HasFooter = structuredItemsView.Footer is not null || structuredItemsView.FooterTemplate is not null;
			}
			else
			{
				HasHeader = false;
				HasFooter = false;
			}
		}

		public bool Equals(LayoutCacheKey other)
		{
			return IsGrouped == other.IsGrouped &&
			       HasGroupHeader == other.HasGroupHeader &&
			       HasGroupFooter == other.HasGroupFooter &&
			       HasHeader == other.HasHeader &&
			       HasFooter == other.HasFooter &&
			       Orientation == other.Orientation &&
			       Span == other.Span &&
			       Math.Abs(VerticalItemSpacing - other.VerticalItemSpacing) < double.Epsilon &&
			       Math.Abs(HorizontalItemSpacing - other.HorizontalItemSpacing) < double.Epsilon &&
			       Math.Abs(ItemSpacing - other.ItemSpacing) < double.Epsilon &&
			       LayoutType == other.LayoutType &&
			       SizingStrategy == other.SizingStrategy &&
			       SnapType == other.SnapType &&
			       SnapAlignment == other.SnapAlignment;
		}

		public override bool Equals(object obj)
		{
			return obj is LayoutCacheKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			var hash = new HashCode();
			hash.Add(IsGrouped);
			hash.Add(HasGroupHeader);
			hash.Add(HasGroupFooter);
			hash.Add(HasHeader);
			hash.Add(HasFooter);
			hash.Add(Orientation);
			hash.Add(Span);
			hash.Add(VerticalItemSpacing);
			hash.Add(HorizontalItemSpacing);
			hash.Add(ItemSpacing);
			hash.Add(LayoutType);
			hash.Add(SizingStrategy);
			hash.Add(SnapType);
			hash.Add(SnapAlignment);
			return hash.ToHashCode();
		}
	}

	public abstract partial class ItemsViewHandler2<TItemsView> : ViewHandler<TItemsView, UIView>
		where TItemsView : ItemsView
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
			[Controls.ItemsView.HorizontalScrollBarVisibilityProperty.PropertyName] =
				MapHorizontalScrollBarVisibility,
			[Controls.ItemsView.VerticalScrollBarVisibilityProperty.PropertyName] = MapVerticalScrollBarVisibility,
			[Controls.ItemsView.ItemTemplateProperty.PropertyName] = MapItemTemplate,
			[Controls.ItemsView.EmptyViewProperty.PropertyName] = MapEmptyView,
			[Controls.ItemsView.EmptyViewTemplateProperty.PropertyName] = MapEmptyViewTemplate,
			[Controls.ItemsView.FlowDirectionProperty.PropertyName] = MapFlowDirection,
			[Controls.ItemsView.IsVisibleProperty.PropertyName] = MapIsVisible,
			[Controls.ItemsView.ItemsUpdatingScrollModeProperty.PropertyName] = MapItemsUpdatingScrollMode
		};

		UICollectionViewLayout _layout;
		LayoutCacheKey _lastLayoutKey;
		
		protected override void DisconnectHandler(UIView platformView)
		{
			ItemsView.ScrollToRequested -= ScrollToRequested;
			_layout = null;
			_lastLayoutKey = default;
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

		protected abstract ItemsViewController2<TItemsView> CreateController(TItemsView newElement,
			UICollectionViewLayout layout);

		protected override UIView CreatePlatformView()
		{
			var controllerView = Controller?.View ??
			                     throw new InvalidOperationException(
				                     "ItemsViewController2's view should not be null at this point.");
			return controllerView;
		}

		public static void MapItemsSource(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			MapItemsUpdatingScrollMode(handler, itemsView);
			handler.Controller?.UpdateItemsSource();
		}

		public static void MapHorizontalScrollBarVisibility(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.CollectionView?.UpdateHorizontalScrollBarVisibility(itemsView
				.HorizontalScrollBarVisibility);
		}

		public static void MapVerticalScrollBarVisibility(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.CollectionView?.UpdateVerticalScrollBarVisibility(itemsView
				.VerticalScrollBarVisibility);
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
		}

		public static void MapIsVisible(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.UpdateVisibility();
		}

		public static void MapItemsUpdatingScrollMode(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			// TODO: Fix handler._layout.ItemsUpdatingScrollMode = itemsView.ItemsUpdatingScrollMode;
		}

		//TODO: this is being called 2 times on startup, one from OnCreatePlatformView and other from the mapper for the layout
		protected virtual void UpdateLayout()
		{
			var currentKey = new LayoutCacheKey(ItemsView);

			// Only recreate layout if something has changed
			if (!currentKey.Equals(_lastLayoutKey) || _layout is null)
			{
				_layout = SelectLayout();
				_lastLayoutKey = currentKey;
			}
			
			Controller?.UpdateLayout(_layout);
		}

		protected virtual void ScrollToRequested(object sender, ScrollToRequestEventArgs args)
		{
			using (var indexPath = DetermineIndex(args))
			{
				if (!IsIndexPathValid(indexPath))
				{
					// Specified path wasn't valid, or item wasn't found
					return;
				}

				var position = Items.ScrollToPositionExtensions.ToCollectionViewScrollPosition(args.ScrollToPosition,
					UICollectionViewScrollDirection.Vertical);

				Controller.CollectionView.ScrollToItem(indexPath,
					position, args.IsAnimated);
			}

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
			if (indexPath.Item < 0 || indexPath.Section < 0)
			{
				return false;
			}

			var collectionView = Controller.CollectionView;
			if (indexPath.Section >= collectionView.NumberOfSections())
			{
				return false;
			}

			if (indexPath.Item >= collectionView.NumberOfItemsInSection(indexPath.Section))
			{
				return false;
			}

			return true;
		}

		public override Size GetDesiredSize(double widthConstraint, double heightConstraint)
		{
			var contentSize = Controller.GetSize();

			// If contentSize comes back null, it means none of the content has been realized yet;
			// we need to return the expansive size the collection view wants by default to get
			// it to start measuring its content
			if (contentSize.Height == 0 || contentSize.Width == 0)
			{
				return base.GetDesiredSize(widthConstraint, heightConstraint);
			}

			// Our target size is the smaller of it and the constraints
			var width = contentSize.Width <= widthConstraint ? contentSize.Width : widthConstraint;
			var height = contentSize.Height <= heightConstraint ? contentSize.Height : heightConstraint;

			IView virtualView = VirtualView;

			width = ViewHandlerExtensions.ResolveConstraints(width, virtualView.Width, virtualView.MinimumWidth,
				virtualView.MaximumWidth);
			height = ViewHandlerExtensions.ResolveConstraints(height, virtualView.Height, virtualView.MinimumHeight,
				virtualView.MaximumHeight);

			return new Size(width, height);
		}
	}
}