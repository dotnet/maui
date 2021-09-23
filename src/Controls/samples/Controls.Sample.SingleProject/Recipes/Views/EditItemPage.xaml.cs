using Microsoft.Maui.Controls;

using Recipes.ViewModels;

namespace Recipes.Views
{
    public partial class EditItemPage : ContentPage
    {
        EditItemViewModel _viewModel;

        public EditItemPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new EditItemViewModel();
        }
    }
}