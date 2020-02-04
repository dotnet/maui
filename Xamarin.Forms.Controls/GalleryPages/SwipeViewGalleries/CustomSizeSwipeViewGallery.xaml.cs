using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class CustomSizeSwipeViewGallery : ContentPage
	{
		public CustomSizeSwipeViewGallery()
		{
			InitializeComponent();
		}

        void OnButtonClicked(object sender, EventArgs e)
        {
			DisplayAlert("Custom SwipeItem", "Button Clicked!", "Ok");
        }
   	}
}