using Xamarin.Forms.Platform.Tizen.Native;

namespace Xamarin.Forms.Platform.Tizen
{
	public class StructuredItemsViewRenderer : ItemsViewRenderer<StructuredItemsView, Native.CollectionView>
	{
		public StructuredItemsViewRenderer()
		{
			RegisterPropertyHandler(StructuredItemsView.ItemsLayoutProperty, UpdateItemsLayout);
			RegisterPropertyHandler(SelectableItemsView.SelectedItemProperty, UpdateSelectedItem);
			RegisterPropertyHandler(SelectableItemsView.SelectionModeProperty, UpdateSelectionMode);
			RegisterPropertyHandler(StructuredItemsView.ItemSizingStrategyProperty, UpdateSizingStrategy);
		}

		protected override Native.CollectionView CreateNativeControl(ElmSharp.EvasObject parent)
		{
			return new Native.CollectionView(parent);
		}

		protected override IItemsLayout GetItemsLayout()
		{
			return Element.ItemsLayout;
		}

		protected override void OnElementChanged(ElementChangedEventArgs<StructuredItemsView> e)
		{
			base.OnElementChanged(e);
		}

		protected override void OnItemSelectedFromUI(object sender, SelectedItemChangedEventArgs e)
		{
			if (Element is SelectableItemsView selectableItemsView)
			{
				selectableItemsView.SelectedItem = e.SelectedItem;
			}
		}

		protected void UpdateSizingStrategy(bool initialize)
		{
			if (initialize)
			{
				return;
			}
			UpdateItemsLayout();
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
	}
}
