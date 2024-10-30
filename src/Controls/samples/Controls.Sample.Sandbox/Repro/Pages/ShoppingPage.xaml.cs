using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class ShoppingPage : ContentPage
{
	public ShoppingPage()
	{
		InitializeComponent();
		BindingContext = new ProductDisplaysViewModel();
	}

	private void CollectionView_RemainingItemsThresholdReached(object sender, EventArgs e)
	{
		((ProductDisplaysViewModel)BindingContext).ThresholdReachedCommand.Execute(null);
		// await Task.Delay(100);
		//((CollectionView)sender).ScrollTo(((ProductDisplaysViewModel)BindingContext).VisibleProducts.Last(), position: ScrollToPosition.MakeVisible, animate: false);
		
	}
}