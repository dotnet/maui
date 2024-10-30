using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class CollectionViewPage : ContentPage
{
	public CollectionViewPage()
	{
		InitializeComponent();
		BindingContext = new ProductsViewModel();
	}
}