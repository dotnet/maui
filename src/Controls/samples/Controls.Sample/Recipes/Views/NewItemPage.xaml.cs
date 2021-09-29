using Microsoft.Maui.Controls;
using Recipes.Models;
using Recipes.ViewModels;

namespace Recipes.Views
{
    public partial class NewItemPage : ContentPage
    {
        public Item Item { get; set; }

        public NewItemPage()
        {
            InitializeComponent();
            BindingContext = new NewItemViewModel();
        }
    }
}