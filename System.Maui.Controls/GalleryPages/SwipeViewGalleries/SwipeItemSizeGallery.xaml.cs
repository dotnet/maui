using System;

namespace System.Maui.Controls.GalleryPages.SwipeViewGalleries
{
	public partial class SwipeItemSizeGallery : ContentPage
	{
		public SwipeItemSizeGallery()
		{
			InitializeComponent();
		}

		void OnSwipeItemInvoked(object sender, EventArgs e)
		{
			DisplayAlert("SwipeItemSizeGallery", "Delete SwipeItem Invoked", "Ok");
		}
	}
}