using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Handlers.Items;
using Tizen.UIExtensions.ElmSharp;
using DPExtensions = Tizen.UIExtensions.ElmSharp.DPExtensions;
using TCollectionView = Tizen.UIExtensions.ElmSharp.CollectionView;
using TCollectionViewSelectionMode = Tizen.UIExtensions.ElmSharp.CollectionViewSelectionMode;
using TItemSizingStrategy = Tizen.UIExtensions.ElmSharp.ItemSizingStrategy;
using TSelectedItemChangedEventArgs = Tizen.UIExtensions.ElmSharp.SelectedItemChangedEventArgs;
using TSnapPointsType = Tizen.UIExtensions.ElmSharp.SnapPointsType;

namespace Microsoft.Maui.Controls.Platform
{
	public static class CollectionViewExtensions
	{
		static INotifyCollectionChanged _observableSource;

		public static void UpdateItemsSource(this TCollectionView platformView, ItemsView view)
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
			UpdateAdaptor(platformView, view);

			void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
			{
				if (view.ItemsSource == null || !view.ItemsSource.Cast<object>().Any())
				{
					platformView.Adaptor = EmptyItemAdaptor.Create(view);
				}
				else
				{
					if (platformView.Adaptor is EmptyItemAdaptor)
					{
						platformView.UpdateAdaptor(view);
					}
				}
			}
		}

		public static void UpdateItemsLayout(this TCollectionView platformView, StructuredItemsView view)
		{
			if (view.ItemsLayout != null)
			{
				var itemSizingStrategy = view.ItemSizingStrategy.ToPlatform();
				if (view.ItemsLayout is GridItemsLayout grid)
				{
					var orientation = grid.Orientation == ItemsLayoutOrientation.Horizontal;
					var verticalItemSpacing = DPExtensions.ConvertToScaledPixel(grid.VerticalItemSpacing);
					var horizontalItemSpacing = DPExtensions.ConvertToScaledPixel(grid.HorizontalItemSpacing);
					platformView.LayoutManager = new GridLayoutManager(orientation, grid.Span, itemSizingStrategy, verticalItemSpacing, horizontalItemSpacing);
				}
				else if (view.ItemsLayout is LinearItemsLayout linear)
				{
					var orientation = linear.Orientation == ItemsLayoutOrientation.Horizontal;
					var itemSpacing = DPExtensions.ConvertToScaledPixel(linear.ItemSpacing);
					platformView.LayoutManager = new LinearLayoutManager(orientation, itemSizingStrategy, itemSpacing);
				}
				else
				{
					platformView.LayoutManager = new LinearLayoutManager(false);
				}
				platformView.SnapPointsType = (view.ItemsLayout as ItemsLayout).SnapPointsType.ToPlatform();
				platformView.SelectionMode = (view as SelectableItemsView).SelectionMode.ToPlatform();
			}
		}

		public static void UpdateAdaptor(this TCollectionView platformView, ItemsView view)
		{
			if (view.ItemsSource == null || !view.ItemsSource.Cast<object>().Any())
			{
				platformView.Adaptor = EmptyItemAdaptor.Create(view);
			}
			else if (view.ItemTemplate == null)
			{
				platformView.Adaptor = new ItemDefaultTemplateAdaptor(view);
			}
			else
			{
				platformView.Adaptor = new ItemTemplateAdaptor(view);
			}
			platformView.Adaptor.ItemSelected += OnItemSelected;
		}

		static void OnItemSelected(object sender, TSelectedItemChangedEventArgs e)
		{
			(sender as ItemTemplateAdaptor)?.SendItemSelected(e.SelectedItem);
		}

		public static TItemSizingStrategy ToPlatform(this ItemSizingStrategy itemSizingStrategy)
		{
			if (itemSizingStrategy == ItemSizingStrategy.MeasureAllItems)
				return TItemSizingStrategy.MeasureAllItems;
			return TItemSizingStrategy.MeasureFirstItem;
		}

		public static TSnapPointsType ToPlatform(this SnapPointsType snapPointsType)
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

		public static TCollectionViewSelectionMode ToPlatform(this SelectionMode selectionMode)
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
