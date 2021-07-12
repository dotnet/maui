using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class SwipeViewPage
	{
		public SwipeViewPage()
		{
			InitializeComponent();
		}

		async void OnFavoriteSwipeItemInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("SwipeView", "Favorite invoked.", "OK");
		}

		async void OnDeleteSwipeItemInvoked(object sender, EventArgs e)
		{
			await DisplayAlert("SwipeView", "Delete invoked.", "OK");
		}
	}
}