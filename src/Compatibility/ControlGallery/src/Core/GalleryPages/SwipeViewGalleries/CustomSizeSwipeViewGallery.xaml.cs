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
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class CustomSizeSwipeViewGallery : ContentPage
	{
		public CustomSizeSwipeViewGallery()
		{
			InitializeComponent();
		}

		void OnContentClicked(object sender, EventArgs args)
		{
			DisplayAlert("OnClicked", "The Content Button has been clicked.", "Ok");
		}

		void OnRightItemsClicked(object sender, EventArgs args)
		{
			DisplayAlert("OnClicked", "The RightItems Button has been clicked.", "Ok");
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			DisplayAlert("Custom SwipeItem", "Button Clicked!", "Ok");
		}
	}

}