using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{

	public class MapPage : ContentPage

	{
		public MapPage()
		{
			Content = new Microsoft.Maui.Controls.Maps.Map();
		}
	}
	public partial class CustomNavigationPage : NavigationPage
	{
		MainViewModel ViewModel { get; }

		public CustomNavigationPage(IServiceProvider services, MainViewModel viewModel) :
			base(new MapPage())
		{
			InitializeComponent();
			ViewModel = viewModel;
		}
	}
}