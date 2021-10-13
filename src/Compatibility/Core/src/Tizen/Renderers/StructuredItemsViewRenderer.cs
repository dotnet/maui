using System.Linq;
using TCollectionView = Tizen.UIExtensions.NUI.CollectionView;
using TCollectionViewSelectionMode = Tizen.UIExtensions.NUI.CollectionViewSelectionMode;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Tizen
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class StructuredItemsViewRenderer : ItemsViewRenderer<StructuredItemsView, TCollectionView>
	{
		public StructuredItemsViewRenderer()
		{
			RegisterPropertyHandler(StructuredItemsView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(StructuredItemsView.ItemSizingStrategyProperty, UpdateSizingStrategy);
			RegisterPropertyHandler(StructuredItemsView.HeaderProperty, UpdateHeaderFooter);
			RegisterPropertyHandler(StructuredItemsView.HeaderTemplateProperty, UpdateHeaderFooter);
			RegisterPropertyHandler(StructuredItemsView.FooterProperty, UpdateHeaderFooter);
			RegisterPropertyHandler(StructuredItemsView.FooterTemplateProperty, UpdateHeaderFooter);
			RegisterPropertyHandler(SelectableItemsView.SelectedItemProperty, UpdateSelectedItem);
			RegisterPropertyHandler(SelectableItemsView.SelectionModeProperty, UpdateSelectionMode);
		}

		protected override TCollectionView CreateNativeControl()
		{
			return new TCollectionView();
		}

		protected override IItemsLayout GetItemsLayout()
		{
			return Element.ItemsLayout;
		}

		protected void UpdateSizingStrategy(bool initialize)
		{
			if (initialize)
			{
				return;
			}
			UpdateItemsLayout();
		}

		protected override void OnItemSelectedFromUI(object sender, CollectionViewSelectionChangedEventArgs e)
		{
			if (Element is SelectableItemsView selectableItemsView)
			{
				if (selectableItemsView.SelectionMode == SelectionMode.Single)
				{
					selectableItemsView.SelectedItem = e.SelectedItems.FirstOrDefault();
				}
				else if (selectableItemsView.SelectionMode == SelectionMode.Multiple)
				{
					selectableItemsView.SelectedItems = e.SelectedItems;
				}
			}
		}

		void UpdateHeaderFooter(bool init)
		{
			if (!init)
			{
				UpdateAdaptor(false);
			}
		}

		void UpdateSelectedItem(bool initialize)
		{
			if (initialize)
				return;

			if (Control == null || Control.Adaptor == null)
				return;

			if (Element is SelectableItemsView selectable && selectable.SelectionMode == SelectionMode.Single)
			{
				if (selectable.SelectedItem == null)
				{
					foreach (var index in Control.SelectedItems.ToList())
					{
						Control.RequestItemUnselect(index);
					}
				}
				else
				{
					var index = Control.Adaptor.GetItemIndex(selectable.SelectedItem);
					Control.RequestItemSelect(index);
				}
			}
		}

		void UpdateSelectionMode()
		{
			if (Element is SelectableItemsView selectable)
			{
				Control.SelectionMode = (TCollectionViewSelectionMode)selectable.SelectionMode;
			}
		}
	}
}
