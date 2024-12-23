﻿#nullable disable
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
		}

		public static void MapIsVisible(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			handler.Controller?.UpdateVisibility();
		}

		public static void MapItemsUpdatingScrollMode(ItemsViewHandler2<TItemsView> handler, ItemsView itemsView)
		{
			// TODO: Fix handler._layout.ItemsUpdatingScrollMode = itemsView.ItemsUpdatingScrollMode;
		}

		//TODO: this is being called 2 times on startup, one from OnCreatePlatformView and otehr from the mapper for the layout
		protected virtual void UpdateLayout()
		{
			_layout = SelectLayout();
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

				var position = Items.ScrollToPositionExtensions.ToCollectionViewScrollPosition(args.ScrollToPosition, UICollectionViewScrollDirection.Vertical);

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

			// Our target size is the smaller of it and the constraints
			var width = contentSize.Width <= widthConstraint ? contentSize.Width : widthConstraint;
			var height = contentSize.Height <= heightConstraint ? contentSize.Height : heightConstraint;

			IView virtualView = VirtualView;

			width = ViewHandlerExtensions.ResolveConstraints(width, virtualView.Width, virtualView.MinimumWidth, virtualView.MaximumWidth);
			height = ViewHandlerExtensions.ResolveConstraints(height, virtualView.Height, virtualView.MinimumHeight, virtualView.MaximumHeight);

			return new Size(width, height);
		}
	}
}
