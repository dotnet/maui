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
			//_ = MemoryTest.IsAliveAsync();
		};
	}

	private async void OnNavigateToPage2Clicked(object? sender, EventArgs e)
	{
		for(var i =0; i< 100; i++)
		{
			_navigationService.NavigateAbsolute(typeof(Page2));
			await Task.Delay(100);
			_navigationService.Navigate(typeof(Page1));
		}

		//_navigationService.NavigateAbsolute(typeof(Page2));

		MemoryTest.RunGC();
	}

	async void OnNavigateToPage2AsyncClicked(object? sender, EventArgs e)
	{
		for(var i=0; i < 100; i++)
		{
			_navigationService.Navigate(typeof(Page2));
			await Task.Delay(100);
			_navigationService.Navigate(typeof(Page1));
		}
		MemoryTest.RunGC();
	}
}
