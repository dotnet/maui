using System.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace Xamarin.Forms.Platform.UWP
{
	public class GroupableItemsViewRenderer : SelectableItemsViewRenderer
	{
		GroupableItemsView _groupableItemsView;

		protected override void SetUpNewElement(ItemsView newElement)
		{
			_groupableItemsView = Element as GroupableItemsView;
			base.SetUpNewElement(newElement);
		}

		protected override void TearDownOldElement(ItemsView oldElement)
		{
			base.TearDownOldElement(oldElement);
			_groupableItemsView = null;
		}

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
			if (_groupableItemsView != null && _groupableItemsView.IsGrouped)
			{
				var itemTemplate = Element.ItemTemplate;
				var itemsSource = Element.ItemsSource;

				return new CollectionViewSource
				{
					Source = TemplatedItemSourceFactory.CreateGrouped(itemsSource, itemTemplate,
					_groupableItemsView.GroupHeaderTemplate, _groupableItemsView.GroupFooterTemplate, Element),
					IsSourceGrouped = true,
					ItemsPath = new Windows.UI.Xaml.PropertyPath(nameof(GroupTemplateContext.Items))
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
