using System;
using Maui.Controls.Sample.ViewModels;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsTitleBarPage : ContentPage
	{
		private TitleBarSampleViewModel _viewModel;

		public WindowsTitleBarPage()
		{
			InitializeComponent();

			_viewModel = new TitleBarSampleViewModel();
			BindingContext = _viewModel;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			Window.SetTitleBar(CustomTitleBar);
		}

		private void SetIconCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				CustomTitleBar.Icon = "tb_appicon.png";
			}
			else
			{
				CustomTitleBar.Icon = "";
			}
		}

		private void ColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(ColorTextBox.Text, out var color))
			{
				CustomTitleBar.BackgroundColor = color;
			}
		}

		private void InactiveColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(InactiveColorTextBox.Text, out var color))
			{
				CustomTitleBar.InactiveBackgroundColor = color;
			}
		}

		private void ForegroundColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(ForegroundColorTextBox.Text, out var color))
			{
				CustomTitleBar.ForegroundColor = color;
			}
		}

		private void InactiveForegroundColorButton_Clicked(object sender, EventArgs e)
		{
			if (Microsoft.Maui.Graphics.Color.TryParse(InactiveForegroundColorTextBox.Text, out var color))
			{
				CustomTitleBar.InactiveForegroundColor = color;
			}
		}

		private void LeadingCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				CustomTitleBar.LeadingContent = new Button()
				{
					Text = "Leading"
				};
			}
			else
			{
				CustomTitleBar.LeadingContent = null;
			}
		}

		private void ContentCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				CustomTitleBar.Content = new Entry()
				{
					Placeholder = "Search",
					MinimumWidthRequest = 200,
					MaximumWidthRequest = 500,
					HeightRequest = 32
				};
			}
			else
			{
				CustomTitleBar.Content = null;
			}
		}

		private void TrailingCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				CustomTitleBar.TrailingContent = new Button()
				{
					Text = "Trailing"
				};
			}
			else
			{
				CustomTitleBar.TrailingContent = null;
			}
		}

		private void TallModeCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (e.Value)
			{
				CustomTitleBar.HeightRequest = 48;
			}
			else
			{
				CustomTitleBar.HeightRequest = 32;
			}
		}
	}
}
