using System;
using Microsoft.Maui.Controls;
using Maui.Controls.Sample.ViewModels;

namespace Maui.Controls.Sample.Pages
{
    public partial class CustomNavigationPage : NavigationPage
    {
		public CustomNavigationPage(IServiceProvider services, MainViewModel viewModel) :
            base(new MainPage(services, viewModel))
		{
            InitializeComponent();
        }
    }
}