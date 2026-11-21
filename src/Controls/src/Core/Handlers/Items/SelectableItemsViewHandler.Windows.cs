#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Handlers;
using Microsoft.UI.Xaml.Controls;
using WASDKListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;
using WASDKSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;

namespace Microsoft.Maui.Controls.Handlers.Items
{
	public partial class SelectableItemsViewHandler<TItemsView> : StructuredItemsViewHandler<TItemsView> where TItemsView : SelectableItemsView
	{
		bool _ignorePlatformSelectionChange;
		bool _ignoreVirtualSelectionChange;

		protected override void ConnectHandler(ListViewBase platformView)
		{
			base.ConnectHandler(platformView);

			if (ItemsView != null)
			{
				ItemsView.SelectionChanged += VirtualSelectionChanged;
			}

			var newListViewBase = ListViewBase;

			if (newListViewBase != null)
			{
				newListViewBase.SetBinding(ListViewBase.SelectionModeProperty,
						new Microsoft.UI.Xaml.Data.Binding
						{
							Source = ItemsView,
							Path = new Microsoft.UI.Xaml.PropertyPath("SelectionMode"),
							Converter = new SelectionModeConvert(),
							Mode = Microsoft.UI.Xaml.Data.BindingMode.TwoWay
						});

				newListViewBase.SelectionChanged += PlatformSelectionChanged;
				newListViewBase.ItemClick += PlatformItemClick;
			}

			UpdatePlatformSelection();
		}

		protected override void DisconnectHandler(ListViewBase platformView)
		{
			var oldListViewBase = platformView;

			if (oldListViewBase != null)
			{
				oldListViewBase.SelectionChanged -= PlatformSelectionChanged;
				oldListViewBase.ItemClick -= PlatformItemClick;
				oldListViewBase.ClearValue(ListViewBase.SelectionModeProperty);
			}

			if (ItemsView != null)
			{
				ItemsView.SelectionChanged -= VirtualSelectionChanged;
			}

			base.DisconnectHandler(platformView);
		}

		public static void MapSelectedItem(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectedItems(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		public static void MapSelectionMode(SelectableItemsViewHandler<TItemsView> handler, SelectableItemsView itemsView)
		{
		}

		partial void UpdateIsItemTapEnabled(SelectableItemsView itemsView)
		{
			if (ListViewBase is ListViewBase listViewBase)
			{
				listViewBase.IsItemClickEnabled = itemsView.IsItemTapEnabled;
			}
		}

		void PlatformItemClick(object sender, ItemClickEventArgs args)
		{
			if (ItemsView is not SelectableItemsView itemsView || sender is not ListViewBase listViewBase ||
				args.ClickedItem is GroupFooterItemTemplateContext)
			{
				return;
			}

			var item = args.ClickedItem is ItemTemplateContext itemPair ? itemPair.Item : args.ClickedItem;
			GetItemPosition(listViewBase, args.ClickedItem, out var group, out var groupIndex, out var indexInGroup);
			var tap = itemsView.CreateItemTappedEventArgs(item, group, groupIndex, indexInGroup);

			// ItemClick is raised before the platform selection changes; report the tap after the selection is updated
			if (!listViewBase.DispatcherQueue.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Low, () => { if (itemsView.Handler is not null) itemsView.SendItemTapped(tap); }))
			{
				itemsView.SendItemTapped(tap);
			}
		}

		// Finds the clicked row rather than the first equal item, so repeated items report their own position
		void GetItemPosition(ListViewBase listViewBase, object clickedItem, out object group, out int groupIndex, out int indexInGroup)
		{
			group = null;
			groupIndex = -1;
			indexInGroup = -1;

			// The platform's items are materialized once, so item contexts can be matched by reference
			var row = clickedItem is ItemTemplateContext
				? IndexOfReference(listViewBase.Items, clickedItem)
				: GetFocusedRow(listViewBase, clickedItem);

			if (row < 0)
			{
				return;
			}

			if (CollectionViewSource?.IsSourceGrouped != true)
			{
				indexInGroup = row;
				return;
			}

			// The rows of a grouped source are the items of each group in turn, followed by the group's footer if any.
			// Each group context corresponds to the entry of the items source at the same position.
			var groups = (ItemsView?.ItemsSource as System.Collections.IEnumerable)?.GetEnumerator();

			try
			{
				var position = 0;
				var currentGroup = 0;

				foreach (var entry in CollectionViewSource.Source as System.Collections.IEnumerable ?? Array.Empty<object>())
				{
					var userGroup = groups is not null && groups.MoveNext() ? groups.Current : null;
					var count = entry is GroupTemplateContext { Items: System.Collections.IEnumerable groupItems } ? Count(groupItems) : 0;

					if (row < position + count)
					{
						group = userGroup;
						groupIndex = currentGroup;
						indexInGroup = row - position;
						return;
					}

					position += count;

					if (userGroup is System.Collections.IEnumerable and not string)
					{
						currentGroup++;
					}
				}
			}
			finally
			{
				(groups as IDisposable)?.Dispose();
			}
		}

		// Without an item template the items themselves are shown; the clicked row has focus
		static int GetFocusedRow(ListViewBase listViewBase, object clickedItem)
		{
			if (listViewBase.XamlRoot is not null &&
				Microsoft.UI.Xaml.Input.FocusManager.GetFocusedElement(listViewBase.XamlRoot) is Microsoft.UI.Xaml.Controls.Primitives.SelectorItem container &&
				ItemsControl.ItemsControlFromItemContainer(container) == listViewBase)
			{
				var index = listViewBase.IndexFromContainer(container);
				if (index >= 0 && Equals(listViewBase.Items[index], clickedItem))
				{
					return index;
				}
			}

			return -1;
		}

		static int Count(System.Collections.IEnumerable items)
		{
			if (items is System.Collections.ICollection collection)
			{
				return collection.Count;
			}

			var count = 0;

			foreach (var _ in items)
			{
				count++;
			}

			return count;
		}

		static int IndexOfReference(System.Collections.IEnumerable items, object item)
		{
			var index = 0;

			foreach (var candidate in items)
			{
				if (ReferenceEquals(candidate, item))
				{
					return index;
				}

				index++;
			}

			return -1;
		}

		void UpdatePlatformSelection()
		{
			_ignorePlatformSelectionChange = true;

			switch (ListViewBase.SelectionMode)
			{
				case WASDKListViewSelectionMode.None:
					break;
				case WASDKListViewSelectionMode.Single:
					if (ItemsView != null)
					{
						if (ItemsView.SelectedItem == null)
						{
							ListViewBase.SelectedItem = null;
						}
						else
						{
							ListViewBase.SelectedItem =
								ListViewBase.Items.FirstOrDefault(item =>
								{
									if (item is ItemTemplateContext itemPair)
									{
										return object.Equals(itemPair.Item, ItemsView.SelectedItem);
									}
									else
									{
										return object.Equals(item, ItemsView.SelectedItem);
									}
								});
						}
					}

					break;
				case WASDKListViewSelectionMode.Multiple:
					ListViewBase.SelectedItems.Clear();
					foreach (var nativeItem in ListViewBase.Items)
					{
						if (nativeItem is ItemTemplateContext itemPair && ItemsView.SelectedItems.Contains(itemPair.Item))
						{
							ListViewBase.SelectedItems.Add(nativeItem);
						}
						else if (ItemsView.SelectedItems.Contains(nativeItem))
						{
							ListViewBase.SelectedItems.Add(nativeItem);
						}
					}
					break;
				case WASDKListViewSelectionMode.Extended:
					break;
				default:
					break;
			}

			UpdateItemContentControlSelection(ListViewBase);
			_ignorePlatformSelectionChange = false;
		}

		void VirtualSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// When the selection changes within the SelectionChanged event, the new selection isn't immediately reflected in the view.
			// After the virtual selection is correctly updated, the flag is reset to enable future updates
			if (_ignoreVirtualSelectionChange)
			{
				_ignoreVirtualSelectionChange = false;
				return;
			}
			UpdatePlatformSelection();
		}

		void PlatformSelectionChanged(object sender, WASDKSelectionChangedEventArgs args)
		{
			if (sender is not ListViewBase platformView)
			{
				return;
			}

			UpdateVirtualSelection(platformView);
		}

		void UpdateVirtualSelection(ListViewBase platformView)
		{
			if (_ignorePlatformSelectionChange ||
				!ReferenceEquals(((IElementHandler)this).PlatformView, platformView) ||
				((IElementHandler)this).VirtualView is null)
			{
				return;
			}

			switch (platformView.SelectionMode)
			{
				case WASDKListViewSelectionMode.None:
					break;
				case WASDKListViewSelectionMode.Single:
					UpdateVirtualSingleSelection(platformView);
					break;
				case WASDKListViewSelectionMode.Multiple:
					UpdateVirtualMultipleSelection(platformView);
					break;
				default:
					break;
			}

			if (!ReferenceEquals(((IElementHandler)this).PlatformView, platformView))
			{
				return;
			}

			UpdateItemContentControlSelection(platformView);
		}

		void UpdateVirtualSingleSelection(ListViewBase platformView)
		{
			var selectedItem = platformView.SelectedItem is ItemTemplateContext itemPair
				? itemPair.Item
				: platformView.SelectedItem;

			if (ItemsView != null)
			{
				_ignoreVirtualSelectionChange = true;
				ItemsView.SelectedItem = selectedItem;

				_ignoreVirtualSelectionChange = false;
			}
		}

		void UpdateVirtualMultipleSelection(ListViewBase platformView)
		{
			ItemsView.SelectionChanged -= VirtualSelectionChanged;

			var selection = new List<object>();
			for (int n = 0; n < platformView.SelectedItems.Count; n++)
			{
				var item = platformView.SelectedItems[n];
				selection.Add(item is ItemTemplateContext itc ? itc.Item : item);
			}

			ItemsView.UpdateSelectedItems(selection);

			ItemsView.SelectionChanged += VirtualSelectionChanged;
		}

		void UpdateItemContentControlSelection(ListViewBase platformView)
		{
			var formsItemContentControls = platformView.GetChildren<ItemContentControl>();

			foreach (var formsItemContentControl in formsItemContentControls)
			{
				bool isSelected = object.Equals(ItemsView.SelectedItem, formsItemContentControl.FormsDataContext) || ItemsView.SelectedItems.Contains(formsItemContentControl.FormsDataContext);
				formsItemContentControl.UpdateIsSelected(isSelected);
			}
		}

		protected override void UpdateItemsLayout()
		{
			_ignorePlatformSelectionChange = true;

			base.UpdateItemsLayout();
			_ignorePlatformSelectionChange = false;
		}

		protected override void UpdateItemsSource()
		{
			_ignorePlatformSelectionChange = true;

			base.UpdateItemsSource();
			UpdatePlatformSelection();

			_ignorePlatformSelectionChange = false;
		}

		partial class SelectionModeConvert : Microsoft.UI.Xaml.Data.IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, string language)
			{
				var formSelectionMode = (SelectionMode)value;
				switch (formSelectionMode)
				{
					case SelectionMode.None:
						return WASDKListViewSelectionMode.None;
					case SelectionMode.Single:
						return WASDKListViewSelectionMode.Single;
					case SelectionMode.Multiple:
						return WASDKListViewSelectionMode.Multiple;
					default:
						return WASDKListViewSelectionMode.None;
				}
			}

			public object ConvertBack(object value, Type targetType, object parameter, string language)
			{
				var uwpListViewSelectionMode = (WASDKListViewSelectionMode)value;
				switch (uwpListViewSelectionMode)
				{
					case WASDKListViewSelectionMode.None:
						return SelectionMode.None;
					case WASDKListViewSelectionMode.Single:
						return SelectionMode.Single;
					case WASDKListViewSelectionMode.Multiple:
						return SelectionMode.Multiple;
					case WASDKListViewSelectionMode.Extended:
						return SelectionMode.None;
					default:
						return SelectionMode.None;
				}
			}
		}
	}
}
