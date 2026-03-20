using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample;

public partial class Page2 : ContentPage
{
	private readonly NavigationService _navigationService;

	public Page2()
	{
		InitializeComponent();
		_navigationService = new NavigationService();
	}

	private void OnNavigateToPage1Clicked(object? sender, EventArgs e)
	{
		 _navigationService.Navigate(typeof(Page1));
	}
}
