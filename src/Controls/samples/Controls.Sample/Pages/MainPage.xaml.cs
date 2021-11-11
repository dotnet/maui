using System;
using System.Diagnostics;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Maui.Controls;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Pages
{
	public partial class MainPage
	{
		readonly IServiceProvider _services;
		readonly MainViewModel _viewModel;

		public MainPage(IServiceProvider services, MainViewModel viewModel)
		{
			InitializeComponent();

			BindingContext = viewModel;

			_services = services;
			_viewModel = viewModel;

			Debug.WriteLine($"Received as parameters, ServiceProvider: {_services != null} and MainViewModel: {_viewModel != null}");
		}

		void TestVisualTreeHelper(object sender, EventArgs e)
		{
			((IWindow)this.Parent.Parent).VisualDiagnosticsLayer.AddAdorner(MauiTextLabel as IVisualTreeElement, true);
		}

		void EnableElementPicker(object sender, EventArgs e)
		{
			((IWindow)this.Parent.Parent).VisualDiagnosticsLayer.DisableUITouchEventPassthrough = true;
		}
	}
}