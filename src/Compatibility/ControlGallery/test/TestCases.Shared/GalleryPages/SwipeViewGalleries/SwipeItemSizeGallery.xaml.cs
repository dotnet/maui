using System;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
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