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

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.RefreshViewGalleries
{
	public partial class RefreshLayoutMarginGallery : ContentPage
	{
		public RefreshLayoutMarginGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}

		void OnMarginValueChanged(object sender, ValueChangedEventArgs e)
		{
			var margin = e.NewValue;
			RefreshScroll.Margin = new Thickness(margin);
		}
	}
}
