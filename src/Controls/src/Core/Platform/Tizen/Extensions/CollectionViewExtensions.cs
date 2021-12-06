using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Handlers.Items;
using Tizen.UIExtensions.ElmSharp;
using TCollectionView = Tizen.UIExtensions.ElmSharp.CollectionView;
using TCollectionViewSelectionMode = Tizen.UIExtensions.ElmSharp.CollectionViewSelectionMode;
using TItemSizingStrategy = Tizen.UIExtensions.ElmSharp.ItemSizingStrategy;
using TSelectedItemChangedEventArgs = Tizen.UIExtensions.ElmSharp.SelectedItemChangedEventArgs;
using TSnapPointsType = Tizen.UIExtensions.ElmSharp.SnapPointsType;
using DPExtensions = Tizen.UIExtensions.ElmSharp.DPExtensions;

namespace Microsoft.Maui.Controls.Platform
{
	public static class CollectionViewExtensions
	{
		static INotifyCollectionChanged _observableSource;

		public static void UpdateItemsSource(this TCollectionView nativeView, ItemsView view)
		{
			if (view.ItemsSource is INotifyCollectionChanged collectionChanged)
			{
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
				_observableSource = collectionChanged;
				_observableSource.CollectionChanged += OnCollectionChanged;
			}
			UpdateAdaptor(nativeView, view);

			void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				if (view.ItemsSource == null || !view.ItemsSource.Cast<object>().Any())
				{
					nativeView.Adaptor = EmptyItemAdaptor.Create(view);
				}
				else
				{
					if (nativeView.Adaptor is EmptyItemAdaptor)
					{
						nativeView.UpdateAdaptor(view);
					}
				}
			}
		}

		public static void UpdateItemsLayout(this TCollectionView nativeView, StructuredItemsView view)
		{
			if (view.ItemsLayout != null)
			{
				var itemSizingStrategy = view.ItemSizingStrategy.ToNative();
				if (view.ItemsLayout is GridItemsLayout grid)
				{
					var orientation = grid.Orientation == ItemsLayoutOrientation.Horizontal;
					var verticalItemSpacing = DPExtensions.ConvertToScaledPixel(grid.VerticalItemSpacing);
					var horizontalItemSpacing = DPExtensions.ConvertToScaledPixel(grid.HorizontalItemSpacing);
					nativeView.LayoutManager = new GridLayoutManager(orientation, grid.Span, itemSizingStrategy, verticalItemSpacing, horizontalItemSpacing);
				}
				else if (view.ItemsLayout is LinearItemsLayout linear)
				{
					var orientation = linear.Orientation == ItemsLayoutOrientation.Horizontal;
					var itemSpacing = DPExtensions.ConvertToScaledPixel(linear.ItemSpacing);
					nativeView.LayoutManager = new LinearLayoutManager(orientation, itemSizingStrategy, itemSpacing);
				}
				else
				{
					nativeView.LayoutManager = new LinearLayoutManager(false);
				}
				nativeView.SnapPointsType = (view.ItemsLayout as ItemsLayout).SnapPointsType.ToNative();
				nativeView.SelectionMode = (view as SelectableItemsView).SelectionMode.ToNative();
			}
		}

		public static void UpdateAdaptor(this TCollectionView nativeView, ItemsView view)
		{
			if (view.ItemsSource == null || !view.ItemsSource.Cast<object>().Any())
			{
				nativeView.Adaptor = EmptyItemAdaptor.Create(view);
			}
			else if (view.ItemTemplate == null)
			{
				nativeView.Adaptor = new ItemDefaultTemplateAdaptor(view);
			}
			else
			{
				nativeView.Adaptor = new ItemTemplateAdaptor(view);
			}
			nativeView.Adaptor.ItemSelected += OnItemSelected;
		}

		static void OnItemSelected(object sender, TSelectedItemChangedEventArgs e)
		{
			(sender as ItemTemplateAdaptor)?.SendItemSelected(e.SelectedItem);
		}

		public static TItemSizingStrategy ToNative(this ItemSizingStrategy itemSizingStrategy)
		{
			if (itemSizingStrategy == ItemSizingStrategy.MeasureAllItems)
				return TItemSizingStrategy.MeasureAllItems;
			return TItemSizingStrategy.MeasureFirstItem;
		}

		public static TSnapPointsType ToNative(this SnapPointsType snapPointsType)
		{
			switch (snapPointsType)
			{
				case SnapPointsType.Mandatory:
					return TSnapPointsType.Mandatory;
				case SnapPointsType.MandatorySingle:
					return TSnapPointsType.MandatorySingle;
				default:
					return TSnapPointsType.None;
			}
		}

		public static TCollectionViewSelectionMode ToNative(this SelectionMode selectionMode)
		{
			switch (selectionMode)
			{
				case SelectionMode.Multiple:
					return TCollectionViewSelectionMode.Multiple;
				case SelectionMode.Single:
					return TCollectionViewSelectionMode.Single;
				default:
					return TCollectionViewSelectionMode.None;
			}
		}
	}
}
