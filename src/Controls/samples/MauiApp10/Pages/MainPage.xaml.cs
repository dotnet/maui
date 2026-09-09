using MauiApp10.Models;
using MauiApp10.PageModels;

namespace MauiApp10.Pages
{
    public partial class MainPage : ContentPage
    {
        public MainPage(MainPageModel model)
        {
            InitializeComponent();
            BindingContext = model;
        }
    }
}