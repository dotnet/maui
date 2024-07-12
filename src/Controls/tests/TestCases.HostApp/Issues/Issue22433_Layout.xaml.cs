using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
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