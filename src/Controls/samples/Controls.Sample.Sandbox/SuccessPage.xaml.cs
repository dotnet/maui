using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class SuccessPage : ContentPage
	{
		public SuccessPage()
		{
			InitializeComponent();
		}

		async private void Button_Clicked(object sender, System.EventArgs e)
		{
			await Navigation.PopAsync();
        }
    }
}