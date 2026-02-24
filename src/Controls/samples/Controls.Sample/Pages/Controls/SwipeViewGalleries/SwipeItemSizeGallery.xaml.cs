using System;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	public partial class SwipeItemSizeGallery
	{
		public SwipeItemSizeGallery()
		{
			InitializeComponent();
		}

		void OnSwipeItemInvoked(object sender, EventArgs e)
		{
			DisplayAlertAsync("SwipeItemSizeGallery", "Delete SwipeItem Invoked", "Ok");
		}
	}
}