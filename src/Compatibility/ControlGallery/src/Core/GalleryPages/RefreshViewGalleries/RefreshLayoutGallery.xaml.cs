using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.RefreshViewGalleries
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