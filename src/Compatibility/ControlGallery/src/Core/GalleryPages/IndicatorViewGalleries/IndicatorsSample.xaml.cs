using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class IndicatorsSample : ContentPage
	{
		public IndicatorsSample()
		{
			InitializeComponent();
			BindingContext = new GalleryPages.CollectionViewGalleries.CarouselViewGalleries.CarouselItemsGalleryViewModel(false, false);
		}
	}
}