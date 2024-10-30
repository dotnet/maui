using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class CheckInsPage : ContentPage
{
	public CheckInsPage()
	{
		InitializeComponent();
		BindingContext = new CheckInsViewModel();
	}
}