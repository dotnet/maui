using System.Threading.Tasks;

namespace Maui.Controls.Sample;

public partial class SettingsPage : ContentPage
{
	public SettingsPage()
	{
		InitializeComponent();
	}

	async void Button_Clicked(object sender, EventArgs e)
	{
		await Navigation.PopToRootAsync();
	}
}

