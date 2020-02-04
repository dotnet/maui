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