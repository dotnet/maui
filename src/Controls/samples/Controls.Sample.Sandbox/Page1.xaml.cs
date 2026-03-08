using Maui.Controls.Sample.Services;

namespace Maui.Controls.Sample;

public partial class Page1 : ContentPage
{
	private readonly NavigationService _navigationService;

	public Page1()
	{
		InitializeComponent();
		_navigationService = new NavigationService();

		Loaded += (e, __) =>
		{
			_ = MemoryTest.IsAliveAsync();
		};
	}

	private void OnNavigateToPage2Clicked(object? sender, EventArgs e)
	{
		_navigationService.NavigateAbsolute(typeof(Page2));
	}

	private void OnNavigateToPage2AsyncClicked(object? sender, EventArgs e)
	{
		 _navigationService.Navigate(typeof(Page2));
	}
}
