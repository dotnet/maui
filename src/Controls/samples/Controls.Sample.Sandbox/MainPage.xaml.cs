using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		private MainPageViewModel viewModel = new MainPageViewModel();

		public MainPage()
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		private void CustomTitlebarButton_Clicked(object sender, EventArgs e)
		{
			Window.TitleBar = new TitlebarSample()
			{
				BindingContext = viewModel,
				HeightRequest = 60
			};
        }

		private void NormalTitlebarButton_Clicked(object sender, EventArgs e)
		{
			var grid = new Grid()
			{
				InputTransparent = true
			};
			var text = new Label
			{ 
				Text = "New Window",
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Center,
				Margin = new Microsoft.Maui.Thickness(8, 0, 0, 0)
			};
			grid.Children.Add(text);

			Window.TitleBar = grid;
		}
	}

	class MainPageViewModel : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler? PropertyChanged;

		private string _searchText = string.Empty;
		public string SearchText
		{
			get => _searchText;
			set
			{
				if (_searchText != value)
				{
					_searchText = value;
					OnPropertyChanged();
				}
			}
		}

		public void OnPropertyChanged([CallerMemberName] string name = "") =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
	}
}