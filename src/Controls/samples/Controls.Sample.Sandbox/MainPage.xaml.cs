namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public MainPage()
	{
		InitializeComponent();
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();

		var layout = new VerticalStackLayout();
		layout.Add(new Label { Text = "Label 1 in a vertical stack" });
		
		Window.TitleBar = new TitleBar()
		{
			Title = "MAUI App",
			Icon = "dotnet_bot.png",
			ForegroundColor = Colors.Blue,
			Background = Colors.LightGreen,
			Content = new Entry(){
				Text = "Search"
			},
			LeadingContent = layout,
			TrailingContent = new Label(){
				Text = "Trailing",
			},
			Subtitle = "This is a subtitle",
			// LeadingContent = new AvatarButton()
		};
	}
}
