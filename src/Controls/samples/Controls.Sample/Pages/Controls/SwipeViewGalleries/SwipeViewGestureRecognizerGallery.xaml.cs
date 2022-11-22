using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class SwipeViewGestureRecognizerGallery
	{
		public SwipeViewGestureRecognizerGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "favourite", sender => { DisplayAlert("SwipeView", "Favourite", "Ok"); });
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "delete", sender => { DisplayAlert("SwipeView", "Delete", "Ok"); });
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "tap", sender => { DisplayAlert("SwipeView", "TapGestureRecognizer", "Ok"); });
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}
}