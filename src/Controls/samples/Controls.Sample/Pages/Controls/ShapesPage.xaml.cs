using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class ShapesPage
	{
		public ShapesPage()
		{
			InitializeComponent();
		}

		void OnMoreSamplesClicked(object? sender, EventArgs args)
		{
			Navigation.PushAsync(new Pages.ShapesGalleries.ShapesGallery());
		}
	}
}