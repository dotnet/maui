using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml;
using WVisibility = Microsoft.UI.Xaml.Visibility;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.Maui.Controls.Internals;
using WASDKDataTemplate = Microsoft.UI.Xaml.DataTemplate;
using WASDKApp = Microsoft.UI.Xaml.Application;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract partial class ItemsViewHandler<TItemsView> : ViewHandler<TItemsView, ListViewBase>  where TItemsView : ItemsView
	{
		protected CollectionViewSource CollectionViewSource;
		FrameworkElement _emptyView;
		View _formsEmptyView;
		bool _emptyViewDisplayed;
		protected ListViewBase ListViewBase => NativeView;
		protected TItemsView ItemsView => VirtualView;
		protected TItemsView Element => VirtualView;
		protected WASDKDataTemplate ItemsViewTemplate => (WASDKDataTemplate)WASDKApp.Current.Resources["ItemsViewDefaultTemplate"];

		protected override ListViewBase CreateNativeView()
		{
			return SelectListViewBase();
		}

		public static void MapItemsSource(ItemsViewHandler<TItemsView> handler, ItemsView itemsView) 
		{
			handler.UpdateItemsSource();
		}

		public static void MapHorizontalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}
		public static void MapVerticalScrollBarVisibility(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapItemTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateItemTemplate();
		}

		public static void MapEmptyView(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
			handler.UpdateEmptyView();
		}

		public static void MapEmptyViewTemplate(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapFlowDirection(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapIsVisible(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		public static void MapItemsUpdatingScrollMode(ItemsViewHandler<TItemsView> handler, ItemsView itemsView)
		{
		}

		protected abstract ListViewBase SelectListViewBase();

		protected virtual void CleanUpCollectionViewSource()
		{
			if (CollectionViewSource != null)
			{
				if (CollectionViewSource.Source is ObservableItemTemplateCollection observableItemTemplateCollection)
				{
					observableItemTemplateCollection.CleanUp();
				}

				if (CollectionViewSource.Source is INotifyCollectionChanged incc)
				{
					incc.CollectionChanged -= ItemsChanged;
				}

				CollectionViewSource.Source = null;
				CollectionViewSource = null;
			}

			if (VirtualView?.ItemsSource == null)
			{
				ListViewBase.ItemsSource = null;
				return;
			}
		}

		void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			UpdateEmptyViewVisibility();
		}

		protected virtual void UpdateEmptyViewVisibility()
		{
			bool isEmpty = (CollectionViewSource?.View?.Count ?? 0) == 0;

			if (isEmpty)
			{
				if (_formsEmptyView != null)
				{
					if (_emptyViewDisplayed)
						ItemsView.RemoveLogicalChild(_formsEmptyView);

					if (ItemsView.EmptyViewTemplate == null)
						ItemsView.AddLogicalChild(_formsEmptyView);
				}

				if (_emptyView != null && ListViewBase is IEmptyView emptyView)
				{
					emptyView.EmptyViewVisibility = WVisibility.Visible;

					if (NativeView.ActualWidth >= 0 && NativeView.ActualHeight >= 0)
						_formsEmptyView?.Layout(new Rectangle(0, 0, NativeView.ActualWidth, NativeView.ActualHeight));
				}

				_emptyViewDisplayed = true;
			}
			else
			{
				if (_emptyViewDisplayed)
				{
					if (_emptyView != null && ListViewBase is IEmptyView emptyView)
						emptyView.EmptyViewVisibility = WVisibility.Collapsed;

					ItemsView.RemoveLogicalChild(_formsEmptyView);
				}

				_emptyViewDisplayed = false;
			}
		}

		protected virtual void UpdateItemsSource()
		{
			if (ListViewBase == null)
			{
				return;
			}

			CleanUpCollectionViewSource();

			if (Element.ItemsSource == null)
			{
				return;
			}

			CollectionViewSource = CreateCollectionViewSource();

			if (CollectionViewSource?.Source is INotifyCollectionChanged incc)
			{
				incc.CollectionChanged += ItemsChanged;
			}

			ListViewBase.ItemsSource = GetCollectionView(CollectionViewSource);

			UpdateEmptyViewVisibility();
		}

		protected virtual void UpdateItemTemplate()
		{
			if (Element == null || ListViewBase == null)
			{
				return;
			}

			ListViewBase.ItemTemplate = Element.ItemTemplate == null ? null : ItemsViewTemplate;

			UpdateItemsSource();
		}

		protected virtual CollectionViewSource CreateCollectionViewSource()
		{
			var itemsSource = Element.ItemsSource;
			var itemTemplate = Element.ItemTemplate;

			if (itemTemplate != null)
			{
				return new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory.Create(itemsSource, itemTemplate, Element, mauiContext: MauiContext),
					IsSourceGrouped = false
				};
			}

			return new CollectionViewSource
			{
				Source = itemsSource,
				IsSourceGrouped = false
			};
		}

		protected virtual ICollectionView GetCollectionView(CollectionViewSource collectionViewSource)
		{
			return collectionViewSource.View;
		}

		protected virtual void UpdateEmptyView()
		{
			if (Element == null || ListViewBase == null)
			{
				return;
			}

			var emptyView = Element.EmptyView;

			if (emptyView == null)
			{
				return;
			}

			switch (emptyView)
			{
				case string text:
					_emptyView = new TextBlock
					{
						HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
						VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
						Text = text
					};
					break;
				case View view:
					_emptyView = RealizeEmptyView(view);
					break;
				default:
					_emptyView = RealizeEmptyViewTemplate(emptyView, Element.EmptyViewTemplate);
					break;
			}

			(ListViewBase as IEmptyView)?.SetEmptyView(_emptyView, _formsEmptyView);

			UpdateEmptyViewVisibility();
		}

		FrameworkElement RealizeEmptyViewTemplate(object bindingContext, DataTemplate emptyViewTemplate)
		{
			if (emptyViewTemplate == null)
			{
				return new TextBlock
				{
					HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center,
					VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Center,
					Text = bindingContext.ToString()
				};
			}

			var template = emptyViewTemplate.SelectDataTemplate(bindingContext, null);
			var view = template.CreateContent() as View;
			view.BindingContext = bindingContext;

			return RealizeEmptyView(view);
		}

		FrameworkElement RealizeEmptyView(View view)
		{
			_formsEmptyView = view ?? throw new ArgumentNullException(nameof(view));
			return view.GetOrCreateHandler(MauiContext).ContainerView as FrameworkElement;
		}
	}
}
