using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class WhoIsWatchingPage : ContentPage
{
	public WhoIsWatchingPage()
	{
		InitializeComponent();
		BindingContext = new StreamingServiceViewModel();
	}
}