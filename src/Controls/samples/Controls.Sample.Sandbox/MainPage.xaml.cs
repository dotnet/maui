using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	// public MainPage()
	// {
	// 	InitializeComponent();
	// }

	// protected override void OnAppearing()
	// {
	// 	base.OnAppearing();

	// 	var layout = new VerticalStackLayout();
	// 	layout.Add(new Label { Text = "Label 1 in a vertical stack" });

	// 	var button = new Button { Text = "Button 1" };
	// 	button.Clicked += (sender, args) => Console.WriteLine("Button in Title Clicked");
	// 	layout.Add(button);
		
	// 	Window.TitleBar = new TitleBar()
	// 	{
	// 		Title = "MAUI App",
	// 		Icon = "dotnet_bot.png",
	// 		ForegroundColor = Colors.Blue,
	// 		Background = Colors.LightGreen,
	// 		Content = new Entry(){
	// 			Text = "Search",
	// 		},
	// 		// TODO, add a clicked command and see if we can click
	// 		LeadingContent = layout,
	// 		TrailingContent = new Label(){
	// 			Text = "Trailing",
	// 		},
	// 		Subtitle = "This is a subtitle",
	// 		// LeadingContent = new AvatarButton()
	// 	};
	// }

	TitleBarViewModel _viewModel;
	TitleBar _customTitleBar;

	public MainPage()
	{
		InitializeComponent();

		_viewModel = new TitleBarViewModel();
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
			_customTitleBar.Icon = "dotnet_bot.png";
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

	private void ForegroundColorButton_Clicked(object sender, EventArgs e)
	{
		if (Microsoft.Maui.Graphics.Color.TryParse(ForegroundColorTextBox.Text, out var color))
		{
			_customTitleBar.ForegroundColor = color;
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
			_customTitleBar.Content = new SearchBar()
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
			_customTitleBar.TrailingContent = new Border()
			{
				WidthRequest = 32,
				HeightRequest = 32,
				StrokeShape = new Ellipse() { WidthRequest = 32, HeightRequest = 32 },
				StrokeThickness = 0,
				BackgroundColor = Microsoft.Maui.Graphics.Colors.Azure,
				Content = new Label()
				{
					Text = "MC",
					TextColor = Microsoft.Maui.Graphics.Colors.Black,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center,
					FontSize = 10
				}
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
			_customTitleBar.HeightRequest = 100;
		}
		else
		{
			_customTitleBar.HeightRequest = -1;
		}
	}
}
