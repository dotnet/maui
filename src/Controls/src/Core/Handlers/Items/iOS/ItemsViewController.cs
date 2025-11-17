#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CoreGraphics;
using Foundation;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract class ItemsViewController<TItemsView> : UICollectionViewController
	where TItemsView : ItemsView
	{
		public const int EmptyTag = 333;
		readonly WeakReference<TItemsView> _itemsView;

		public IItemsViewSource ItemsSource { get; protected set; }
		public TItemsView ItemsView => _itemsView.GetTargetOrDefault();

		// ItemsViewLayout provides an accessor to the typed UICollectionViewLayout. It's also important to note that the
		// initial UICollectionViewLayout which is passed in to the ItemsViewController (and accessed via the Layout property)
		// _does not_ get updated when the layout is updated for the CollectionView. That property only refers to the
		// original layout. So it's unlikely that you would ever want to use .Layout; use .ItemsViewLayout instead.
		// See https://developer.apple.com/documentation/uikit/uicollectionviewcontroller/1623980-collectionviewlayout
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		protected ItemsViewLayout ItemsViewLayout { get; set; }

		bool _initialized;
		bool _laidOut;
		bool _isEmpty = true;
		bool _emptyViewDisplayed;
		bool _disposed;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		Func<UICollectionViewCell> _getPrototype;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		Func<NSIndexPath, UICollectionViewCell> _getPrototypeForIndexPath;
		CGSize _previousContentSize = CGSize.Empty;

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		UIView _emptyUIView;
		VisualElement _emptyViewFormsElement;
		Dictionary<object, TemplatedCell> _measurementCells = new Dictionary<object, TemplatedCell>();
		List<string> _cellReuseIds = new List<string>();

		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		protected UICollectionViewDelegateFlowLayout Delegator { get; set; }

		protected ItemsViewController(TItemsView itemsView, ItemsViewLayout layout) : base(layout)
		{
			_itemsView = new(itemsView);
			ItemsViewLayout = layout;
		}

		public void UpdateLayout(ItemsViewLayout newLayout)
		{
			// Ignore calls to this method if the new layout is the same as the old one
			if (CollectionView.CollectionViewLayout == newLayout)
				return;

			ItemsViewLayout = newLayout;
			_initialized = false;

			EnsureLayoutInitialized();

			if (_initialized)
			{
				// Reload the data so the currently visible cells get arranged according to the new layout
				CollectionView.ReloadData();
			}
		}

		internal virtual void Disconnect()
		{
			DisposeItemsSource();
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			_disposed = true;

			if (disposing)
			{
				ItemsSource?.Dispose();

				((IUIViewLifeCycleEvents)CollectionView).MovedToWindow -= MovedToWindow;

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

			switch (cell)
			{
				case DefaultCell defaultCell:
					UpdateDefaultCell(defaultCell, indexPath);
					break;
				case TemplatedCell templatedCell:
					UpdateTemplatedCell(templatedCell, indexPath);
					break;
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

			if (_isEmpty)
			{
				_measurementCells?.Clear();
				ItemsViewLayout?.ClearCellSizeCache();
			}

			if (wasEmpty != _isEmpty)
			{
				UpdateEmptyViewVisibility(_isEmpty);
			}

			if (wasEmpty && !_isEmpty)
			{
				// If we're going from empty to having stuff, it's possible that we've never actually measured
				// a prototype cell and our itemSize or estimatedItemSize are wrong/unset
				// So trigger a constraint update; if we need a measurement, that will make it happen
				ItemsViewLayout.ConstrainTo(CollectionView.Bounds.Size);
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
			var collectionView = new MauiCollectionView(CGRect.Empty, ItemsViewLayout);
			((IUIViewLifeCycleEvents)collectionView).MovedToWindow += MovedToWindow;
			CollectionView = collectionView;
		}

		private void MovedToWindow(object sender, EventArgs e)
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

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);
		}

		public override void ViewWillLayoutSubviews()
		{
			ConstrainItemsToBounds();

			var mauiCollectionView = CollectionView as MauiCollectionView;
			var needsCellLayout = mauiCollectionView?.NeedsCellLayout is true;
			if (needsCellLayout)
			{
				InvalidateLayoutIfItemsMeasureChanged();
				mauiCollectionView.NeedsCellLayout = false;
			}

			base.ViewWillLayoutSubviews();

			if (needsCellLayout || // A cell changed its measure
				!_laidOut || // We have never laid out
							 // With no cells, nothing will trigger a layout when bounds change,
							 // but we still need to properly lay out supplementary views
				ItemsSource.ItemCount == 0)
			{
				// We don't want to mess up with ContentOffset while refreshing, given that's also gonna cause
				// a change in the content's offset Y.
				if (!IsRefreshing())
				{
					MeasureSupplementaryViews();
					LayoutSupplementaryViews();
				}
			}

			InvalidateMeasureIfContentSizeChanged();

			_laidOut = true;
		}

		private protected virtual void MeasureSupplementaryViews()
		{
			RemeasureLayout(_emptyViewFormsElement, _emptyUIView);
		}

		private protected virtual void LayoutSupplementaryViews()
		{
			LayoutEmptyView();
		}

		void InvalidateLayoutIfItemsMeasureChanged()
		{
			var visibleCells = CollectionView.VisibleCells;
			List<TemplatedCell> invalidatedCells = null;

			var visibleCellsLength = visibleCells.Length;
			for (int n = 0; n < visibleCellsLength; n++)
			{
				if (visibleCells[n] is TemplatedCell { MeasureInvalidated: true } cell)
				{
					invalidatedCells ??= [];
					invalidatedCells.Add(cell);
				}
			}

			if (invalidatedCells is not null)
			{
				// GridLayout has a special positioning override when there's only one item
				// so we have to invalidate the layout entirely to trigger that special case.
				if (ItemsSource.ItemCount == 1)
				{
					CollectionView.CollectionViewLayout.InvalidateLayout();
				}
				else
				{
					var layoutInvalidationContext = new UICollectionViewFlowLayoutInvalidationContext();
					layoutInvalidationContext.InvalidateItems(invalidatedCells.Select(CollectionView.IndexPathForCell).ToArray());
					CollectionView.CollectionViewLayout.InvalidateLayout(layoutInvalidationContext);
				}
			}
		}

		bool IsRefreshing()
		{
			var subviews = CollectionView.Subviews;
			var subviewsLength = subviews.Length;
			for (int i = 0; i < subviewsLength; i++)
			{
				if (subviews[i] is UIRefreshControl { Refreshing: true })
				{
					return true;
				}
			}

			return false;
		}

		void InvalidateMeasureIfContentSizeChanged()
		{
			var contentSize = CollectionView?.CollectionViewLayout?.CollectionViewContentSize;

			if (!contentSize.HasValue)
			{
				return;
			}

			bool widthChanged = _previousContentSize.Width != contentSize.Value.Width;
			bool heightChanged = _previousContentSize.Height != contentSize.Value.Height;

			if (_initialized && (widthChanged || heightChanged))
			{
				var screenFrame = CollectionView?.Window?.Frame;

				if (!screenFrame.HasValue)
				{
					return;
				}

				var screenWidth = screenFrame.Value.Width;
				var screenHeight = screenFrame.Value.Height;
				bool invalidate = false;

				// If both the previous content size and the current content size are larger
				// than the screen size, then we know that we're already maxed out and the 
				// CollectionView items are scrollable. There's no reason to force an invalidation
				// of the CollectionView to expand/contract it.

				// If either size is smaller than that, we need to invalidate to ensure that the 
				// CollectionView is re-measured and set to the correct size.

				if (widthChanged && (contentSize.Value.Width < screenWidth || _previousContentSize.Width < screenWidth))
				{
					invalidate = true;
				}
				else if (heightChanged && (contentSize.Value.Height < screenHeight || _previousContentSize.Height < screenHeight))
				{
					invalidate = true;
				}

				if (invalidate)
				{
					ItemsView.InvalidateMeasureInternal(InvalidationTrigger.MeasureChanged);
				}
			}

			_previousContentSize = contentSize.Value;
		}


		internal virtual Size? GetSize()
		{
			if (_emptyViewDisplayed)
			{
				return _emptyUIView.Frame.Size.ToSize();
			}

			return CollectionView.CollectionViewLayout.CollectionViewContentSize.ToSize();
		}

		void ConstrainItemsToBounds()
		{
			var contentBounds = CollectionView.AdjustedContentInset.InsetRect(CollectionView.Bounds);
			var constrainedSize = contentBounds.Size;
			ItemsViewLayout.UpdateConstraints(constrainedSize, !_laidOut);
		}

		void EnsureLayoutInitialized()
		{
			if (_initialized)
			{
				return;
			}

			_initialized = true;

			_getPrototype ??= GetPrototype;
			ItemsViewLayout.GetPrototype = _getPrototype;

			_getPrototypeForIndexPath ??= GetPrototypeForIndexPath;
			ItemsViewLayout.GetPrototypeForIndexPath = _getPrototypeForIndexPath;

			Delegator = CreateDelegator();
			CollectionView.Delegate = Delegator;

			ItemsViewLayout.SetInitialConstraints(CollectionView.Bounds.Size);
			CollectionView.SetCollectionViewLayout(ItemsViewLayout, false);

			UpdateEmptyView();
		}

		protected virtual UICollectionViewDelegateFlowLayout CreateDelegator()
		{
			return new ItemsViewDelegator<TItemsView, ItemsViewController<TItemsView>>(ItemsViewLayout, this);
		}

		protected virtual IItemsViewSource CreateItemsViewSource()
		{
			return ItemsSourceFactory.Create(ItemsView.ItemsSource, this);
		}

		public virtual void UpdateItemsSource()
		{
			_measurementCells?.Clear();
			ItemsViewLayout?.ClearCellSizeCache();
			ItemsSource?.Dispose();
			ItemsSource = CreateItemsViewSource();

			CollectionView.ReloadData();
			CollectionView.CollectionViewLayout.InvalidateLayout();

			(ItemsView as IView)?.InvalidateMeasure();
		}

		internal void DisposeItemsSource()
		{
			_measurementCells?.Clear();
			ItemsViewLayout?.ClearCellSizeCache();
			ItemsSource?.Dispose();
			ItemsSource = new EmptySource();
			CollectionView.ReloadData();
		}

		public virtual void UpdateFlowDirection()
		{
			if (ItemsView.Handler.PlatformView is UIView itemsView)
			{
				itemsView.UpdateFlowDirection(ItemsView);
				if (ItemsView.ItemTemplate is not null)
				{
					foreach (var child in ItemsView.LogicalChildrenInternal)
					{
						if (child is VisualElement ve && ve.Handler?.PlatformView is UIView view)
						{
							view.UpdateFlowDirection(ve);
						}
					}
				}
				else
				{
					// If we don't have an ItemTemplate, then we need to update the default cell's flow direction
					if (CollectionView?.VisibleCells is UICollectionViewCell[] visibleCells)
					{
						foreach (var cell in visibleCells.OfType<DefaultCell>())
						{
							cell.Label.UpdateFlowDirection(ItemsView);
						}
					}
				}
			}

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

		protected virtual void UpdateDefaultCell(DefaultCell cell, NSIndexPath indexPath)
		{
			cell.Label.Text = ItemsSource[indexPath].ToString();

			if (cell is ItemsViewCell constrainedCell)
			{
				ItemsViewLayout.PrepareCellForLayout(constrainedCell);
			}
		}

		protected virtual void UpdateTemplatedCell(TemplatedCell cell, NSIndexPath indexPath)
		{
			cell.LayoutAttributesChanged -= CellLayoutAttributesChanged;

			var bindingContext = ItemsSource[indexPath];

			// If we've already created a cell for this index path (for measurement), re-use the content
			if (_measurementCells != null && _measurementCells.TryGetValue(bindingContext, out TemplatedCell measurementCell))
			{
				_measurementCells.Remove(bindingContext);
				measurementCell.LayoutAttributesChanged -= CellLayoutAttributesChanged;
				cell.UseContent(measurementCell);
			}
			else
			{
				cell.Bind(ItemsView.ItemTemplate, ItemsSource[indexPath], ItemsView);
			}

			cell.LayoutAttributesChanged += CellLayoutAttributesChanged;

			ItemsViewLayout.PrepareCellForLayout(cell);
		}

		public virtual NSIndexPath GetIndexForItem(object item)
		{
			return ItemsSource.GetIndexForItem(item);
		}

		protected object GetItemAtIndex(NSIndexPath index)
		{
			return ItemsSource[index];
		}

		[UnconditionalSuppressMessage("Memory", "MEM0003", Justification = "Proven safe in test: CollectionViewTests.ItemsSourceDoesNotLeak")]
		void CellLayoutAttributesChanged(object sender, LayoutAttributesChangedEventArgs args)
		{
			CacheCellAttributes(args.NewAttributes.IndexPath, args.NewAttributes.Size);
		}

		protected virtual void CacheCellAttributes(NSIndexPath indexPath, CGSize size)
		{
			if (!ItemsSource.IsIndexPathValid(indexPath))
			{
				// The upate might be coming from a cell that's being removed; don't cache it.
				return;
			}

			var item = ItemsSource[indexPath];
			if (item != null)
			{
				ItemsViewLayout.CacheCellSize(item, size);
			}
		}

		protected virtual string DetermineCellReuseId(NSIndexPath indexPath)
		{
			if (ItemsView.ItemTemplate != null)
			{
				var item = ItemsSource[indexPath];

				var dataTemplate = ItemsView.ItemTemplate.SelectDataTemplate(item, ItemsView);

				var cellOrientation = ItemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Vertical ? "v" : "h";
				(Type cellType, var cellTypeReuseId) = DetermineTemplatedCellType();

				var reuseId = $"_{cellTypeReuseId}_{cellOrientation}_{dataTemplate.Id}";

				if (!_cellReuseIds.Contains(reuseId))
				{
					CollectionView.RegisterClassForCell(cellType, new NSString(reuseId));
					_cellReuseIds.Add(reuseId);
				}

				return reuseId;
			}

			return ItemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Horizontal
				? HorizontalDefaultCell.ReuseId
				: VerticalDefaultCell.ReuseId;
		}

		private protected virtual (Type CellType, string CellTypeReuseId) DetermineTemplatedCellType()
		{
			return (ItemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Vertical ? typeof(VerticalCell) : typeof(HorizontalCell), "maui");
		}

		[Obsolete("Use DetermineCellReuseId(NSIndexPath indexPath) instead.")]
		protected virtual string DetermineCellReuseId()
		{
			if (ItemsView.ItemTemplate != null)
			{
				return ItemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Horizontal
					? HorizontalCell.ReuseId
					: VerticalCell.ReuseId;
			}

			return ItemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Horizontal
				? HorizontalDefaultCell.ReuseId
				: VerticalDefaultCell.ReuseId;
		}

		UICollectionViewCell GetPrototype()
		{
			if (ItemsSource == null || ItemsSource.ItemCount == 0)
			{
				return null;
			}

			var group = 0;

			if (ItemsSource.GroupCount > 1)
			{
				// If we're in a grouping situation, then we need to make sure we find an actual data item
				// to use for our prototype cell. It's possible that we have empty groups.
				for (int n = 0; n < ItemsSource.GroupCount; n++)
				{
					if (ItemsSource.ItemCountInGroup(n) > 0)
					{
						group = n;
						break;
					}
				}
			}

			var indexPath = NSIndexPath.Create(group, 0);

			return GetPrototypeForIndexPath(indexPath);
		}

		internal UICollectionViewCell GetPrototypeForIndexPath(NSIndexPath indexPath)
		{
			return CreateMeasurementCell(indexPath);
		}

		protected virtual void RegisterViewTypes()
		{
			CollectionView.RegisterClassForCell(typeof(HorizontalDefaultCell), HorizontalDefaultCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalDefaultCell), VerticalDefaultCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(HorizontalCell), HorizontalCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalCell), VerticalCell.ReuseId);
		}

		protected abstract bool IsHorizontal { get; }

		protected virtual CGRect DetermineEmptyViewFrame()
		{
			return new CGRect(CollectionView.Frame.X, CollectionView.Frame.Y,
				CollectionView.Frame.Width, CollectionView.Frame.Height);
		}

		protected void RemeasureLayout(VisualElement formsElement)
		{
			Size size;
			if (IsHorizontal)
			{
				var request = formsElement.Measure(double.PositiveInfinity, CollectionView.Frame.Height);
				size = new Size(request.Width, CollectionView.Frame.Height);
			}
			else
			{
				var request = formsElement.Measure(CollectionView.Frame.Width, double.PositiveInfinity);
				size = new Size(CollectionView.Frame.Width, request.Height);
			}

			var platformView = formsElement.ToPlatform();
			if (platformView.Superview is GeneralWrapperView generalWrapperView)
			{
				var originalFrame = generalWrapperView.Frame;
				generalWrapperView.Frame = new CGRect(originalFrame.X, originalFrame.Y, (nfloat)size.Width, (nfloat)size.Height);
			}
			else
			{
				var frame = new Rect(platformView.Frame.X, platformView.Frame.Y, size.Width, size.Height);
				formsElement.Arrange(frame);
			}
		}

		void RemeasureLayout(UIView nativeView)
		{
			var originalFrame = nativeView.Frame;

			if (IsHorizontal)
			{
				var constraints = new CGSize(double.PositiveInfinity, CollectionView.Frame.Height);
				var size = nativeView.SizeThatFits(constraints);
				nativeView.Frame = new CGRect(originalFrame.X, originalFrame.Y, size.Width, CollectionView.Frame.Height);
			}
			else
			{
				var constraints = new CGSize(CollectionView.Frame.Width, double.PositiveInfinity);
				var size = nativeView.SizeThatFits(constraints);
				nativeView.Frame = new CGRect(originalFrame.X, originalFrame.Y, CollectionView.Frame.Width, size.Height);
			}
		}

		private protected void RemeasureLayout(VisualElement formsElement, UIView nativeElement)
		{
			if (formsElement is not null)
			{
				RemeasureLayout(formsElement);
			}
			else if (nativeElement is not null)
			{
				RemeasureLayout(nativeElement);
			}
		}

		protected void OnFormsElementMeasureInvalidated(object sender, EventArgs e)
		{
			if (sender is VisualElement formsElement)
			{
				HandleFormsElementMeasureInvalidated(formsElement);
			}
		}

		protected virtual void HandleFormsElementMeasureInvalidated(VisualElement formsElement)
		{
			RemeasureLayout(formsElement);
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

				uiView?.Dispose();
				uiView = null;
				formsElement?.Handler?.DisconnectHandler();
				formsElement = null;
			}
			else
			{
				// Create the native renderer for the view, and keep the actual Forms element (if any)
				// around for updating the layout later
				(uiView, formsElement) = TemplateHelpers.RealizeView(view, viewTemplate, ItemsView);
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

			if (_emptyViewFormsElement is not null)
			{
				// Update flow direction for EmptyView 
				if (_emptyViewFormsElement.Handler?.PlatformView is UIView emptyView)
				{
					emptyView.UpdateFlowDirection(_emptyViewFormsElement);
				}
			}
			else if (_emptyUIView is UILabel label)
			{
				// For UILabel, always set to center for better UX
				label.TextAlignment = UITextAlignment.Center;
			}
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

			_emptyUIView.InvalidateMeasure();

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

			_emptyViewFormsElement?.Measure(frame.Width, frame.Height);
			_emptyViewFormsElement?.Arrange(frame.ToRectangle());
			_emptyUIView.Frame = frame;
		}

		TemplatedCell CreateAppropriateCellForLayout()
		{
			var frame = new CGRect(0, 0, ItemsViewLayout.EstimatedItemSize.Width, ItemsViewLayout.EstimatedItemSize.Height);

			if (ItemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Horizontal)
			{
				return new HorizontalCell(frame);
			}

			return new VerticalCell(frame);
		}

		public UICollectionViewCell CreateMeasurementCell(NSIndexPath indexPath)
		{
			if (ItemsView.ItemTemplate == null)
			{
				var frame = new CGRect(0, 0, ItemsViewLayout.EstimatedItemSize.Width, ItemsViewLayout.EstimatedItemSize.Height);

				DefaultCell cell;
				if (ItemsViewLayout.ScrollDirection == UICollectionViewScrollDirection.Horizontal)
				{
					cell = new HorizontalDefaultCell(frame);
				}
				else
				{
					cell = new VerticalDefaultCell(frame);
				}

				UpdateDefaultCell(cell, indexPath);
				return cell;
			}

			TemplatedCell templatedCell = CreateAppropriateCellForLayout();

			UpdateTemplatedCell(templatedCell, indexPath);

			// Keep this cell around, we can transfer the contents to the actual cell when the UICollectionView creates it
			if (_measurementCells != null)
				_measurementCells[ItemsSource[indexPath]] = templatedCell;

			return templatedCell;
		}

		internal CGSize GetSizeForItem(NSIndexPath indexPath)
		{
			if (ItemsViewLayout.EstimatedItemSize.IsEmpty)
			{
				return ItemsViewLayout.ItemSize;
			}

			if (ItemsSource.IsIndexPathValid(indexPath))
			{
				var item = ItemsSource[indexPath];

				if (item != null && ItemsViewLayout.TryGetCachedCellSize(item, out CGSize size))
				{
					return size;
				}
			}

			return ItemsViewLayout.EstimatedItemSize;
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
			if (cell is TemplatedCell templatedCell &&
				(templatedCell.PlatformHandler?.VirtualView as View)?.BindingContext is object bindingContext)
			{
				// We want to unbind a cell that is no longer present in the items source. Unfortunately
				// it's too expensive to check directly, so let's check that the current binding context
				// matches the item at a given position.

				indexPath = GetAdjustedIndexPathForItemSource(indexPath);

				var itemsSource = ItemsSource;
				if (itemsSource is null ||
					!itemsSource.IsIndexPathValid(indexPath) ||
					!Equals(itemsSource[indexPath], bindingContext))
				{
					templatedCell.Unbind();

					if (CollectionView is MauiCollectionView collectionView)
					{
						// When removing a cell, we need to trigger a layout update in order to sync the footer position
						collectionView.NeedsCellLayout = true;
					}
				}
			}
		}
	}
}
