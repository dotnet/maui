using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages
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