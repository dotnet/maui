using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class VirtualizedListViewPage : ContentPage
{
	public VirtualizedListViewPage()
	{
		InitializeComponent();
		BindingContext = new ProductsViewModel();
	}
}