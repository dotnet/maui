using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
			var mvm = Services.GetService<MainViewModel>();
;			XamlApp.Instance.OpenWindow(new Window(new MainPage(Services, mvm)));
		}
	}
}