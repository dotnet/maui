using System;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class BasicSwipeGallery : ContentPage
	{
		public BasicSwipeGallery()
		{
			InitializeComponent();
		}

		private void OnInvoked(object sender, EventArgs e)
		{
			DisplayAlert("SwipeView", "Delete Invoked", "OK");
		}
	}
}