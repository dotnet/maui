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

using System.ComponentModel;
using Android.Content;

namespace Microsoft.Maui.Controls.Compatibility.Platform.Android
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class StructuredItemsViewRenderer<TItemsView, TAdapter, TItemsViewSource> : ItemsViewRenderer<TItemsView, TAdapter, TItemsViewSource>
		where TItemsView : StructuredItemsView
		where TAdapter : StructuredItemsViewAdapter<TItemsView, TItemsViewSource>
		where TItemsViewSource : IItemsViewSource
	{
		StructuredItemsView _itemsView;

		public StructuredItemsViewRenderer(Context context) : base(context)
		{
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.Is(StructuredItemsView.ItemsLayoutProperty))
			{
				UpdateLayoutManager();
			}
			else if (changedProperty.Is(StructuredItemsView.ItemSizingStrategyProperty))
			{
				UpdateAdapter();
			}
		}

		protected override TAdapter CreateAdapter()
		{
			return (TAdapter)new StructuredItemsViewAdapter<TItemsView, TItemsViewSource>(ItemsView);
		}

		protected override void SetUpNewElement(TItemsView newElement)
		{
			_itemsView = newElement;

			base.SetUpNewElement(newElement);
		}

		protected override IItemsLayout GetItemsLayout()
		{
			return _itemsView.ItemsLayout;
		}
	}
}