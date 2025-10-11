namespace Maui.Controls.Sample.Issues
{
	public partial class Issue27101SecondPage : ContentPage
	{
		public Issue27101SecondPage()
		{
			InitializeComponent();
		}

		async void OnNavigateBackButtonClicked(object sender, EventArgs e)
		{
			await Navigation.PopAsync();
		}
	}
}