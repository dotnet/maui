namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 29236, "Window dimensions not updating after device orientation changes on iOS", PlatformAffected.iOS)]
public partial class Issue29236 : ContentPage
{
	private Label windowWidth, windowHeight;

	public Issue29236()
	{
		windowWidth = new Label { AutomationId = "windowWidth" };
		windowHeight = new Label { AutomationId = "windowHeight" };

		var grid = new Grid
		{
			ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() },
			RowDefinitions = { new RowDefinition(), new RowDefinition() }
		};

		grid.Add(new Label { Text = "Width:" }, 0, 0);
		grid.Add(new Label { Text = "Height:" }, 0, 1);
		grid.Add(windowWidth, 1, 0);
		grid.Add(windowHeight, 1, 1);

		var button = new Button
		{
			Text = "Get Dimensions",
			HorizontalOptions = LayoutOptions.Fill,
			AutomationId = "getDimensions"
		};
		button.Clicked += UpdateDimensions;

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			Children =
			{
				new Label { Text = "Window Dimension Test", AutomationId = "windowTestTitle" },
				grid,
				button
			}
		};
	}

	private void UpdateDimensions(object sender, EventArgs e)
	{
		windowWidth.Text = Application.Current.Windows[0].Width.ToString();
		windowHeight.Text = Application.Current.Windows[0].Height.ToString();
	}
}