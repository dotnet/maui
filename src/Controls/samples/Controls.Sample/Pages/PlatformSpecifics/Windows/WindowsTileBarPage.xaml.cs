using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsTitleBarPage : ContentPage
	{
		TitleBarSampleViewModel _viewModel;
		TitleBar _customTitleBar;

		public WindowsTitleBarPage()
		{
			InitializeComponent();

			_viewModel = new TitleBarSampleViewModel();
			BindingContext = _viewModel;

			string titleBarXaml =
				"""
				<TitleBar
					xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
					Title="{Binding Title}"
					Subtitle="{Binding Subtitle}"
					IsVisible="{Binding ShowTitleBar}"/>
				""";

			_customTitleBar = new TitleBar().LoadFromXaml(titleBarXaml);
			_customTitleBar.BindingContext = _viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();
			Window.TitleBar = _customTitleBar;
		}

		private void SetIconCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				_customTitleBar.Icon = "tb_appicon.png";
			}
			else
			{
				_customTitleBar.Icon = "";
			}
		}

		private void ColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(ColorTextBox.Text, out var color))
			{
				_customTitleBar.BackgroundColor = color;
			}
		}

		private void InactiveColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(InactiveColorTextBox.Text, out var color))
			{
				_customTitleBar.InactiveBackgroundColor = color;
			}
		}

		private void ForegroundColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(ForegroundColorTextBox.Text, out var color))
			{
				_customTitleBar.ForegroundColor = color;
			}
		}

		private void InactiveForegroundColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(InactiveForegroundColorTextBox.Text, out var color))
			{
				_customTitleBar.InactiveForegroundColor = color;
			}
		}

		private void LeadingCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				_customTitleBar.LeadingContent = new Button()
				{
					Text = "Leading"
				};
			}
			else
			{
				_customTitleBar.LeadingContent = null;
			}
		}

		private void ContentCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				_customTitleBar.Content = new Entry()
				{
					Placeholder = "Search",
					MinimumWidthRequest = 200,
					MaximumWidthRequest = 500,
					HeightRequest = 32
				};
			}
			else
			{
				_customTitleBar.Content = null;
			}
		}

		private void TrailingCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				_customTitleBar.TrailingContent = new Button()
				{
					Text = "Trailing"
				};
			}
			else
			{
				_customTitleBar.TrailingContent = null;
			}
		}

		private void TallModeCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				_customTitleBar.HeightRequest = 48;
			}
			else
			{
				_customTitleBar.HeightRequest = 32;
			}
		}
	}
}
