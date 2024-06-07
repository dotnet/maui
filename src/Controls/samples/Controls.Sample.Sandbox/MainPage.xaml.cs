using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Maui.Controls;
namespace Maui.Controls.Sample
{
	public partial class MainPage : ContentPage
	{
		private MainPageViewModel viewModel = new MainPageViewModel();
		private TitleBar? _titleBar;

		public MainPage()
		{
			InitializeComponent();
			BindingContext = viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			_titleBar = new TitleBar();
			Window.TitleBar = _titleBar;
		}

		private void SetIconCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (_titleBar == null)
				return;

			if (e.Value)
			{
				_titleBar.Icon = "appicon.png";
			}
			else
			{
				_titleBar.Icon = "";
			}
		}

		private void TitleButton_Clicked(object sender, EventArgs e)
		{
			if (_titleBar == null)
				return;

			_titleBar.Title = TitleTextBox.Text;
		}

		private void SubtitleButton_Clicked(object sender, EventArgs e)
		{
			if (_titleBar == null)
				return;

			_titleBar.Subtitle = SubtitleTextBox.Text;
		}

		private void ColorButton_Clicked(object sender, EventArgs e)
		{
			if (_titleBar == null)
				return;

			if (Microsoft.Maui.Graphics.Color.TryParse(ColorTextBox.Text, out var color))
			{
				_titleBar.BackgroundColor = color;
			}
		}

		private void LeadingCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (_titleBar == null)
				return;

			if (e.Value)
			{
				_titleBar.LeadingContent = new Button()
				{
					Text = "Leading"
				};
			}
			else
			{
				_titleBar.LeadingContent = null;
			}
		}

		private void ContentCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (_titleBar == null)
				return;

			if (e.Value)
			{
				_titleBar.Content = new Entry()
				{
					Placeholder = "Search",
					MinimumWidthRequest = 200,
					MaximumWidthRequest = 500,
					HeightRequest = 32
				};
			}
			else
			{
				_titleBar.Content = null;
			}
		}

		private void TrailingCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (_titleBar == null)
				return;

			if (e.Value)
			{
				_titleBar.TrailingContent = new Button()
				{
					Text = "Trailing"
				};
			}
			else
			{
				_titleBar.TrailingContent = null;
			}
		}

		private void TallModeCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (_titleBar == null)
				return;

			if (e.Value)
			{
				_titleBar.HeightRequest = 48;
			}
			else
			{
				_titleBar.HeightRequest = 32;
			}
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