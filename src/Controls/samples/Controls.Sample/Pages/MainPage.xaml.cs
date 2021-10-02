using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Maui.Controls.Sample.Pages
{
	public partial class MainPage
	{
		IServiceProvider _services;
		MainViewModel _viewModel;

		public MainPage(IServiceProvider services, MainViewModel viewModel)
		{
			InitializeComponent();

			BindingContext = viewModel;

			_services = services;
			_viewModel = viewModel;
		}

		void ShowNewWindow(object sender, EventArgs e)
		{
			var mvm = _services.GetService<MainViewModel>();
			Application.Current.OpenWindow(new Window(new MainPage(_services, mvm)));
		}
	}
}