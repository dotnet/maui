using System.Collections.Specialized;
using System.Linq;
using Microsoft.Maui.Controls.Platform;
using Tizen.UIExtensions.NUI;
using IMeasurable = Tizen.UIExtensions.Common.IMeasurable;
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using TScrollToPosition = Tizen.UIExtensions.Common.ScrollToPosition;
using TSize = Tizen.UIExtensions.Common.Size;
using TSnapPointsAlignment = Tizen.UIExtensions.NUI.SnapPointsAlignment;
using TSnapPointsType = Tizen.UIExtensions.NUI.SnapPointsType;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public abstract class MauiCollectionView<TItemsView> : TCollectionView, IMeasurable where TItemsView : ItemsView
	{
		INotifyCollectionChanged? _observableSource;
		protected TItemsView? ItemsView { get; set; }
		protected IItemsLayout? ItemsLayout { get; private set; }

		public virtual void SetupNewElement(TItemsView newElement)
		{
			if (newElement == null)
			{
				ItemsView = null;
				return;
			}

			Scrolled += OnScrolled;

			ItemsView = newElement;
			ItemsView.ScrollToRequested += OnScrollToRequested;
		}

		public virtual void TearDownOldElement(TItemsView oldElement)
		{
			if (ItemsLayout != null)
			{
				ItemsLayout.PropertyChanged -= OnLayoutPropertyChanged;
			}
			oldElement.ScrollToRequested -= OnScrollToRequested;

			using var oldAdaptor = Adaptor;
			Adaptor = null;
			LayoutManager = null;
		}

		public virtual void UpdateItemsSource()
		{
			if (ItemsView == null)
				return;

			if (ItemsView.ItemsSource is INotifyCollectionChanged collectionChanged)
			{
				if (_observableSource != null)
				{
					_observableSource.CollectionChanged -= OnCollectionChanged;
				}
				_observableSource = collectionChanged;
				_observableSource.CollectionChanged += OnCollectionChanged;
			}
			UpdateAdaptor();
		}

		public virtual void UpdateAdaptor()
		{
			if (ItemsView == null)
				return;

			using var oldAdaptor = Adaptor;
			if (Adaptor is ItemTemplateAdaptor old)
			{
				old.SelectionChanged -= OnItemSelectedFromUI;
			}

			if (ItemsView.ItemsSource == null || !ItemsView.ItemsSource.Cast<object>().Any())
			{
				Adaptor = EmptyItemAdaptor.Create(ItemsView);
			}
			else
			{
				Adaptor = CreateItemAdaptor(ItemsView);
			}

			if (Adaptor is ItemTemplateAdaptor adaptor)
			{
				adaptor.SelectionChanged += OnItemSelectedFromUI;
			}
		}

		public virtual void UpdateLayoutManager()
		{
			if (ItemsLayout != null)
			{
				ItemsLayout.PropertyChanged -= OnLayoutPropertyChanged;
			}
			ItemsLayout = GetItemsLayout();
			if (ItemsLayout != null)
			{
				LayoutManager = ItemsLayout.ToLayoutManager((ItemsView as StructuredItemsView)?.ItemSizingStrategy ?? ItemSizingStrategy.MeasureFirstItem);
				ItemsLayout.PropertyChanged += OnLayoutPropertyChanged;
				if (ItemsLayout is ItemsLayout itemsLayout)
				{
					SnapPointsType = (TSnapPointsType)itemsLayout.SnapPointsType;
					SnapPointsAlignment = (TSnapPointsAlignment)itemsLayout.SnapPointsAlignment;
				}
			}
		}

		public void UpdateHorizontalScrollBarVisibility()
		{
			if (ItemsView == null || LayoutManager == null)
				return;

			if (LayoutManager.IsHorizontal)
			{
				ScrollView.HideScrollbar = ItemsView.HorizontalScrollBarVisibility == ScrollBarVisibility.Never;
			}
		}

		public void UpdateVerticalScrollBarVisibility()
		{
			if (ItemsView == null || LayoutManager == null)
				return;

			if (!LayoutManager.IsHorizontal)
			{
				ScrollView.HideScrollbar = ItemsView.VerticalScrollBarVisibility == ScrollBarVisibility.Never;
			}
		}

		public abstract IItemsLayout GetItemsLayout();

		TSize IMeasurable.Measure(double availableWidth, double availableHeight)
		{
			if (Adaptor == null || LayoutManager == null || AllocatedSize == TSize.Zero)
			{
				var scaled = Devices.DeviceDisplay.MainDisplayInfo.GetScaledScreenSize();
				var size = new TSize(availableWidth, availableHeight);
				if (size.Width == double.PositiveInfinity)
					size.Width = scaled.Width.ToScaledPixel();
				if (size.Height == double.PositiveInfinity)
					size.Height = scaled.Height.ToScaledPixel();
				return size;
			}

			var canvasSize = LayoutManager.GetScrollCanvasSize();
			canvasSize.Width = System.Math.Min(canvasSize.Width, availableWidth);
			canvasSize.Height = System.Math.Min(canvasSize.Height, availableHeight);

			return canvasSize;
		}

		void OnLayoutPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (sender == null)
				return;

			if (e.PropertyName == nameof(LinearItemsLayout.ItemSpacing)
				|| e.PropertyName == nameof(GridItemsLayout.VerticalItemSpacing)
				|| e.PropertyName == nameof(GridItemsLayout.HorizontalItemSpacing)
				|| e.PropertyName == nameof(Controls.ItemsLayout.SnapPointsType)
				|| e.PropertyName == nameof(Controls.ItemsLayout.SnapPointsAlignment))
			{
				UpdateLayoutManager();
			}
			else if (e.PropertyName == nameof(GridItemsLayout.Span))
			{
				(LayoutManager as GridLayoutManager)?.UpdateSpan(((GridItemsLayout)sender).Span);
			}
		}

		void OnScrolled(object? sender, CollectionViewScrolledEventArgs e)
		{
			if (ItemsView == null || Adaptor == null)
				return;

			ItemsView.SendScrolled(new ItemsViewScrolledEventArgs
			{
				HorizontalDelta = e.HorizontalDelta.ToScaledDP(),
				HorizontalOffset = e.HorizontalOffset.ToScaledDP(),
				VerticalDelta = e.VerticalDelta.ToScaledDP(),
				VerticalOffset = e.VerticalOffset.ToScaledDP(),
				FirstVisibleItemIndex = e.FirstVisibleItemIndex,
				CenterItemIndex = e.CenterItemIndex,
				LastVisibleItemIndex = e.LastVisibleItemIndex,
			});

			if (ItemsView.RemainingItemsThreshold >= 0)
			{
				if (Adaptor.Count - 1 - e.LastVisibleItemIndex <= ItemsView.RemainingItemsThreshold)
					ItemsView.SendRemainingItemsThresholdReached();
			}
		}

		protected virtual ItemTemplateAdaptor CreateItemAdaptor(ItemsView view)
		{
			return new ItemTemplateAdaptor(view);
		}

		protected virtual void OnItemSelectedFromUI(object? sender, CollectionViewSelectionChangedEventArgs e) { }

		protected virtual int GetIndex(ScrollToRequestEventArgs request)
		{
			return request.Index;
		}

		void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
		{
			if (ItemsView == null)
				return;

			if (ItemsView.ItemsSource == null || !ItemsView.ItemsSource.Cast<object>().Any())
			{
				Adaptor = EmptyItemAdaptor.Create(ItemsView);
			}
			else if (Adaptor is EmptyItemAdaptor)
			{
				UpdateAdaptor();
			}
		}

		void OnScrollToRequested(object? sender, ScrollToRequestEventArgs e)
		{
			if (e.Mode == ScrollToMode.Position)
			{
				ScrollTo(GetIndex(e), (TScrollToPosition)e.ScrollToPosition, e.IsAnimated);
			}
			else
			{
				ScrollTo(e.Item, (TScrollToPosition)e.ScrollToPosition, e.IsAnimated);
			}
		}
	}

	public class MauiStructuredItemsView<TItemsView> : MauiCollectionView<TItemsView> where TItemsView : StructuredItemsView
	{
		public override IItemsLayout GetItemsLayout()
		{
			return ItemsView!.ItemsLayout;
		}
	}

	public class MauiSelectableItemsView<TItemsView> : MauiStructuredItemsView<TItemsView> where TItemsView : SelectableItemsView
	{
		bool _updateSelection;
		bool _updateFromUI;

		public override void UpdateAdaptor()
		{
			base.UpdateAdaptor();
			UpdateSelection();
		}

		public void UpdateSelection()
		{
			if (ItemsView == null || _updateFromUI)
				return;

			_updateSelection = true;

			if (SelectionMode != ItemsView.SelectionMode.ToNative())
			{
				SelectionMode = ItemsView.SelectionMode.ToNative();
			}

			if (Adaptor == null)
			{
				_updateSelection = false;
				return;
			}

			// Sync SelectedItem from Maui to Native
			if (ItemsView.SelectionMode == Controls.SelectionMode.Single)
			{
				var selected = Adaptor.GetItemIndex(ItemsView.SelectedItem);
				foreach (var index in SelectedItems.ToList())
				{
					if (selected != index)
						RequestItemUnselect(index);
				}
				if (selected != -1)
					RequestItemSelect(selected);

			}
			else if (ItemsView.SelectionMode == Controls.SelectionMode.Multiple)
			{
				var selectedItemIndexes = ItemsView.SelectedItems.Select(d => Adaptor.GetItemIndex(d)).ToHashSet();
				foreach (var index in SelectedItems.ToList())
				{
					if (index < 0 || Adaptor.Count <= index)
						continue;

					if (!selectedItemIndexes.Contains(index))
					{
						RequestItemUnselect(index);
					}
				}
				var alreadySelected = SelectedItems.ToHashSet();
				foreach (var selected in selectedItemIndexes)
				{
					if (!alreadySelected.Contains(selected))
					{
						RequestItemSelect(selected);
					}
				}
			}

			_updateSelection = false;
		}

		protected override void OnItemSelectedFromUI(object? sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (ItemsView == null || Adaptor == null || _updateSelection)
			{
				return;
			}

			_updateFromUI = true;

			if (ItemsView.SelectionMode == Controls.SelectionMode.Single)
			{
				ItemsView.SelectedItem = e.SelectedItems?.FirstOrDefault() ?? null;
			}
			else if (ItemsView.SelectionMode == Controls.SelectionMode.Multiple)
			{
				ItemsView.SelectedItems = e.SelectedItems;
			}

			_updateFromUI = false;
		}
	}

	public class MauiGroupableItemsView<TItemsView> : MauiSelectableItemsView<TItemsView> where TItemsView : GroupableItemsView
	{
		protected override ItemTemplateAdaptor CreateItemAdaptor(ItemsView view)
		{
			if (view is GroupableItemsView groupableItemsView && groupableItemsView.IsGrouped)
			{
				return new GroupItemTemplateAdaptor(groupableItemsView, new GroupItemSource(groupableItemsView));
			}
			return base.CreateItemAdaptor(view);
		}

		protected override int GetIndex(ScrollToRequestEventArgs request)
		{
			if (Adaptor is GroupItemTemplateAdaptor groupAdaptor)
			{
				return groupAdaptor.GetAbsoluteIndex(request.GroupIndex, request.Index);
			}
			return base.GetIndex(request);
		}
	}
}
