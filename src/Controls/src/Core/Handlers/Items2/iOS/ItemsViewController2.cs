#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items2
{
	public abstract class ItemsViewController2<TItemsView> : UICollectionViewController, Items.MauiCollectionView.ICustomMauiCollectionViewDelegate
	where TItemsView : ItemsView
	{
		public const int EmptyTag = 333;
		readonly WeakReference<TItemsView> _itemsView;

		public Items.IItemsViewSource ItemsSource { get; protected set; }
		public TItemsView ItemsView => _itemsView.GetTargetOrDefault();

		// ItemsViewLayout provides an accessor to the typed UICollectionViewLayout. It's also important to note that the
		// initial UICollectionViewLayout which is passed in to the ItemsViewController2 (and accessed via the Layout property)
		// _does not_ get updated when the layout is updated for the CollectionView. That property only refers to the
		// original layout. So it's unlikely that you would ever want to use .Layout; use .ItemsViewLayout instead.
		// See https://developer.apple.com/documentation/uikit/uicollectionviewcontroller/1623980-collectionviewlayout
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		protected UICollectionViewLayout ItemsViewLayout { get; set; }

		bool _initialized;
		bool _disposed;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		List<string> _cellReuseIds = new List<string>();
		CGSize _previousContentSize;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		protected UICollectionViewDelegateFlowLayout Delegator { get; set; }

		protected UICollectionViewScrollDirection ScrollDirection { get; private set; } =
			UICollectionViewScrollDirection.Vertical;

		protected ItemsViewController2(TItemsView itemsView, UICollectionViewLayout layout) : base(layout)
		{
			_itemsView = new(itemsView);
			ItemsViewLayout = layout;
		}

		public void UpdateLayout(UICollectionViewLayout newLayout)
		{
			// Ignore calls to this method if the new layout is the same as the old one
			if (CollectionView.CollectionViewLayout == newLayout)
				return;

			if (newLayout is UICollectionViewCompositionalLayout compositionalLayout)
			{
				// Note: on carousel layout, the scroll direction is always vertical to achieve horizontal paging with snapping.
				// Thanks to it, we can use OrthogonalScrollingBehavior.GroupPagingCentered to scroll the section horizontally.
				// And even if CarouselView is vertically oriented, each section scrolls horizontally — which results in the carousel-style behavior.
				ScrollDirection = compositionalLayout.Configuration.ScrollDirection;
			}

			ItemsViewLayout = newLayout;
			_initialized = false;

			EnsureLayoutInitialized();

			if (_initialized)
			{
				// Reload the data so the currently visible cells get laid out according to the new layout
				CollectionView.ReloadData();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				ItemsSource?.Dispose();

				CollectionView.Delegate = null;
				Delegator?.Dispose();

				ItemsViewLayout?.Dispose();
				CollectionView?.Dispose();
			}

			base.Dispose(disposing);
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			if (ItemsSource.ItemCount == 0)
			{
				return GetEmptyViewCell(collectionView, indexPath);
			}

			var cell = collectionView.DequeueReusableCell(DetermineCellReuseId(indexPath), indexPath) as UICollectionViewCell;

			// We need to get the index path that is adjusted for the item source
			// Some ItemsView like CarouselView have a loop feature that will make the index path different from the item source
			var indexpathAdjusted = GetAdjustedIndexPathForItemSource(indexPath);

			if (cell is TemplatedCell2 TemplatedCell2)
			{
				TemplatedCell2.ScrollDirection = ScrollDirection;

				TemplatedCell2.Bind(ItemsView.ItemTemplate, ItemsSource[indexpathAdjusted], ItemsView);
			}
			else if (cell is DefaultCell2 DefaultCell2)
			{
				DefaultCell2.Label.Text = ItemsSource[indexpathAdjusted].ToString();
			}

			return cell;
		}

		protected UICollectionViewCell GetEmptyViewCell(UICollectionView collectionView, NSIndexPath indexPath, double headerHeight = 0, double footerHeight = 0)
		{
			if (ItemsSource.ItemCount == 0 && ItemsView?.EmptyView is View emptyView)
			{
				var emptyViewCell = collectionView.DequeueReusableCell("EmptyViewCell", indexPath) as TemplatedCell2;
				emptyViewCell.ScrollDirection = ScrollDirection;
				emptyViewCell.Unbind();
				emptyViewCell.PlatformHandler = null;

				emptyViewCell.Bind(emptyView, ItemsView);
				return emptyViewCell;
			}
			return null;
		}

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			// If the ItemsSource is empty, we need to return 1 so that the empty view can be displayed
			if (ItemsSource.ItemCount == 0 && ItemsView?.EmptyView is not null)
			{
				return 1;
			}

			return ItemsSource.ItemCountInGroup(section);
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			ItemsSource = CreateItemsViewSource();

			if (!(OperatingSystem.IsIOSVersionAtLeast(11) || OperatingSystem.IsMacCatalystVersionAtLeast(11)
#if TVOS
				|| OperatingSystem.IsTvOSVersionAtLeast(11)
#endif
			))
			{
				AutomaticallyAdjustsScrollViewInsets = false;
			}
			else
			{
				// We set this property to keep iOS from trying to be helpful about insetting all the 
				// CollectionView content when we're in landscape mode (to avoid the notch)
				// The SetUseSafeArea Platform Specific is already taking care of this for us 
				// That said, at some point it's possible folks will want a PS for controlling this behavior
				CollectionView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
			}

			RegisterViewTypes();

			EnsureLayoutInitialized();
		}

		public override void LoadView()
		{
			base.LoadView();
			var collectionView = new Items.MauiCollectionView(CGRect.Empty, ItemsViewLayout);
			collectionView.SetCustomDelegate(this);
			CollectionView = collectionView;
		}

		public override void ViewWillLayoutSubviews()
		{
			if (CollectionView is Items.MauiCollectionView { NeedsCellLayout: true } collectionView)
			{
				InvalidateLayoutIfItemsMeasureChanged();
				collectionView.NeedsCellLayout = false;
			}

			base.ViewWillLayoutSubviews();
			InvalidateMeasureIfContentSizeChanged();
		}

		void InvalidateLayoutIfItemsMeasureChanged()
		{
			var collectionView = CollectionView;
			var visibleCells = collectionView.VisibleCells;
			List<NSIndexPath> invalidatedPaths = null;

			var visibleCellsLength = visibleCells.Length;
			for (int n = 0; n < visibleCellsLength; n++)
			{
				if (visibleCells[n] is TemplatedCell2 { MeasureInvalidated: true } cell)
				{
					invalidatedPaths ??= new List<NSIndexPath>(visibleCellsLength);
					var path = collectionView.IndexPathForCell(cell);
					invalidatedPaths.Add(path);
				}
			}

			if (invalidatedPaths != null)
			{
				var layoutInvalidationContext = new UICollectionViewLayoutInvalidationContext();
				layoutInvalidationContext.InvalidateItems(invalidatedPaths.ToArray());
				collectionView.CollectionViewLayout.InvalidateLayout(layoutInvalidationContext);
			}
		}

		void Items.MauiCollectionView.ICustomMauiCollectionViewDelegate.MovedToWindow(UIView view)
		{
			if (CollectionView?.Window != null)
			{
				AttachingToWindow();
			}
			else
			{
				DetachingFromWindow();
			}
		}

		internal void DisposeItemsSource()
		{
			ItemsSource?.Dispose();
			ItemsSource = new Items.EmptySource();
			CollectionView.ReloadData();
		}

		void EnsureLayoutInitialized()
		{
			if (_initialized)
			{
				return;
			}

			_initialized = true;

			Delegator = CreateDelegator();
			CollectionView.Delegate = Delegator;

			CollectionView.SetCollectionViewLayout(ItemsViewLayout, false);
		}

		protected virtual UICollectionViewDelegateFlowLayout CreateDelegator()
		{
			return new ItemsViewDelegator2<TItemsView, ItemsViewController2<TItemsView>>(ItemsViewLayout, this);
		}

		protected virtual Items.IItemsViewSource CreateItemsViewSource()
		{
			return Items.ItemsSourceFactory.Create(ItemsView.ItemsSource, this);
		}

		public virtual void UpdateItemsSource()
		{
			ItemsSource?.Dispose();
			ItemsSource = CreateItemsViewSource();

			CollectionView.ReloadData();
			CollectionView.CollectionViewLayout.InvalidateLayout();

			(ItemsView as IView)?.InvalidateMeasure();
		}

		public virtual void UpdateFlowDirection()
		{
			CollectionView.UpdateFlowDirection(ItemsView);
			Layout.InvalidateLayout();
		}

		public override nint NumberOfSections(UICollectionView collectionView)
		{
			// https://github.com/dotnet/maui/issues/26997
			// On iOS versions 15 and 16, this method is called immediately after the base LoadView method is invoked.
			// At this point, ItemsSource was not set, which caused a crash when accessed.
			// To handle this scenario, we check if ItemsSource is null before proceeding.
			// In iOS 17 and later versions, this behavior works properly, as the method is no longer called prematurely.
			// To ensure compatibility with older iOS versions, we include this null check and return 0 if ItemsSource is null.
			if (ItemsSource.ItemCount == 0 && ItemsView?.EmptyView is not null)
			{
				return 1;
			}

			return ItemsSource.GroupCount;
		}


		public virtual NSIndexPath GetIndexForItem(object item)
		{
			return ItemsSource.GetIndexForItem(item);
		}

		protected object GetItemAtIndex(NSIndexPath index)
		{
			return ItemsSource[index];
		}

		protected virtual string DetermineCellReuseId(NSIndexPath indexPath)
		{
			if (ItemsView.ItemTemplate != null)
			{
				var item = ItemsSource[indexPath];

				var dataTemplate = ItemsView.ItemTemplate.SelectDataTemplate(item, ItemsView);

				var orientation = ScrollDirection == UICollectionViewScrollDirection.Horizontal ? "Horizontal" : "Vertical";
				(Type cellType, var cellTypeReuseId) = DetermineTemplatedCellType();
				var reuseId = $"{cellTypeReuseId}.{orientation}.{dataTemplate.Id}";

				if (!_cellReuseIds.Contains(reuseId))
				{
					CollectionView.RegisterClassForCell(cellType, new NSString(reuseId));
					_cellReuseIds.Add(reuseId);
				}

				return reuseId;
			}

			return ScrollDirection == UICollectionViewScrollDirection.Horizontal ? HorizontalDefaultCell2.ReuseId : VerticalDefaultCell2.ReuseId;
		}

		private protected virtual (Type CellType, string CellTypeReuseId) DetermineTemplatedCellType()
			=> (typeof(TemplatedCell2), TemplatedCell2.ReuseId);

		protected virtual void RegisterViewTypes()
		{
			CollectionView.RegisterClassForCell(typeof(HorizontalDefaultCell2), HorizontalDefaultCell2.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalDefaultCell2), VerticalDefaultCell2.ReuseId);
			CollectionView.RegisterClassForCell(typeof(HorizontalCell2), HorizontalCell2.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalCell2), VerticalCell2.ReuseId);
			CollectionView.RegisterClassForCell(typeof(TemplatedCell2), "EmptyViewCell");
		}

		protected abstract bool IsHorizontal { get; }

		void InvalidateMeasureIfContentSizeChanged()
		{
			if (CollectionView?.CollectionViewLayout?.CollectionViewContentSize is not { } contentSize)
			{
				return;
			}

			var previousContentSize = _previousContentSize;
			_previousContentSize = contentSize;

			bool widthChanged = previousContentSize.Width != contentSize.Width;
			bool heightChanged = previousContentSize.Height != contentSize.Height;

			if (_initialized && (widthChanged || heightChanged))
			{
				if (CollectionView?.Bounds is not { } bounds)
				{
					return;
				}

				var cvWidth = bounds.Width;
				var cvHeight = bounds.Height;
				bool invalidate = false;

				// If both the previous content size and the current content size are larger
				// than the UICollectionView size, then we know that we're already maxed out and the 
				// CollectionView items are scrollable. There's no reason to force an invalidation
				// of the CollectionView to expand/contract it.

				// If either size is smaller than that, we need to invalidate to ensure that the 
				// CollectionView is re-measured and set to the correct size.

				if (widthChanged && (contentSize.Width <= cvWidth || previousContentSize.Width <= cvWidth))
				{
					invalidate = true;
				}
				else if (heightChanged && (contentSize.Height <= cvHeight || previousContentSize.Height <= cvHeight))
				{
					invalidate = true;
				}

				if (invalidate)
				{
					(ItemsView as IView)?.InvalidateMeasure();
				}
			}
		}

		internal Size GetSize()
		{
			return CollectionView.CollectionViewLayout.CollectionViewContentSize.ToSize();
		}

		internal void UpdateView(object view, DataTemplate viewTemplate, ref UIView uiView, ref VisualElement formsElement)
		{
			// Is view set on the ItemsView?
			if (view is null && (viewTemplate is null || viewTemplate is DataTemplateSelector))
			{
				if (formsElement != null)
				{
					//Platform.GetRenderer(formsElement)?.DisposeRendererAndChildren();
				}

				uiView = null;
				formsElement?.Handler?.DisconnectHandler();
				formsElement = null;
			}
			else
			{
				// Create the native renderer for the view, and keep the actual Forms element (if any)
				// around for updating the layout later
				(uiView, formsElement) = Items.TemplateHelpers.RealizeView(view, viewTemplate, ItemsView);
			}
		}

		internal protected virtual void UpdateVisibility()
		{
			if (ItemsView.IsVisible)
			{
				if (CollectionView.Hidden)
				{
					CollectionView.ReloadData();
					CollectionView.Hidden = false;
					Layout.InvalidateLayout();
					CollectionView.LayoutIfNeeded();
				}
			}
			else
			{
				CollectionView.Hidden = true;
			}
		}

		private protected virtual void AttachingToWindow()
		{

		}

		private protected virtual void DetachingFromWindow()
		{
		}

		private protected virtual NSIndexPath GetAdjustedIndexPathForItemSource(NSIndexPath indexPath)
		{
			return indexPath;
		}

		internal virtual void CellDisplayingEndedFromDelegate(UICollectionViewCell cell, NSIndexPath indexPath)
		{
			if (cell is TemplatedCell2 templatedCell2 &&
				(templatedCell2.PlatformHandler?.VirtualView as View)?.BindingContext is { } bindingContext)
			{
				// We want to unbind a cell that is no longer present in the items source. Unfortunately
				// it's too expensive to check directly, so let's check that the current binding context
				// matches the item at a given position.

				indexPath = GetAdjustedIndexPathForItemSource(indexPath);

				var itemsSource = ItemsSource;
				if (itemsSource is null ||
					!Items.IndexPathHelpers.IsIndexPathValid(itemsSource, indexPath) ||
					!Equals(itemsSource[indexPath], bindingContext))
				{
					templatedCell2.Unbind();
				}
			}
		}
	}
}
