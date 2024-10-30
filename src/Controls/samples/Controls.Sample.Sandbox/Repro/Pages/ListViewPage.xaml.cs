using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class ListViewPage : ContentPage
{
	public ListViewPage()
	{
		InitializeComponent();
		BindingContext = new ProductsViewModel();
	}
}