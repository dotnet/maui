using System;
using System.ComponentModel;
using Android.Content;

namespace Xamarin.Forms.Platform.Android
{
	public class GroupableItemsViewRenderer<TItemsView, TAdapter, TItemsViewSource> : SelectableItemsViewRenderer<TItemsView, TAdapter, TItemsViewSource>
		where TItemsView : GroupableItemsView
		where TAdapter : GroupableItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsViewSource : IGroupableItemsViewSource
	{
		public GroupableItemsViewRenderer(Context context) : base(context)
		{
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

		protected override TAdapter CreateAdapter()
		{
			return (TAdapter)new GroupableItemsViewAdapter<TItemsView, TItemsViewSource>(ItemsView);
		}
	}
}