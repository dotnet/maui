using System.ComponentModel;

namespace Xamarin.Forms.Platform.iOS
{
	public class GroupableItemsViewRenderer : SelectableItemsViewRenderer
	{
		GroupableItemsView GroupableItemsView => (GroupableItemsView)Element;
		GroupableItemsViewController GroupableItemsViewController => (GroupableItemsViewController)ItemsViewController;

		protected override ItemsViewController CreateController(ItemsView itemsView, ItemsViewLayout layout)
		{
			return new GroupableItemsViewController(itemsView as GroupableItemsView, layout);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(GroupableItemsView.IsGroupedProperty))
			{
				GroupableItemsViewController?.UpdateItemsSource();
			}
		}
	}
}