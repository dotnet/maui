using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.RefreshViewGalleries
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