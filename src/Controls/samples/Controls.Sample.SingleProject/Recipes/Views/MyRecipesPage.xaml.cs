using Microsoft.Maui.Controls;
using Recipes.ViewModels;

namespace Recipes.Views
{
	public partial class MyRecipesPage : ContentPage
	{
		ItemsViewModel _viewModel;

		public MyRecipesPage()
		{
			InitializeComponent();
			BindingContext = _viewModel = new ItemsViewModel();

			MessagingCenter.Subscribe<ItemsViewModel>(this, "RemoveRecipeFromVirtualListView", (sender) =>
			{
				vMyRecipesListView.InvalidateData();
			});
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			_viewModel.OnAppearing();
		}

		private void MyRecipesPage_Tapped(object sender, System.EventArgs e)
		{
			BindableObject bo = sender as BindableObject;
			_viewModel.ItemTapped.Execute(bo.BindingContext);

		}
	}
}