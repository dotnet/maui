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

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	[System.Obsolete(Compatibility.Hosting.MauiAppBuilderExtensions.UseMapperInstead)]
	public class SelectableItemsViewRenderer<TItemsView, TViewController> : StructuredItemsViewRenderer<TItemsView, TViewController>
		where TItemsView : SelectableItemsView
		where TViewController : SelectableItemsViewController<TItemsView>
	{
		[Microsoft.Maui.Controls.Internals.Preserve(Conditional = true)]
		public SelectableItemsViewRenderer() { }

		protected override TViewController CreateController(TItemsView itemsView, ItemsViewLayout layout)
		{
			return new SelectableItemsViewController<TItemsView>(itemsView, layout) as TViewController;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs changedProperty)
		{
			base.OnElementPropertyChanged(sender, changedProperty);

			if (changedProperty.IsOneOf(SelectableItemsView.SelectedItemProperty, SelectableItemsView.SelectedItemsProperty))
			{
				UpdateNativeSelection();
			}
			else if (changedProperty.Is(SelectableItemsView.SelectionModeProperty))
			{
				UpdateSelectionMode();
			}
		}

		protected override void SetUpNewElement(TItemsView newElement)
		{
			base.SetUpNewElement(newElement);

			if (newElement == null)
			{
				return;
			}

			UpdateSelectionMode();
			UpdateNativeSelection();
		}

		protected virtual void UpdateNativeSelection()
		{
			Controller.UpdateNativeSelection();
		}

		protected virtual void UpdateSelectionMode()
		{
			Controller.UpdateSelectionMode();
		}

		protected override void UpdateItemsSource()
		{
			base.UpdateItemsSource();
			UpdateNativeSelection();
		}
	}
}