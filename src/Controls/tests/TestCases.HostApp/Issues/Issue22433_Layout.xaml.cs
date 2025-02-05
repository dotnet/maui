namespace Maui.Controls.Sample.Issues;

public partial class Issue22433_Layout : ContentPage
{
	public Issue22433_Layout()
	{
		InitializeComponent();
	}

	async void NavBackButton_Clicked(object sender, System.EventArgs e)
	{
		await Navigation.PopAsync();
	}
}