using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class BorderLayout : ContentPage
	{
		public BorderLayout()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			BorderWidthSlider.Value = 5;
		}
	}
}