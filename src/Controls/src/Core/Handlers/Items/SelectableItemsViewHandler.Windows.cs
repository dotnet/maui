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
			}

			UpdatePlatformSelection();
		}

		protected override void DisconnectHandler(ListViewBase platformView)
		{
			var oldListViewBase = platformView;

			if (oldListViewBase != null)
			{
				oldListViewBase.SelectionChanged -= PlatformSelectionChanged;
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
										return itemPair.Item == ItemsView.SelectedItem;
									}
									else
									{
										return item == ItemsView.SelectedItem;
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

			UpdateItemContentControlSelection();
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
			UpdateVirtualSelection();
		}

		void UpdateVirtualSelection()
		{
			if (_ignorePlatformSelectionChange || ItemsView == null)
			{
				return;
			}

			switch (ListViewBase.SelectionMode)
			{
				case WASDKListViewSelectionMode.None:
					break;
				case WASDKListViewSelectionMode.Single:
					UpdateVirtualSingleSelection();
					break;
				case WASDKListViewSelectionMode.Multiple:
					UpdateVirtualMultipleSelection();
					break;
				default:
					break;
			}

			UpdateItemContentControlSelection();
		}

		void UpdateVirtualSingleSelection()
		{
			var selectedItem = ListViewBase.SelectedItem is ItemTemplateContext itemPair
				? itemPair.Item
				: ListViewBase.SelectedItem;

			if (ItemsView != null)
			{
				_ignoreVirtualSelectionChange = true;
				ItemsView.SelectedItem = selectedItem;

				_ignoreVirtualSelectionChange = false;
			}
		}

		void UpdateVirtualMultipleSelection()
		{
			ItemsView.SelectionChanged -= VirtualSelectionChanged;

			var selection = new List<object>();
			for (int n = 0; n < ListViewBase.SelectedItems.Count; n++)
			{
				var item = ListViewBase.SelectedItems[n];
				selection.Add(item is ItemTemplateContext itc ? itc.Item : item);
			}

			ItemsView.UpdateSelectedItems(selection);

			ItemsView.SelectionChanged += VirtualSelectionChanged;
		}

		void UpdateItemContentControlSelection()
		{
			var formsItemContentControls = ListViewBase.GetChildren<ItemContentControl>();

			foreach (var formsItemContentControl in formsItemContentControls)
			{
				bool isSelected = ItemsView.SelectedItem == formsItemContentControl.FormsDataContext || ItemsView.SelectedItems.Contains(formsItemContentControl.FormsDataContext);
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
