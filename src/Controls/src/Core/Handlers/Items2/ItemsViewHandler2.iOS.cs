#nullable disable
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
// Pack 5 boolean fields into a single byte using bit flags
// Used to reduce struct size as much as possible.
[Flags]
internal enum LayoutFlags : byte
{
    None = 0,
    IsGrouped = 1,
    HasGroupHeader = 2,
    HasGroupFooter = 4,
    HasHeader = 8,
    HasFooter = 16
}

/// <summary>
/// Cache key for UICollectionViewLayout instances to avoid recreating layouts unnecessarily.
/// </summary>
internal readonly struct LayoutCacheKey : IEquatable<LayoutCacheKey>
{
    public readonly LayoutFlags Flags;
	
    public readonly byte Orientation;
    public readonly byte SizingStrategy;
    public readonly byte SnapType;
    public readonly byte SnapAlignment;
	
    public readonly short Span;
	
    public readonly double VerticalItemSpacing;
    public readonly double HorizontalItemSpacing;
    public readonly double ItemSpacing;
	
    public readonly int LayoutTypeHash;
	
    public readonly int HeaderTemplateHashCode;
    public readonly int FooterTemplateHashCode;
    public readonly int ItemTemplateHashCode;
    public readonly int GroupHeaderTemplateHashCode;
    public readonly int GroupFooterTemplateHashCode;
    public readonly int HeaderHashCode;
    public readonly int FooterHashCode;
	
    public bool IsGrouped => (Flags & LayoutFlags.IsGrouped) != 0;
    public bool HasGroupHeader => (Flags & LayoutFlags.HasGroupHeader) != 0;
    public bool HasGroupFooter => (Flags & LayoutFlags.HasGroupFooter) != 0;
    public bool HasHeader => (Flags & LayoutFlags.HasHeader) != 0;
    public bool HasFooter => (Flags & LayoutFlags.HasFooter) != 0;

    public LayoutCacheKey(ItemsView itemsView)
    {
        var itemsLayout = (itemsView as StructuredItemsView)?.ItemsLayout;
        var sizingStrategy = (itemsView as StructuredItemsView)?.ItemSizingStrategy ?? ItemSizingStrategy.MeasureFirstItem;
        
        LayoutFlags flags = LayoutFlags.None;
        
        LayoutTypeHash = itemsLayout?.GetType().GetHashCode() ?? 0;
        SizingStrategy = (byte)sizingStrategy;

        if (itemsLayout is GridItemsLayout gridLayout)
        {
            Orientation = (byte)gridLayout.Orientation;
            Span = (short)Math.Min(gridLayout.Span, short.MaxValue);
            VerticalItemSpacing = gridLayout.VerticalItemSpacing;
            HorizontalItemSpacing = gridLayout.HorizontalItemSpacing;
            ItemSpacing = 0;
            SnapType = (byte)gridLayout.SnapPointsType;
            SnapAlignment = (byte)gridLayout.SnapPointsAlignment;
        }
        else if (itemsLayout is LinearItemsLayout linearLayout)
        {
            Orientation = (byte)linearLayout.Orientation;
            Span = 1;
            VerticalItemSpacing = 0;
            HorizontalItemSpacing = 0;
            ItemSpacing = linearLayout.ItemSpacing;
            SnapType = (byte)linearLayout.SnapPointsType;
            SnapAlignment = (byte)linearLayout.SnapPointsAlignment;
        }
        else
        {
            Orientation = (byte)ItemsLayoutOrientation.Vertical;
            Span = 1;
            VerticalItemSpacing = 0;
            HorizontalItemSpacing = 0;
            ItemSpacing = 0;
            SnapType = (byte)SnapPointsType.None;
            SnapAlignment = (byte)SnapPointsAlignment.Start;
        }

        // Handle grouping
        if (itemsView is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped)
        {
            flags |= LayoutFlags.IsGrouped;
            
            if (groupableItemsView.GroupHeaderTemplate is not null)
            {
                flags |= LayoutFlags.HasGroupHeader;
                GroupHeaderTemplateHashCode = groupableItemsView.GroupHeaderTemplate.GetHashCode();
            }
            else
            {
                GroupHeaderTemplateHashCode = 0;
            }
            
            if (groupableItemsView.GroupFooterTemplate is not null)
            {
                flags |= LayoutFlags.HasGroupFooter;
                GroupFooterTemplateHashCode = groupableItemsView.GroupFooterTemplate.GetHashCode();
            }
            else
            {
                GroupFooterTemplateHashCode = 0;
            }
        }
        else
        {
            GroupHeaderTemplateHashCode = 0;
            GroupFooterTemplateHashCode = 0;
        }

        // Handle headers/footers
        if (itemsView is StructuredItemsView structuredItemsView)
        {
            if (structuredItemsView.Header is not null || structuredItemsView.HeaderTemplate is not null)
            {
                flags |= LayoutFlags.HasHeader;
                HeaderTemplateHashCode = structuredItemsView.HeaderTemplate?.GetHashCode() ?? 0;
                HeaderHashCode = structuredItemsView.Header?.GetHashCode() ?? 0;
            }
            else
            {
                HeaderTemplateHashCode = 0;
                HeaderHashCode = 0;
            }
            
            if (structuredItemsView.Footer is not null || structuredItemsView.FooterTemplate is not null)
            {
                flags |= LayoutFlags.HasFooter;
                FooterTemplateHashCode = structuredItemsView.FooterTemplate?.GetHashCode() ?? 0;
                FooterHashCode = structuredItemsView.Footer?.GetHashCode() ?? 0;
            }
            else
            {
                FooterTemplateHashCode = 0;
                FooterHashCode = 0;
            }
        }
        else
        {
            HeaderTemplateHashCode = 0;
            FooterTemplateHashCode = 0;
            HeaderHashCode = 0;
            FooterHashCode = 0;
        }

        // Track item template changes
        ItemTemplateHashCode = itemsView.ItemTemplate?.GetHashCode() ?? 0;

        Flags = flags;
    }

    public bool Equals(LayoutCacheKey other)
    {
        // Compare the most likely to differ fields first
        return LayoutTypeHash == other.LayoutTypeHash &&
               ItemTemplateHashCode == other.ItemTemplateHashCode &&
               Flags == other.Flags &&
               Orientation == other.Orientation &&
               SizingStrategy == other.SizingStrategy &&
               Span == other.Span &&
               SnapType == other.SnapType &&
               SnapAlignment == other.SnapAlignment &&
               // Use bitwise comparison for doubles to avoid epsilon issues in hash contexts
               VerticalItemSpacing.Equals(other.VerticalItemSpacing) &&
               HorizontalItemSpacing.Equals(other.HorizontalItemSpacing) &&
               ItemSpacing.Equals(other.ItemSpacing) &&
               HeaderTemplateHashCode == other.HeaderTemplateHashCode &&
               FooterTemplateHashCode == other.FooterTemplateHashCode &&
               GroupHeaderTemplateHashCode == other.GroupHeaderTemplateHashCode &&
               GroupFooterTemplateHashCode == other.GroupFooterTemplateHashCode &&
               HeaderHashCode == other.HeaderHashCode &&
               FooterHashCode == other.FooterHashCode;
    }

    public override bool Equals(object obj)
    {
        return obj is LayoutCacheKey other && Equals(other);
    }

    public override int GetHashCode()
    {
        // Combine most discriminating values first
        var hashLayout = HashCode.Combine(
            LayoutTypeHash,
            ItemTemplateHashCode,
            (int)Flags,
            Orientation | (SizingStrategy << 8) | (Span << 16) // Pack small values
        );
        
        var hashItemSpacing = HashCode.Combine(
            VerticalItemSpacing.GetHashCode(),
            HorizontalItemSpacing.GetHashCode(),
            ItemSpacing.GetHashCode(),
            HeaderTemplateHashCode
        );
        
        var hashHeaderFooter = HashCode.Combine(
            FooterTemplateHashCode,
            GroupHeaderTemplateHashCode,
            GroupFooterTemplateHashCode,
            HeaderHashCode,
            FooterHashCode
        );

        return HashCode.Combine(hashLayout, hashItemSpacing, hashHeaderFooter);
    }
	
    public override string ToString()
    {
        return $"LayoutCacheKey(Flags: {Flags}, Orientation: {(ItemsLayoutOrientation)Orientation}, " +
               $"Span: {Span}, Strategy: {(ItemSizingStrategy)SizingStrategy})";
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