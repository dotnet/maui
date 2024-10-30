using AllTheLists.ViewModels;

namespace AllTheLists.Pages;

public partial class ReviewsPage : ContentPage
{
	public ReviewsPage()
	{
		InitializeComponent();
		BindingContext = new ReviewsViewModel();
	}
}