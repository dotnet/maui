using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using CoreGraphics;
using Foundation;
using UIKit;

namespace Xamarin.Forms.Platform.iOS
{
	// TODO hartez 2018/06/01 14:17:00 Implement Dispose override ?	
	// TODO hartez 2018/06/01 14:21:24 Add a method for updating the layout	
	internal class CollectionViewController : UICollectionViewController
	{
		IItemsViewSource _itemsSource;
		readonly ItemsView _itemsView;
		readonly ItemsViewLayout _layout;
		bool _initialConstraintsSet;
		bool _wasEmpty;

		UIView _backgroundUIView;
		UIView _emptyUIView;
		VisualElement _emptyViewFormsElement;

		public CollectionViewController(ItemsView itemsView, ItemsViewLayout layout) : base(layout)
		{
			_itemsView = itemsView;
			_itemsSource = ItemsSourceFactory.Create(_itemsView.ItemsSource, CollectionView);
			_layout = layout;

			_layout.GetPrototype = GetPrototype;
			_layout.UniformSize = false; // todo hartez Link this to ItemsView.ItemSizingStrategy hint
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
			_itemsSource =  ItemsSourceFactory.Create(_itemsView.ItemsSource, CollectionView);
			CollectionView.ReloadData();
			CollectionView.CollectionViewLayout.InvalidateLayout();
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

		void ApplyTemplateAndDataContext(TemplatedCell cell, NSIndexPath indexPath)
		{
			// We need to create a renderer, which means we need a template
			var templateElement = _itemsView.ItemTemplate.CreateContent() as View;
			IVisualElementRenderer renderer = CreateRenderer(templateElement);

			if (renderer != null)
			{
				BindableObject.SetInheritedBindingContext(renderer.Element, _itemsSource[indexPath.Row]);
				cell.SetRenderer(renderer);
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
				// Nope, no EmptyView set. So nothing to display. If there _was_ a background view on the UICollectionView, 
				// we should restore it here (in case the EmptyView _used to be_ set, and has been un-set)
				if(_backgroundUIView != null)
				{
					CollectionView.BackgroundView = _backgroundUIView;
				}

				// Also, clear the cached version
				_emptyUIView = null;

				return;
			}

			if (_emptyUIView == null)
			{
				// Create the native renderer for the EmptyView, and keep the actual Forms element (if any)
				// around for updating the layout later
				var (NativeView, FormsElement) = RealizeEmptyView(emptyView, _itemsView.EmptyViewTemplate);
				_emptyUIView = NativeView;
				_emptyViewFormsElement = FormsElement;
			}
		}

		void UpdateEmptyViewVisibility(bool isEmpty)
		{
			if (isEmpty)
			{
				// Cache any existing background view so we can restore it later
				_backgroundUIView = CollectionView.BackgroundView;

				// Replace any current background with the EmptyView. This will also set the native view's frame
				// to match the UICollectionView's frame
				CollectionView.BackgroundView = _emptyUIView;

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
				if (CollectionView.BackgroundView == _emptyUIView)
				{
					CollectionView.BackgroundView = _backgroundUIView;
				}
			}
		}

		public (UIView NativeView, VisualElement FormsElement) RealizeEmptyView(object emptyView, DataTemplate emptyViewTemplate)
		{
			if (emptyViewTemplate != null)
			{
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