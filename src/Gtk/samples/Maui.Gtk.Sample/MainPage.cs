using System.IO;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Gtk.Sample;

/// <summary>A code-only page showing the controls the core GTK backend supports.</summary>
public sealed class MainPage : ContentPage
{
	readonly Label _status = new() { Text = "Press the button", TextColor = Colors.Gray };
	int _count;

	public MainPage()
	{
		TitleLabel = new Label { Text = "Hello, MAUI on GTK 4", FontSize = 24, FontAttributes = FontAttributes.Bold };

		var button = new Button { Text = "Click me" };
		button.Clicked += (_, _) => _status.Text = $"Clicked {++_count} time(s)";

		var entry = new Entry { Placeholder = "Type something", ClearButtonVisibility = ClearButtonVisibility.WhileEditing };
		entry.TextChanged += (_, e) => _status.Text = $"Entry: {e.NewTextValue}";

		var editor = new Editor { Placeholder = "A multi-line editor", HeightRequest = 80 };

		var image = new Image
		{
			Source = ImageSource.FromStream(() => new MemoryStream(SampleImage.CreatePng(64, 32))),
			HeightRequest = 48,
			WidthRequest = 96,
			Aspect = Aspect.Fill,
			HorizontalOptions = LayoutOptions.Start,
		};

		// MauiImage / MauiFont / MauiAsset (see the project file): Resources/Images/dotnet_bot.png, Resources/Fonts/OpenSans-Regular.ttf.
		var bot = new Image { Source = "dotnet_bot", HeightRequest = 104, WidthRequest = 84, HorizontalOptions = LayoutOptions.Start };
		var fontLabel = new Label { Text = "OpenSans-Regular from MauiFont", FontFamily = "OpenSansRegular", FontSize = 16 };

		var card = new Border
		{
			StrokeThickness = 2,
			Stroke = Colors.SteelBlue,
			StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(12) },
			Background = new LinearGradientBrush(
				new GradientStopCollection { new GradientStop(Colors.LightSkyBlue, 0), new GradientStop(Colors.White, 1) },
				new Point(0, 0),
				new Point(1, 1)),
			Padding = new Thickness(12),
			Content = new Label { Text = "A border with a gradient", TextColor = Colors.Black },
		};

		Content = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 12,
				Children =
				{
					TitleLabel,
					_status,
					button,
					entry,
					editor,
					new HorizontalStackLayout
					{
						Spacing = 12,
						Children =
						{
							new Label { Text = "Switch", VerticalOptions = LayoutOptions.Center },
							new Switch(),
							new Label { Text = "Check", VerticalOptions = LayoutOptions.Center },
							new CheckBox { IsChecked = true },
						},
					},
					image,
					bot,
					fontLabel,
					card,
				},
			},
		};
	}

	public Label TitleLabel { get; }
}
