using CommunityToolkit.Mvvm.Messaging;
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

			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "favourite", (_, sender) => { DisplayAlertAsync("SwipeView", "Favourite", "Ok"); });
			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "delete", (_, sender) => { DisplayAlertAsync("SwipeView", "Delete", "Ok"); });
			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "tap", (_, sender) => { DisplayAlertAsync("SwipeView", "TapGestureRecognizer", "Ok"); });
		}
	}
}