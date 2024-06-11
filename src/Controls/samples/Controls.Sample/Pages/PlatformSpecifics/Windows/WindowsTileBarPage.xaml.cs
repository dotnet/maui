using System;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages
{
	public partial class WindowsTitleBarPage : ContentPage
	{
		private TitleBar? _titleBar;
		public WindowsTitleBarPage()
		{
			InitializeComponent();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			_titleBar = new TitleBar(Window);
			Window.TitleBar = _titleBar;
		}

		private void SetIconCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (_titleBar == null)
				return;

			if (e.Value)
			{
				_titleBar.Icon = "tb_appicon.png";
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

		private void InactiveColorButton_Clicked(object sender, EventArgs e)
		{
			if (_titleBar == null)
				return;

			if (Microsoft.Maui.Graphics.Color.TryParse(InactiveColorTextBox.Text, out var color))
			{
				_titleBar.InactiveBackgroundColor = color;
			}
		}

		private void ForegroundColorButton_Clicked(object sender, EventArgs e)
		{
			if (_titleBar == null)
				return;

			if (Microsoft.Maui.Graphics.Color.TryParse(ForegroundColorTextBox.Text, out var color))
			{
				_titleBar.ForegroundColor = color;
			}
		}

		private void InactiveForegroundColorButton_Clicked(object sender, EventArgs e)
		{
			if (_titleBar == null)
				return;

			if (Microsoft.Maui.Graphics.Color.TryParse(InactiveForegroundColorTextBox.Text, out var color))
			{
				_titleBar.InactiveForegroundColor = color;
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

		private void VisibilityCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
		{
			if (_titleBar == null)
				return;

			if (e.Value)
			{
				_titleBar.IsVisible = false;
			}
			else
			{
				_titleBar.IsVisible = true;
			}
		}
	}
}
