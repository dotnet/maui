using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.RefreshViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class RefreshListViewGallery : ContentPage
	{
		public RefreshListViewGallery()
		{
			InitializeComponent();
			BindingContext = new RefreshViewModel();
		}
	}
}
