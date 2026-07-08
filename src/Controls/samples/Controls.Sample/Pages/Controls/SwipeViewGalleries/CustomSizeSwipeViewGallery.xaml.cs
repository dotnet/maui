using System;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class CustomSizeSwipeViewGallery
	{
		public CustomSizeSwipeViewGallery()
		{
			InitializeComponent();
		}

		void OnContentClicked(object sender, EventArgs args)
		{
			DisplayAlertAsync("OnClicked", "The Content Button has been clicked.", "Ok");
		}

		void OnRightItemsClicked(object sender, EventArgs args)
		{
			DisplayAlertAsync("OnClicked", "The RightItems Button has been clicked.", "Ok");
		}

		void OnButtonClicked(object sender, EventArgs e)
		{
			DisplayAlertAsync("Custom SwipeItem", "Button Clicked!", "Ok");
		}
	}

}