using System;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages
{
	[Preserve(AllMembers = true)]
	public partial class PageWithTransparentBkgnd : ContentPage
	{
		public PageWithTransparentBkgnd()
		{
			InitializeComponent();
		}

		void ClosePageButtonClicked(object sender, EventArgs e)
		{
			Navigation.PopModalAsync();
		}
	}
}