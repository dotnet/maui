
using System.Threading.Tasks;
using Microsoft.Maui.Controls;

namespace MauiApp2;

public partial class MainPage : ContentPage
{
	public MainPage(MainPageModel model)
	{
		InitializeComponent();

		BindingContext = model;
	}

	private async void CounterBtn_Clicked(object sender, System.EventArgs e)
	{
		await Shell.Current.GoToAsync(nameof(Child));
	}
}


public partial class MainPageModel
{
	public async Task Navigate()
	{
		await Shell.Current.GoToAsync(nameof(Child));
	}
}
