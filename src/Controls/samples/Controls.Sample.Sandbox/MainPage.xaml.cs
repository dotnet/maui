
using System.Threading.Tasks;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
		btn.Clicked += BtnClicked;
	}

	private async void BtnClicked(object? sender, EventArgs e)
	{
		await Navigation.PushModalAsync(new MainPage());
	}
}