using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.RefreshViewGalleries
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