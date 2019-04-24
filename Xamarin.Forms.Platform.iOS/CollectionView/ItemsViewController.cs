using System;
using System.Collections.Generic;
using Foundation;
using UIKit;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Platform.iOS
{
	// TODO hartez 2018/06/01 14:21:24 Add a method for updating the layout	
	public class ItemsViewController : UICollectionViewController
	{
		IItemsViewSource _itemsSource;
		readonly ItemsView _itemsView;
		ItemsViewLayout _layout;
		bool _initialConstraintsSet;
		bool _safeForReload;
		bool _wasEmpty;
		bool _currentBackgroundIsEmptyView;
		bool _disposed;

		UIView _backgroundUIView;
		UIView _emptyUIView;
		VisualElement _emptyViewFormsElement;

		protected UICollectionViewDelegator Delegator { get; set; }

		public ItemsViewController(ItemsView itemsView, ItemsViewLayout layout) : base(layout)
		{
			_itemsView = itemsView;
			_itemsSource = ItemsSourceFactory.Create(_itemsView.ItemsSource, CollectionView);
			
			// If we already have data, the UICollectionView will have items and we'll be safe to call
			// ReloadData if the ItemsSource changes in the future (see UpdateItemsSource for more).
			_safeForReload = _itemsSource?.Count > 0;

			UpdateLayout(layout);
		}

		public void UpdateLayout(ItemsViewLayout layout)
		{
			_layout = layout;
			_layout.GetPrototype = GetPrototype;

			// If we're updating from a previous layout, we should keep any settings for the SelectableItemsViewController around
			var selectableItemsViewController = Delegator?.SelectableItemsViewController;
			Delegator = new UICollectionViewDelegator(_layout, this);

			CollectionView.Delegate = Delegator;

			if (CollectionView.CollectionViewLayout != _layout)
			{
				// We're updating from a previous layout

				// Make sure the new layout is sized properly
				_layout.ConstrainTo(CollectionView.Bounds.Size);
				
				CollectionView.SetCollectionViewLayout(_layout, false);
				
				// Reload the data so the currently visible cells get laid out according to the new layout
				CollectionView.ReloadData();
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					_itemsSource?.Dispose();
				}

				_disposed = true;

				base.Dispose(disposing);
			}
		}

		public override UICollectionViewCell GetCell(UICollectionView collectionView, NSIndexPath indexPath)
		{
			var cell = collectionView.DequeueReusableCell(DetermineCellReusedId(), indexPath) as UICollectionViewCell;

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
			var count = _itemsSource.Count;

			if (_wasEmpty && count > 0)
			{
				// We've moved from no items to having at least one item; it's likely that the layout needs to update
				// its cell size/estimate
				_layout?.SetNeedCellSizeUpdate();
			}

			_wasEmpty = count == 0;

			UpdateEmptyViewVisibility(_wasEmpty);

			return count;
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			AutomaticallyAdjustsScrollViewInsets = false;
			RegisterCells();
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
				_layout.ConstrainTo(CollectionView.Bounds.Size);
				_initialConstraintsSet = true;
			}
		}

		public virtual void UpdateItemsSource()
		{
			if (_safeForReload)
			{
				UpdateItemsSourceAndReload();
			}
			else
			{
				// Okay, thus far this UICollectionView has never had any items in it. At this point, if
				// we set the ItemsSource and try to call ReloadData(), it'll crash. AFAICT this is a bug, but
				// until it's fixed (or we can figure out another way to go from empty -> having items), we'll
				// have to use this crazy workaround
				EmptyCollectionViewReloadWorkaround();
			}
		}

		void UpdateItemsSourceAndReload()
		{
			_itemsSource = ItemsSourceFactory.Create(_itemsView.ItemsSource, CollectionView);
			CollectionView.ReloadData();
			CollectionView.CollectionViewLayout.InvalidateLayout();
		}

		void EmptyCollectionViewReloadWorkaround()
		{
			var enumerator = _itemsView.ItemsSource.GetEnumerator();

			if (!enumerator.MoveNext())
			{
				// The source we're updating to is empty, so we can just update as normal; it won't crash
				UpdateItemsSourceAndReload();
			}
			else
			{
				// Grab the first item from the new ItemsSource and create a usable source for the UICollectionView
				// from that
				var firstItem = new List<object> { enumerator.Current };
				_itemsSource = ItemsSourceFactory.Create(firstItem, CollectionView);

				// Insert that item into the UICollectionView
				// TODO ezhart When we implement grouping, this will need to be the index of the first actual item
				// Which might not be zero,zero if we have empty groups
				var indexesToInsert = new NSIndexPath[1] { NSIndexPath.Create(0, 0) };

				UIView.PerformWithoutAnimation(() =>
				{
					CollectionView.InsertItems(indexesToInsert);
				});

				// Okay, from now on we can just call ReloadData and things will work fine
				_safeForReload = true;
				UpdateItemsSource();
			}
		}

		protected virtual void UpdateDefaultCell(DefaultCell cell, NSIndexPath indexPath)
		{
			cell.Label.Text = _itemsSource[indexPath.Row].ToString();

			if (cell is ItemsViewCell constrainedCell)
			{
				_layout.PrepareCellForLayout(constrainedCell);
			}
		}

		protected virtual void UpdateTemplatedCell(TemplatedCell cell, NSIndexPath indexPath)
		{
			ApplyTemplateAndDataContext(cell, indexPath);

			if (cell is ItemsViewCell constrainedCell)
			{
				_layout.PrepareCellForLayout(constrainedCell);
			}
		}

		public virtual NSIndexPath GetIndexForItem(object item)
		{
			for (int n = 0; n < _itemsSource.Count; n++)
			{
				if (_itemsSource[n] == item)
				{
					return NSIndexPath.Create(0, n);
				}
			}

			return NSIndexPath.Create(-1, -1);
		}

		protected object GetItemAtIndex(NSIndexPath index)
		{
			return _itemsSource[index.Row];
		}

		void ApplyTemplateAndDataContext(TemplatedCell cell, NSIndexPath indexPath)
		{
			var template = _itemsView.ItemTemplate;
			var item = _itemsSource[indexPath.Row];

			// Run this through the extension method in case it's really a DataTemplateSelector
			template = template.SelectDataTemplate(item, _itemsView);

			// Create the content and renderer for the view and 
			var view = template.CreateContent() as View;
			var renderer = CreateRenderer(view);
			cell.SetRenderer(renderer);

			// Bind the view to the data item
			view.BindingContext = _itemsSource[indexPath.Row];

			// And make sure it's a "child" of the ItemsView
			_itemsView.AddLogicalChild(view);

			cell.ContentSizeChanged += CellContentSizeChanged;
		}

		void CellContentSizeChanged(object sender, EventArgs e)
		{
			Layout?.InvalidateLayout();
		}

		internal void PrepareCellForRemoval(UICollectionViewCell cell)
		{
			if (cell is TemplatedCell templatedCell)
			{
				templatedCell.ContentSizeChanged -= CellContentSizeChanged;

				var oldView = templatedCell.VisualElementRenderer?.Element;
				if (oldView != null)
				{
					oldView.BindingContext = null;
					_itemsView.RemoveLogicalChild(oldView);
				}

				templatedCell.PrepareForRemoval();
			}
		}

		IVisualElementRenderer CreateRenderer(View view)
		{
			if (view == null)
			{
				throw new ArgumentNullException(nameof(view));
			}

			var renderer = Platform.CreateRenderer(view);
			Platform.SetRenderer(view, renderer);

			return renderer;
		}

		string DetermineCellReusedId()
		{
			if (_itemsView.ItemTemplate != null)
			{
				return _layout.ScrollDirection == UICollectionViewScrollDirection.Horizontal
					? HorizontalTemplatedCell.ReuseId
					: VerticalTemplatedCell.ReuseId;
			}

			return _layout.ScrollDirection == UICollectionViewScrollDirection.Horizontal
				? HorizontalDefaultCell.ReuseId
				: VerticalDefaultCell.ReuseId;
		}

		UICollectionViewCell GetPrototype()
		{
			if (_itemsSource.Count == 0)
			{
				return null;
			}

			// TODO hartez assuming this works, we'll need to evaluate using this nsindexpath (what about groups?)
			var indexPath = NSIndexPath.Create(0, 0);
			return GetCell(CollectionView, indexPath);
		}

		void RegisterCells()
		{
			CollectionView.RegisterClassForCell(typeof(HorizontalDefaultCell), HorizontalDefaultCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalDefaultCell), VerticalDefaultCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(HorizontalTemplatedCell),
				HorizontalTemplatedCell.ReuseId);
			CollectionView.RegisterClassForCell(typeof(VerticalTemplatedCell), VerticalTemplatedCell.ReuseId);
		}

		internal void UpdateEmptyView()
		{
			// Is EmptyView set on the ItemsView?
			var emptyView = _itemsView?.EmptyView;

			if (emptyView == null)
			{
				// Clear the cached Forms and native views
				_emptyUIView = null;
				_emptyViewFormsElement = null;
			}
			else
			{
				// Create the native renderer for the EmptyView, and keep the actual Forms element (if any)
				// around for updating the layout later
				var (NativeView, FormsElement) = RealizeEmptyView(emptyView, _itemsView.EmptyViewTemplate);
				_emptyUIView = NativeView;
				_emptyViewFormsElement = FormsElement;
			}

			// If the empty view is being displayed, we might need to update it
			UpdateEmptyViewVisibility(_itemsSource?.Count == 0);
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
				}

				_currentBackgroundIsEmptyView = false;
			}
		}

		public (UIView NativeView, VisualElement FormsElement) RealizeEmptyView(object emptyView, DataTemplate emptyViewTemplate)
		{
			if (emptyViewTemplate != null)
			{
				// Run this through the extension method in case it's really a DataTemplateSelector
				emptyViewTemplate = emptyViewTemplate.SelectDataTemplate(emptyView, _itemsView);

				// We have a template; turn it into a Forms view 
				var templateElement = emptyViewTemplate.CreateContent() as View;
				var renderer = CreateRenderer(templateElement);

				// and set the EmptyView as its BindingContext
				BindableObject.SetInheritedBindingContext(renderer.Element, emptyView);

				return (renderer.NativeView, renderer.Element);
			}

			if (emptyView is View formsView)
			{
				// No template, and the EmptyView is a Forms view; use that
				var renderer = CreateRenderer(formsView);

				return (renderer.NativeView, renderer.Element);
			}

			// No template, EmptyView is not a Forms View, so just display EmptyView.ToString
			var label = new UILabel { Text = emptyView.ToString() };
			return (label, null);
		}
	}
}