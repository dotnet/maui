using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class StreamingServicePage : ContentPage
{
	public StreamingServicePage()
	{
		InitializeComponent();
		BindingContext = new StreamingServiceViewModel();
	}
}