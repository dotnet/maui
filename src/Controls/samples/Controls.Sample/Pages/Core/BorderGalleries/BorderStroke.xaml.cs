using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class BorderStroke : ContentPage
	{
		public BorderStroke()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			ContentHeightSlider.Value = 60;
		}
	}
}