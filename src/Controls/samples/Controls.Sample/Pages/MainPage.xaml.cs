using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

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
			Application.Current.OpenWindow(new Window(new CustomNavigationPage(_services, mvm)));
		}

		void TestVisualTreeHelper(object sender, EventArgs e)
		{
			((IWindow)this.Parent.Parent).VisualDiagnosticsLayer.AddAdorner(TestLabel as IVisualTreeElement);
		}
	}
}