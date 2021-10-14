using Microsoft.Maui.Controls;
using Recipes.ViewModels;

namespace Recipes.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        ItemDetailViewModel _viewModel;

        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new ItemDetailViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.OnAppearing();
		}

		async void OpenUrl(object sender, System.EventArgs e)
		{
			await Microsoft.Maui.Essentials.Launcher.OpenAsync(_viewModel.RecipeUrl);

		}
	}
}