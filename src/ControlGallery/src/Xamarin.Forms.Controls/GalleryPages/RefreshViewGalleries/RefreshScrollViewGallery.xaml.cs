using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshScrollViewGallery : ContentPage
	{
		public RefreshScrollViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel(true);
		}
	}
}