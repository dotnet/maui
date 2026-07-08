using System;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class BasicSwipeGallery
	{
		public BasicSwipeGallery()
		{
			InitializeComponent();
		}

		async void OnInvoked(object sender, EventArgs e)
		{
			await DisplayAlertAsync("SwipeView", "Delete Invoked", "OK");
		}
	}
}