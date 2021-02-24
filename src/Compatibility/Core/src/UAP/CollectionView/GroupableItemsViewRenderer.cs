using System.ComponentModel;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class GroupableItemsViewRenderer<TItemsView> : SelectableItemsViewRenderer<TItemsView>
		where TItemsView : GroupableItemsView
	{
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.IsOneOf(GroupableItemsView.IsGroupedProperty,
				GroupableItemsView.GroupFooterTemplateProperty, GroupableItemsView.GroupHeaderTemplateProperty))
			{
				UpdateItemsSource();
			}
		}

		protected override CollectionViewSource CreateCollectionViewSource()
		{
			if (ItemsView != null && ItemsView.IsGrouped)
			{
				var itemTemplate = Element.ItemTemplate;
				var itemsSource = Element.ItemsSource;

				return new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory.CreateGrouped(itemsSource, itemTemplate,
					ItemsView.GroupHeaderTemplate, ItemsView.GroupFooterTemplate, Element),
					IsSourceGrouped = true,
					ItemsPath = new Microsoft.UI.Xaml.PropertyPath(nameof(GroupTemplateContext.Items))
				};
			}
			else
			{
				return base.CreateCollectionViewSource();
			}
		}

		protected override void UpdateItemTemplate()
		{
			base.UpdateItemTemplate();

			ListViewBase.GroupStyleSelector = new GroupHeaderStyleSelector();
		}
	}
}
