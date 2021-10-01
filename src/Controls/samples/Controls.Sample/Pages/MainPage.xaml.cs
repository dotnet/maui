using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class MainPage
	{
		public MainPage(IServiceProvider services, MainViewModel viewModel)
		{
			InitializeComponent();

			BindingContext = viewModel;

			Services = services;
			ViewModel = viewModel;
		}

		IServiceProvider Services { get; }
		MainViewModel ViewModel { get; }

		void Button_Clicked(System.Object sender, System.EventArgs e)
		{
			XamlApp.Instance.OpenWindow(new Window(new MainPage(Services, ViewModel)));
		}
	}
}