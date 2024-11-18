#nullable disable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using PassKit;
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
		bool _isEmpty = true;
		bool _emptyViewDisplayed;
		bool _disposed;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		UIView _emptyUIView;
		VisualElement _emptyViewFormsElement;
		List<string> _cellReuseIds = new List<string>();

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

				_emptyUIView?.Dispose();
				_emptyUIView = null;

				_emptyViewFormsElement = null;

				ItemsViewLayout?.Dispose();
				CollectionView?.Dispose();
			}

			base.Dispose(disposing);
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
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

		public override nint GetItemsCount(UICollectionView collectionView, nint section)
		{
			CheckForEmptySource();

			return ItemsSource.ItemCountInGroup(section);
		}

		void CheckForEmptySource()
		{
			var wasEmpty = _isEmpty;

			_isEmpty = ItemsSource.ItemCount == 0;

			if (wasEmpty != _isEmpty)
			{
				UpdateEmptyViewVisibility(_isEmpty);
			}

			if (wasEmpty && !_isEmpty)
			{
				// If we're going from empty to having stuff, it's possible that we've never actually measured
				// a prototype cell and our itemSize or estimatedItemSize are wrong/unset
				// So trigger a constraint update; if we need a measurement, that will make it happen
				// TODO: Fix ItemsViewLayout.ConstrainTo(CollectionView.Bounds.Size);
			}
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
			base.ViewWillLayoutSubviews();
			LayoutEmptyView();
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

		// void InvalidateMeasureIfContentSizeChanged()
		// {
		// 	var contentSize = CollectionView?.CollectionViewLayout?.CollectionViewContentSize;
		//
		// 	if (!contentSize.HasValue)
		// 	{
		// 		return;
		// 	}
		//
		// 	bool widthChanged = _previousContentSize.Width != contentSize.Value.Width;
		// 	bool heightChanged = _previousContentSize.Height != contentSize.Value.Height;
		//
		// 	if (_initialized && (widthChanged || heightChanged))
		// 	{
		// 		var screenFrame = CollectionView?.Window?.Frame;
		//
		// 		if (!screenFrame.HasValue)
		// 		{
		// 			return;
		// 		}
		//
		// 		var screenWidth = screenFrame.Value.Width;
		// 		var screenHeight = screenFrame.Value.Height;
		// 		bool invalidate = false;
		//
		// 		// If both the previous content size and the current content size are larger
		// 		// than the screen size, then we know that we're already maxed out and the 
		// 		// CollectionView items are scrollable. There's no reason to force an invalidation
		// 		// of the CollectionView to expand/contract it.
		//
		// 		// If either size is smaller than that, we need to invalidate to ensure that the 
		// 		// CollectionView is re-measured and set to the correct size.
		//
		// 		if (widthChanged && (contentSize.Value.Width < screenWidth || _previousContentSize.Width < screenWidth))
		// 		{
		// 			invalidate = true;
		// 		}
		//
		// 		if (heightChanged && (contentSize.Value.Height < screenHeight || _previousContentSize.Height < screenHeight))
		// 		{
		// 			invalidate = true;
		// 		}
		//
		// 		if (invalidate)
		// 		{
		// 			(ItemsView as IView)?.InvalidateMeasure();
		// 		}
		// 	}
		// 	_previousContentSize = contentSize.Value;
		// }

		const int HeaderTag = 111;

		// internal Size? GetSize()
		// {
		// 	if (_emptyViewDisplayed)
		// 	{
		// 		return _emptyUIView.Frame.Size.ToSize();
		// 	}

		// 	nfloat headerHeight = 0;
		// 	var headerView = CollectionView.ViewWithTag(HeaderTag);

		// 	if (headerView != null)
		// 		headerHeight = headerView.Frame.Height;

		// 	var sizeColl = CollectionView.CollectionViewLayout.CollectionViewContentSize;
		// 	return sizeColl.ToSize();
		// }

		// void ConstrainItemsToBounds()
		// {
		// 	var contentBounds = CollectionView.AdjustedContentInset.InsetRect(CollectionView.Bounds);
		// 	var constrainedSize = contentBounds.Size;
		// 	ItemsViewLayout.UpdateConstraints(constrainedSize);
		// }

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

			UpdateEmptyView();
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

			if (_emptyViewDisplayed)
			{
				AlignEmptyView();
			}

			Layout.InvalidateLayout();
		}

		public override nint NumberOfSections(UICollectionView collectionView)
		{
			CheckForEmptySource();
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

				var cellType = typeof(TemplatedCell2);

				var orientation = ScrollDirection == UICollectionViewScrollDirection.Horizontal ? "Horizontal" : "Vertical";
				var reuseId = $"{TemplatedCell2.ReuseId}.{orientation}.{dataTemplate.Id}";

				if (!_cellReuseIds.Contains(reuseId))
				{
					Console.WriteLine($"REGISTER CELL ID: {reuseId}");
					CollectionView.RegisterClassForCell(cellType, new NSString(reuseId));
					_cellReuseIds.Add(reuseId);
				}

				return reuseId;
			}

			return ScrollDirection == UICollectionViewScrollDirection.Horizontal ? HorizontalDefaultCell2.ReuseId : VerticalDefaultCell2.ReuseId;
		}

		protected virtual void RegisterViewTypes()
		{
			CollectionView.RegisterClassForCell(typeof(HorizontalDefaultCell2), HorizontalDefaultCell2.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalDefaultCell2), VerticalDefaultCell2.ReuseId);
			CollectionView.RegisterClassForCell(typeof(HorizontalCell2), HorizontalCell2.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalCell2), VerticalCell2.ReuseId);
		}

		protected abstract bool IsHorizontal { get; }

		protected virtual CGRect DetermineEmptyViewFrame()
		{
			return new CGRect(CollectionView.Frame.X, CollectionView.Frame.Y,
				CollectionView.Frame.Width, CollectionView.Frame.Height);
		}


		internal void UpdateView(object view, DataTemplate viewTemplate, ref UIView uiView, ref VisualElement formsElement)
		{
			// Is view set on the ItemsView?
			if (view == null && viewTemplate is null)
			{
				if (formsElement != null)
				{
					//Platform.GetRenderer(formsElement)?.DisposeRendererAndChildren();
				}

				uiView?.Dispose();
				uiView = null;

				formsElement = null;
			}
			else
			{
				// Create the native renderer for the view, and keep the actual Forms element (if any)
				// around for updating the layout later
				(uiView, formsElement) = Items.TemplateHelpers.RealizeView(view, viewTemplate, ItemsView);
			}
		}

		internal void UpdateEmptyView()
		{
			if (!_initialized)
			{
				return;
			}

			// Get rid of the old view
			TearDownEmptyView();

			// Set up the new empty view
			UpdateView(ItemsView?.EmptyView, ItemsView?.EmptyViewTemplate, ref _emptyUIView, ref _emptyViewFormsElement);

			// We may need to show the updated empty view
			UpdateEmptyViewVisibility(ItemsSource?.ItemCount == 0);
		}

		void UpdateEmptyViewVisibility(bool isEmpty)
		{
			if (!_initialized)
			{
				return;
			}

			if (isEmpty)
			{
				ShowEmptyView();
			}
			else
			{
				HideEmptyView();
			}
		}

		void AlignEmptyView()
		{
			if (_emptyUIView == null)
			{
				return;
			}

			bool isRtl;

			if (OperatingSystem.IsIOSVersionAtLeast(10) || OperatingSystem.IsTvOSVersionAtLeast(10))
				isRtl = CollectionView.EffectiveUserInterfaceLayoutDirection == UIUserInterfaceLayoutDirection.RightToLeft;
			else
				isRtl = CollectionView.SemanticContentAttribute == UISemanticContentAttribute.ForceRightToLeft;

			if (isRtl)
			{
				if (_emptyUIView.Transform.A == -1)
				{
					return;
				}

				FlipEmptyView();
			}
			else
			{
				if (_emptyUIView.Transform.A == -1)
				{
					FlipEmptyView();
				}
			}
		}

		void FlipEmptyView()
		{
			// Flip the empty view 180 degrees around the X axis 
			_emptyUIView.Transform = CGAffineTransform.Scale(_emptyUIView.Transform, -1, 1);
		}

		void ShowEmptyView()
		{
			if (_emptyViewDisplayed || _emptyUIView == null)
			{
				return;
			}

			_emptyUIView.Tag = EmptyTag;
			CollectionView.AddSubview(_emptyUIView);

			if (((IElementController)ItemsView).LogicalChildren.IndexOf(_emptyViewFormsElement) == -1)
			{
				ItemsView.AddLogicalChild(_emptyViewFormsElement);
			}

			LayoutEmptyView();

			AlignEmptyView();
			_emptyViewDisplayed = true;
		}

		void HideEmptyView()
		{
			if (!_emptyViewDisplayed || _emptyUIView == null)
			{
				return;
			}

			_emptyUIView.RemoveFromSuperview();

			_emptyViewDisplayed = false;
		}

		void TearDownEmptyView()
		{
			HideEmptyView();

			// RemoveLogicalChild will trigger a disposal of the native view and its content
			ItemsView.RemoveLogicalChild(_emptyViewFormsElement);

			_emptyUIView = null;
			_emptyViewFormsElement = null;
		}

		void LayoutEmptyView()
		{
			if (!_initialized || _emptyUIView == null || _emptyUIView.Superview == null)
			{
				return;
			}

			var frame = DetermineEmptyViewFrame();

			_emptyUIView.Frame = frame;

			if (_emptyViewFormsElement != null && ((IElementController)ItemsView).LogicalChildren.IndexOf(_emptyViewFormsElement) != -1)
				_emptyViewFormsElement.Layout(frame.ToRectangle());
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
			if (cell is TemplatedCell2 TemplatedCell2 &&
				(TemplatedCell2.PlatformHandler?.VirtualView as View)?.BindingContext is object bindingContext)
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
					TemplatedCell2.Unbind();
				}
			}
		}
	}
}
