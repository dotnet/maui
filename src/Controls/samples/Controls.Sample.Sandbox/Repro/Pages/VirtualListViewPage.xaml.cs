using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class VirtualListViewPage : ContentPage
{
	public VirtualListViewPage()
	{
		InitializeComponent();
		BindingContext = new ProductsViewModel();
	}
}