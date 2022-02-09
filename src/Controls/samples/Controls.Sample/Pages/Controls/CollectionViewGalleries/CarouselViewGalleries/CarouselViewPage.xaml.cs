using System;

namespace Maui.Controls.Sample.Pages
{
	public partial class CarouselViewPage
	{
		public CarouselViewPage()
		{
			InitializeComponent();
		}

		async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
		{
			await DisplayAlert("Item", "Tapped", "Successfully");
		}
	}
}