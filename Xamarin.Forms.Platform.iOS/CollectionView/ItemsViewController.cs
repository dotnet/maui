using System;
using System.Drawing;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	// TODO hartez 2018/06/01 14:21:24 Add a method for updating the layout	
	public abstract class ItemsViewController : UICollectionViewController
	{
		public IItemsViewSource ItemsSource { get; protected set; }
		public ItemsView ItemsView { get; }
		protected ItemsViewLayout ItemsViewLayout { get; set; }
		bool _initialConstraintsSet;
		bool _isEmpty;
		bool _currentBackgroundIsEmptyView;
		bool _disposed;

		UIView _backgroundUIView;
		UIView _emptyUIView;
		VisualElement _emptyViewFormsElement;

		protected UICollectionViewDelegator Delegator { get; set; }

		public ItemsViewController(ItemsView itemsView, ItemsViewLayout layout) : base(layout)
		{
			ItemsView = itemsView;
			ItemsSource = CreateItemsViewSource();

			UpdateLayout(layout);
		}

		public void UpdateLayout(ItemsViewLayout layout)
		{
			ItemsViewLayout = layout;
			ItemsViewLayout.GetPrototype = GetPrototype;

			// If we're updating from a previous layout, we should keep any settings for the SelectableItemsViewController around
			var selectableItemsViewController = Delegator?.SelectableItemsViewController;
			Delegator = CreateDelegator();

			CollectionView.Delegate = Delegator;

			if (CollectionView.CollectionViewLayout != ItemsViewLayout)
			{
				// We're updating from a previous layout

				// Make sure the new layout is sized properly
				ItemsViewLayout.ConstrainTo(CollectionView.Bounds.Size);

				CollectionView.SetCollectionViewLayout(ItemsViewLayout, false);

				// Reload the data so the currently visible cells get laid out according to the new layout
				CollectionView.ReloadData();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				ItemsSource?.Dispose();
				_emptyUIView?.Dispose();
				_emptyUIView = null;
				_emptyViewFormsElement = null;
			}

			_disposed = true;

			base.Dispose(disposing);
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.DequeueReusableCell(DetermineCellReuseId(), indexPath) as UICollectionViewCell;

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
			var count = ItemsSource.ItemCountInGroup(section);

			CheckForEmptySource();

			return count;
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
				ItemsViewLayout.ConstrainTo(CollectionView.Bounds.Size);
			}
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			AutomaticallyAdjustsScrollViewInsets = false;
			RegisterViewTypes();
		}

		public override void ViewWillLayoutSubviews()
		{
			base.ViewWillLayoutSubviews();

			// We can't set this constraint up on ViewDidLoad, because Forms does other stuff that resizes the view
			// and we end up with massive layout errors. And View[Will/Did]Appear do not fire for this controller
			// reliably. So until one of those options is cleared up, we set this flag so that the initial constraints
			// are set up the first time this method is called.
			if (!_initialConstraintsSet)
			{
				ItemsViewLayout.ConstrainTo(CollectionView.Bounds.Size);
				_initialConstraintsSet = true;
			}
		}

		protected virtual UICollectionViewDelegator CreateDelegator()
		{
			return new UICollectionViewDelegator(ItemsViewLayout, this);
		}

		protected virtual IItemsViewSource CreateItemsViewSource()
		{
			return ItemsSourceFactory.Create(ItemsView.ItemsSource, CollectionView);
		}

		public virtual void UpdateItemsSource()
		{
			ItemsSource = CreateItemsViewSource();
			CollectionView.ReloadData();
			CollectionView.CollectionViewLayout.InvalidateLayout();
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
			cell.ContentSizeChanged -= CellContentSizeChanged;

			cell.Bind(ItemsView, ItemsSource[indexPath]);

			cell.ContentSizeChanged += CellContentSizeChanged;

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

		void CellContentSizeChanged(object sender, EventArgs e)
		{
			if (_disposed)
				return;

			Layout?.InvalidateLayout();
		}

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
			if (ItemsSource.ItemCount == 0)
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

			return GetCell(CollectionView, indexPath);
		}

		protected virtual void RegisterViewTypes()
		{
			CollectionView.RegisterClassForCell(typeof(HorizontalDefaultCell), HorizontalDefaultCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalDefaultCell), VerticalDefaultCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(HorizontalCell),
				HorizontalCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalCell), VerticalCell.ReuseId);
		}

		protected abstract bool IsHorizontal { get; }

		internal void UpdateEmptyView()
		{
			UpdateView(ItemsView?.EmptyView, ItemsView?.EmptyViewTemplate, ref _emptyUIView, ref _emptyViewFormsElement);

			// If the empty view is being displayed, we might need to update it
			UpdateEmptyViewVisibility(ItemsSource?.ItemCount == 0);
		}

		protected void UpdateSubview(object view, DataTemplate viewTemplate, ref UIView uiView, ref VisualElement formsElement)
		{
			uiView?.RemoveFromSuperview();

			if (formsElement != null)
			{
				ItemsView.RemoveLogicalChild(formsElement);
				formsElement.MeasureInvalidated -= OnFormsElementMeasureInvalidated;
			}

			UpdateView(view, viewTemplate, ref uiView, ref formsElement);

			if (uiView != null)
			{
				CollectionView.AddSubview(uiView);
			}

			if (formsElement != null)
				ItemsView.AddLogicalChild(formsElement);

			if (formsElement != null)
			{
				RemeasureLayout(formsElement);
				formsElement.MeasureInvalidated += OnFormsElementMeasureInvalidated;
			}
			else if (uiView != null)
			{
				uiView.SizeToFit();
			}
		}

		void RemeasureLayout(VisualElement formsElement)
		{
			if (IsHorizontal)
			{
				var request = formsElement.Measure(double.PositiveInfinity, CollectionView.Frame.Height, MeasureFlags.IncludeMargins);
				Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(formsElement, new Rectangle(0, 0, request.Request.Width, CollectionView.Frame.Height));
			}
			else
			{
				var request = formsElement.Measure(CollectionView.Frame.Width, double.PositiveInfinity, MeasureFlags.IncludeMargins);
				Xamarin.Forms.Layout.LayoutChildIntoBoundingRegion(formsElement, new Rectangle(0, 0, CollectionView.Frame.Width, request.Request.Height));
			}
		}

		void OnFormsElementMeasureInvalidated(object sender, EventArgs e)
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
			if (view == null)
			{
				if (formsElement != null)
					Platform.GetRenderer(formsElement)?.DisposeRendererAndChildren();

				uiView = null;
				formsElement = null;
			}
			else
			{
				// Create the native renderer for the view, and keep the actual Forms element (if any)
				// around for updating the layout later
				var (NativeView, FormsElement) = TemplateHelpers.RealizeView(view, viewTemplate, ItemsView);
				uiView = NativeView;
				formsElement = FormsElement;
			}
		}

		void UpdateEmptyViewVisibility(bool isEmpty)
		{
			if (isEmpty && _emptyUIView != null)
			{
				if (!_currentBackgroundIsEmptyView)
				{
					// Cache any existing background view so we can restore it later
					_backgroundUIView = CollectionView.BackgroundView;
				}

				// Replace any current background with the EmptyView. This will also set the native empty view's frame
				// to match the UICollectionView's frame
				CollectionView.BackgroundView = _emptyUIView;
				_currentBackgroundIsEmptyView = true;

				if (_emptyViewFormsElement != null)
				{
					if (ItemsView.EmptyViewTemplate == null)
					{
						ItemsView.AddLogicalChild(_emptyViewFormsElement);
					}

					// Now that the native empty view's frame is sized to the UICollectionView, we need to handle
					// the Forms layout for its content
					_emptyViewFormsElement.Layout(_emptyUIView.Frame.ToRectangle());
				}
			}
			else
			{
				// Is the empty view currently in the background? Swap back to the default.
				if (_currentBackgroundIsEmptyView)
				{
					CollectionView.BackgroundView = _backgroundUIView;
					ItemsView.RemoveLogicalChild(_emptyViewFormsElement);
				}

				_currentBackgroundIsEmptyView = false;
			}
		}
	}
}
