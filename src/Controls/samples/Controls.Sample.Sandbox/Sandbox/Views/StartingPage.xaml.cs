using Recipes.ViewModels;

namespace Recipes.Views
{
    public partial class StartingPage : ContentPage
    {
        StartingPageViewModel _viewModel;

        public StartingPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new StartingPageViewModel();
        }
    }
}