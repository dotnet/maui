using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages.RefreshViewGalleries
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