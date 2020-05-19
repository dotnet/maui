using System;
using System.Collections.Specialized;
using System.Linq;

using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public abstract class ItemsViewRenderer<TItemsView, TNative> : ViewRenderer<TItemsView, TNative>
		where TItemsView : ItemsView
		where TNative : Native.CollectionView
	{
		INotifyCollectionChanged _observableSource;

		protected IItemsLayout ItemsLayout { get; private set; }

		public ItemsViewRenderer()
		{
			RegisterPropertyHandler(ItemsView.ItemsSourceProperty, UpdateItemsSource);
			RegisterPropertyHandler(ItemsView.ItemTemplateProperty, UpdateAdaptor);
		}

		protected abstract TNative CreateNativeControl(ElmSharp.EvasObject parent);

		protected override void OnElementChanged(ElementChangedEventArgs<TItemsView> e)
		{
			if (Control == null)
			{
				SetNativeControl(CreateNativeControl(Forms.NativeParent));
				Control.Scrolled += OnScrolled;
			}
			if (e.NewElement != null)
			{
				e.NewElement.ScrollToRequested += OnScrollToRequest;
			}
			base.OnElementChanged(e);
			ItemsLayout = GetItemsLayout();
			UpdateAdaptor(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Element.ScrollToRequested -= OnScrollToRequest;
					ItemsLayout.PropertyChanged -= OnLayoutPropertyChanged;
					Control.Scrolled -= OnScrolled;
				}
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
			}
			base.Dispose(disposing);
		}

		protected void UpdateItemsLayout()
		{
			if (ItemsLayout != null)
			{
				ItemsLayout.PropertyChanged -= OnLayoutPropertyChanged;
			}
			ItemsLayout = GetItemsLayout();
			if (ItemsLayout != null)
			{
				if (Element is CollectionView)
				{
					Control.LayoutManager = ItemsLayout.ToLayoutManager((Element as CollectionView).ItemSizingStrategy);
				}
				else
				{
					Control.LayoutManager = ItemsLayout.ToLayoutManager(ItemSizingStrategy.MeasureFirstItem);
				}
				Control.SnapPointsType = ((ItemsLayout)ItemsLayout)?.SnapPointsType ?? SnapPointsType.None;
				ItemsLayout.PropertyChanged += OnLayoutPropertyChanged;
			}
		}

		protected virtual void OnLayoutPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Xamarin.Forms.ItemsLayout.SnapPointsType))
			{
				Control.SnapPointsType = (sender as ItemsLayout)?.SnapPointsType ?? SnapPointsType.None;
			}
			else if (e.PropertyName == nameof(GridItemsLayout.Span))
			{
				((GridLayoutManager)(Control.LayoutManager)).UpdateSpan(((GridItemsLayout)sender).Span);
			}
		}

		protected abstract IItemsLayout GetItemsLayout();

		protected virtual void OnItemSelectedFromUI(object sender, SelectedItemChangedEventArgs e)
		{
		}

		void OnScrolled(object sender, ItemsViewScrolledEventArgs e)
		{
			Element.SendScrolled(e);
		}

		void OnScrollToRequest(object sender, ScrollToRequestEventArgs e)
		{
			if (e.Mode == ScrollToMode.Position)
			{
				Control.ScrollTo(e.Index, e.ScrollToPosition, e.IsAnimated);
			}
			else
			{
				Control.ScrollTo(e.Item, e.ScrollToPosition, e.IsAnimated);
			}
		}

		void UpdateItemsSource(bool initialize)
		{
			if (Element.ItemsSource is INotifyCollectionChanged collectionChanged)
			{
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
				_observableSource = collectionChanged;
				_observableSource.CollectionChanged += OnCollectionChanged;
			}
			UpdateAdaptor(initialize);
		}

		void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
			{
				Control.Adaptor = EmptyItemAdaptor.Create(Element);
			}
			else
			{
				if (Control.Adaptor is EmptyItemAdaptor)
				{
					UpdateAdaptor(false);
				}
			}
		}

		void UpdateAdaptor(bool initialize)
		{
			if (!initialize)
			{
				if (Element.ItemsSource == null || !Element.ItemsSource.Cast<object>().Any())
				{
					Control.Adaptor = EmptyItemAdaptor.Create(Element);
				}
				else if (Element.ItemTemplate == null)
				{
					Control.Adaptor = new ItemDefaultTemplateAdaptor(Element);
				}
				else
				{
					Control.Adaptor = new ItemTemplateAdaptor(Element);
					Control.Adaptor.ItemSelected += OnItemSelectedFromUI;
				}
			}
		}
	}

	static class ItemsLayoutExtension
	{
		public static ICollectionViewLayoutManager ToLayoutManager(this IItemsLayout layout, ItemSizingStrategy sizing = ItemSizingStrategy.MeasureFirstItem)
		{
			switch (layout)
			{
				case LinearItemsLayout listItemsLayout:
					return new LinearLayoutManager(listItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal, sizing);
				case GridItemsLayout gridItemsLayout:
					return new GridLayoutManager(gridItemsLayout.Orientation == ItemsLayoutOrientation.Horizontal, gridItemsLayout.Span, sizing);
				default:
					break;
			}

			return new LinearLayoutManager(false);
		}
	}
}
