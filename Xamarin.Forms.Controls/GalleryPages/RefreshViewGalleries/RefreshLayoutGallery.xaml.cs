using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshLayoutGallery : ContentPage
	{
		public RefreshLayoutGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}