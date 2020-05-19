using System;
using System.Maui.Internals;

namespace System.Maui.Controls.GalleryPages
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