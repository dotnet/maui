using System.Collections.Specialized;
using System.Linq;

using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class ItemsViewRenderer : ViewRenderer<StructuredItemsView, Native.CollectionView>
	{
		INotifyCollectionChanged _observableSource;

		public ItemsViewRenderer()
		{
			RegisterPropertyHandler(ItemsView.ItemsSourceProperty, UpdateItemsSource);
			RegisterPropertyHandler(ItemsView.ItemTemplateProperty, UpdateAdaptor);
			RegisterPropertyHandler(StructuredItemsView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(StructuredItemsView.ItemSizingStrategyProperty, UpdateSizingStrategy);
			RegisterPropertyHandler(SelectableItemsView.SelectedItemProperty, UpdateSelectedItem);
			RegisterPropertyHandler(SelectableItemsView.SelectionModeProperty, UpdateSelectionMode);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<StructuredItemsView> e)
		{
			if (Control == null)
			{
				SetNativeControl(new Native.CollectionView(Forms.NativeParent));
			}

			if (e.NewElement != null)
			{
				e.NewElement.ScrollToRequested += OnScrollToRequest;
			}

			base.OnElementChanged(e);
			UpdateAdaptor(false);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Element != null)
				{
					Element.ScrollToRequested -= OnScrollToRequest;
					Element.ItemsLayout.PropertyChanged -= OnLayoutPropertyChanged;
				}
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
			}
			base.Dispose(disposing);
		}

		void UpdateSelectedItem(bool initialize)
		{
			if (initialize)
				return;

			if (Element is SelectableItemsView selectable)
			{
				Control?.Adaptor?.RequestItemSelected(selectable.SelectedItem);
			}
		}

		void UpdateSelectionMode()
		{
			if (Element is SelectableItemsView selectable)
			{
				Control.SelectionMode = selectable.SelectionMode == SelectionMode.None ? CollectionViewSelectionMode.None : CollectionViewSelectionMode.Single;
			}
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

		void OnItemSelectedFromUI(object sender, SelectedItemChangedEventArgs e)
		{
			if (Element is SelectableItemsView selectableItemsView)
			{
				selectableItemsView.SelectedItem = e.SelectedItem;
			}
		}

		void UpdateItemsLayout()
		{
			if (Element.ItemsLayout != null)
			{
				Control.LayoutManager = Element.ItemsLayout.ToLayoutManager(Element.ItemSizingStrategy);
				Control.SnapPointsType = (Element.ItemsLayout as ItemsLayout)?.SnapPointsType ?? SnapPointsType.None;
				Element.ItemsLayout.PropertyChanged += OnLayoutPropertyChanged;
			}
		}

		void UpdateSizingStrategy(bool initialize)
		{
			if (initialize)
			{
				return;
			}
			Control.LayoutManager = Element.ItemsLayout.ToLayoutManager(Element.ItemSizingStrategy);
		}

		void OnLayoutPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(ItemsLayout.SnapPointsType))
			{
				Control.SnapPointsType = (Element.ItemsLayout as ItemsLayout)?.SnapPointsType ?? SnapPointsType.None;
			}
			else if (e.PropertyName == nameof(GridItemsLayout.Span))
			{
				((GridLayoutManager)(Control.LayoutManager)).UpdateSpan(((GridItemsLayout)Element.ItemsLayout).Span);
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
