using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public class StatusBarThemePage : ContentPage
	{
		readonly Label _statusLabel;

		public StatusBarThemePage()
		{
			Title = "StatusBarTheme";

			_statusLabel = new Label
			{
				Text = "Current: Default",
				FontSize = 18,
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 20, 0, 0)
			};

			Content = new VerticalStackLayout
			{
				Spacing = 12,
				Padding = new Thickness(20),
				Children =
				{
					new Label
					{
						Text = "Window.StatusBarTheme",
						FontSize = 24,
						FontAttributes = FontAttributes.Bold
					},
					new Label
					{
						Text = "Controls whether OS-drawn status bar icons (clock, battery, signal) are light or dark. " +
							"On mobile, set Dark for pages with dark headers so icons are white."
					},
					_statusLabel,
					CreateButton("Default (follow theme)", StatusBarTheme.Default, Colors.Transparent),
					CreateButton("Light (dark icons)", StatusBarTheme.Light, Color.FromArgb("#F0F0F0")),
					CreateButton("Dark (light icons)", StatusBarTheme.Dark, Color.FromArgb("#1A1A2E")),
				}
			};
		}

		Button CreateButton(string text, StatusBarTheme theme, Color bgColor)
		{
			var button = new Button
			{
				Text = text,
				BackgroundColor = bgColor,
				TextColor = bgColor.GetLuminosity() > 0.5 ? Colors.Black : Colors.White
			};

			button.Clicked += (s, e) =>
			{
				if (this.Window is Window window)
				{
					window.StatusBarTheme = theme;
					_statusLabel.Text = $"Current: {theme}";
				}
			};

			return button;
		}
	}
}
