//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.ComponentModel;
using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
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