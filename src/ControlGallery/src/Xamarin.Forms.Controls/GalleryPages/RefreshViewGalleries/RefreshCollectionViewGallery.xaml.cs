using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshCollectionViewGallery : ContentPage
	{
		public RefreshCollectionViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}