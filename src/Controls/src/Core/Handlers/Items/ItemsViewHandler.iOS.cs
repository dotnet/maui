using System;
using System.Collections.Generic;
using System.Text;
using Foundation;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract partial class ItemsViewHandler<TItemsView> : ViewHandler<TItemsView, UIView> where TItemsView : ItemsView
	{
		ItemsViewLayout _layout;
		bool? _defaultHorizontalScrollVisibility;
		bool? _defaultVerticalScrollVisibility;

		private protected override UIView OnCreateNativeView()
		{
			UpdateLayout();
			Controller = CreateController(ItemsView, _layout);
			return base.OnCreateNativeView();
		}
		protected TItemsView ItemsView => VirtualView;
		protected ItemsViewController<TItemsView> Controller { get; private set; }

		protected override UIView CreateNativeView() => Controller?.View;

		public static void MapItemsSource(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			MapItemsUpdatingScrollMode(handler, itemsView);
			handler.Controller?.UpdateItemsSource();
		}

		public static void MapHorizontalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateHorizontalScrollBarVisibility();
		}

		public static void MapVerticalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateVerticalScrollBarVisibility();
		}

		public static void MapItemTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		public static void MapEmptyView(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateEmptyView();
		}

		public static void MapEmptyViewTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateEmptyView();
		}

		public static void MapFlowDirection(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.UpdateFlowDirection();
		}

		public static void MapIsVisible(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.UpdateVisibility();
		}

		public static void MapItemsUpdatingScrollMode(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler._layout.ItemsUpdatingScrollMode = itemsView.ItemsUpdatingScrollMode;
		}

		public static void MapItemsLayout(ItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		public static void MapItemSizingStrategy(ItemsViewHandler<TItemsView> handler, StructuredItemsView itemsView)
		{
			handler.UpdateLayout();
		}

		protected abstract ItemsViewLayout SelectLayout();

		protected override void DisconnectHandler(UIView nativeView)
		{
			ItemsView.ScrollToRequested -= ScrollToRequested;
			base.DisconnectHandler(nativeView);
		}

		protected override void ConnectHandler(UIView nativeView)
		{
			base.ConnectHandler(nativeView);
			Controller.CollectionView.BackgroundColor = UIColor.Clear;
			ItemsView.ScrollToRequested += ScrollToRequested;
		}

		internal void UpdateEmptyView()
		{
			//if (!_initialized)
			//{
			//	return;
			//}

			//// Get rid of the old view
			//TearDownEmptyView();

			//// Set up the new empty view
			//UpdateView(ItemsView?.EmptyView, ItemsView?.EmptyViewTemplate, ref _emptyUIView, ref _emptyViewFormsElement);

			//// We may need to show the updated empty view
			//UpdateEmptyViewVisibility(ItemsSource?.ItemCount == 0);
		}

		protected virtual void UpdateLayout()
		{
			_layout = SelectLayout();
			Controller?.UpdateLayout(_layout);
		}

		protected abstract ItemsViewController<TItemsView> CreateController(TItemsView newElement, ItemsViewLayout layout);


		void UpdateVerticalScrollBarVisibility()
		{
			if (_defaultVerticalScrollVisibility == null)
				_defaultVerticalScrollVisibility = Controller.CollectionView.ShowsVerticalScrollIndicator;

			switch (ItemsView.VerticalScrollBarVisibility)
			{
				case ScrollBarVisibility.Always:
					Controller.CollectionView.ShowsVerticalScrollIndicator = true;
					break;
				case ScrollBarVisibility.Never:
					Controller.CollectionView.ShowsVerticalScrollIndicator = false;
					break;
				case ScrollBarVisibility.Default:
					Controller.CollectionView.ShowsVerticalScrollIndicator = _defaultVerticalScrollVisibility.Value;
					break;
			}
		}

		void UpdateHorizontalScrollBarVisibility()
		{
			if (_defaultHorizontalScrollVisibility == null)
				_defaultHorizontalScrollVisibility = Controller.CollectionView.ShowsHorizontalScrollIndicator;

			switch (ItemsView.HorizontalScrollBarVisibility)
			{
				case ScrollBarVisibility.Always:
					Controller.CollectionView.ShowsHorizontalScrollIndicator = true;
					break;
				case ScrollBarVisibility.Never:
					Controller.CollectionView.ShowsHorizontalScrollIndicator = false;
					break;
				case ScrollBarVisibility.Default:
					Controller.CollectionView.ShowsHorizontalScrollIndicator = _defaultHorizontalScrollVisibility.Value;
					break;
			}
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

				Controller.CollectionView.ScrollToItem(indexPath,
					args.ScrollToPosition.ToCollectionViewScrollPosition(_layout.ScrollDirection), args.IsAnimated);
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
	}
}
