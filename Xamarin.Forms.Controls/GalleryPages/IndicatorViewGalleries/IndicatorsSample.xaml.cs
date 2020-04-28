using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Controls
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