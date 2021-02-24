using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.UI.Xaml.Controls;
using UWPListViewSelectionMode = Microsoft.UI.Xaml.Controls.ListViewSelectionMode;
using UWPSelectionChangedEventArgs = Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{

	public class SelectableItemsViewRenderer<TItemsView> : StructuredItemsViewRenderer<TItemsView>
		where TItemsView : SelectableItemsView
	{
		bool _ignoreNativeSelectionChange;

		protected override void TearDownOldElement(ItemsView oldElement)
		{
			var oldListViewBase = ListViewBase;
			if (oldListViewBase != null)
			{
				oldListViewBase.ClearValue(ListViewBase.SelectionModeProperty);
				oldListViewBase.SelectionChanged -= NativeSelectionChanged;
			}

			if (ItemsView != null)
			{
				ItemsView.SelectionChanged -= FormsSelectionChanged;
			}

			base.TearDownOldElement(oldElement);
		}

		protected override void SetUpNewElement(ItemsView newElement)
		{
			base.SetUpNewElement(newElement);

			if (newElement == null)
			{
				return;
			}

			if (ItemsView != null)
			{
				ItemsView.SelectionChanged += FormsSelectionChanged;
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

				newListViewBase.SelectionChanged += NativeSelectionChanged;
			}

			UpdateNativeSelection();
		}

		protected override void UpdateItemsSource()
		{
			_ignoreNativeSelectionChange = true;

			base.UpdateItemsSource();
			UpdateNativeSelection();

			_ignoreNativeSelectionChange = false;
		}

		void UpdateNativeSelection()
		{
			_ignoreNativeSelectionChange = true;

			switch (ListViewBase.SelectionMode)
			{
				case UWPListViewSelectionMode.None:
					break;
				case UWPListViewSelectionMode.Single:
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
				case UWPListViewSelectionMode.Multiple:
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
				case UWPListViewSelectionMode.Extended:
					break;
				default:
					break;
			}

			_ignoreNativeSelectionChange = false;
		}

		void FormsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			UpdateNativeSelection();
		}

		void NativeSelectionChanged(object sender, UWPSelectionChangedEventArgs args)
		{
			UpdateFormsSelection();
		}
			
		void UpdateFormsSelection()
		{
			if (_ignoreNativeSelectionChange || ItemsView == null)
			{
				return;
			}

			switch (ListViewBase.SelectionMode)
			{
				case UWPListViewSelectionMode.None:
					break;
				case UWPListViewSelectionMode.Single:
					UpdateFormsSingleSelection();
					break;
				case UWPListViewSelectionMode.Multiple:
					UpdateFormsMultipleSelection();
					break;
				default:
					break;
			}

			var formsItemContentControls = ListViewBase.GetChildren<ItemContentControl>();

			foreach (var formsItemContentControl in formsItemContentControls)
			{
				bool isSelected = ItemsView.SelectedItem == formsItemContentControl.FormsDataContext || ItemsView.SelectedItems.Contains(formsItemContentControl.FormsDataContext);
				formsItemContentControl.UpdateIsSelected(isSelected);
			}
		}

		void UpdateFormsSingleSelection()
		{
			var selectedItem = ListViewBase.SelectedItem is ItemTemplateContext itemPair
				? itemPair.Item
				: ListViewBase.SelectedItem;

			if (ItemsView != null)
			{
				ItemsView.SelectionChanged -= FormsSelectionChanged;
				ItemsView.SelectedItem = selectedItem;

				ItemsView.SelectionChanged += FormsSelectionChanged;
			}
		}

		void UpdateFormsMultipleSelection()
		{
			ItemsView.SelectionChanged -= FormsSelectionChanged;

			var selection = new List<object>();
			for (int n = 0; n < ListViewBase.SelectedItems.Count; n++)
			{
				var item = ListViewBase.SelectedItems[n];
				selection.Add(item is ItemTemplateContext itc ? itc.Item : item);
			}

			ItemsView.UpdateSelectedItems(selection);

			ItemsView.SelectionChanged += FormsSelectionChanged;
		}

		class SelectionModeConvert : Microsoft.UI.Xaml.Data.IValueConverter
		{
			public object Convert(object value, Type targetType, object parameter, string language)
			{
				var formSelectionMode = (SelectionMode)value;
				switch (formSelectionMode)
				{
					case SelectionMode.None:
						return UWPListViewSelectionMode.None;
					case SelectionMode.Single:
						return UWPListViewSelectionMode.Single;
					case SelectionMode.Multiple:
						return UWPListViewSelectionMode.Multiple;
					default:
						return UWPListViewSelectionMode.None;
				}
			}

			public object ConvertBack(object value, Type targetType, object parameter, string language)
			{
				var uwpListViewSelectionMode = (UWPListViewSelectionMode)value;
				switch (uwpListViewSelectionMode)
				{
					case UWPListViewSelectionMode.None:
						return SelectionMode.None;
					case UWPListViewSelectionMode.Single:
						return SelectionMode.Single;
					case UWPListViewSelectionMode.Multiple:
						return SelectionMode.Multiple;
					case UWPListViewSelectionMode.Extended:
						return SelectionMode.None;
					default:
						return SelectionMode.None;
				}
			}
		}
	}
}
