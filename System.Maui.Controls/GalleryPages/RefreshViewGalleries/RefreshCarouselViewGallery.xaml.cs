using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.RefreshViewGalleries
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