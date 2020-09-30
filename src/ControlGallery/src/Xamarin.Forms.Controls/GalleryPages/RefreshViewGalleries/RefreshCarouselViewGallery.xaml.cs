using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshCarouselViewGallery : ContentPage
	{
		public RefreshCarouselViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}