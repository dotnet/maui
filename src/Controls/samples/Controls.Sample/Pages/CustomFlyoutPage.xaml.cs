using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class CustomFlyoutPage
	{
		public CustomFlyoutPage(IServiceProvider services, MainViewModel viewModel) :
			base()
		{
			InitializeComponent();
			Detail = new CustomNavigationPage(services, viewModel);
			Detail.PropertyChanged += Detail_PropertyChanged;
			Flyout.BindingContext = viewModel;
		}

		private void Detail_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(NavigationPage.CurrentPage))
			{
				Flyout.BindingContext = (Detail as NavigationPage)!.CurrentPage.BindingContext;
			}
		}
	}
}
