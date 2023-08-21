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

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.RefreshViewGalleries
{
	public partial class IsEnabledRefreshViewGallery : ContentPage
	{
		public IsEnabledRefreshViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}

		void IsEnabledBtnClicked(object sender, EventArgs e)
		{
			var button = (Button)sender;

			if (RefreshContainer.IsEnabled)
			{
				button.Text = "Enable RefreshView";
				RefreshContainer.IsEnabled = false;
				Title = "Is disabled";
			}
			else
			{
				button.Text = "Disable RefreshView";
				RefreshContainer.IsEnabled = true;
				Title = "Is enabled";
			}
		}
	}
}
