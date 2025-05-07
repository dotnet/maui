namespace Maui.Controls.Sample.Issues;
using Microsoft.Maui.Controls;
using System;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 29236, "Window dimensions not updating after device orientation changes on iOS", PlatformAffected.iOS)]
public partial class Issue29236 : ContentPage
{
	private Label WindowWidth, WindowHeight;

	public Issue29236()
	{
		WindowWidth = new Label { AutomationId = "WindowWidth" };
		WindowHeight = new Label { AutomationId = "WindowHeight" };

		var grid = new Grid
		{
			ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition() },
			RowDefinitions = { new RowDefinition(), new RowDefinition() }
		};

		grid.Add(new Label { Text = "Width:" }, 0, 0);
		grid.Add(new Label { Text = "Height:" }, 0, 1);
		grid.Add(WindowWidth, 1, 0);
		grid.Add(WindowHeight, 1, 1);

		var button = new Button
		{
			Text = "Get dimensions",
			HorizontalOptions = LayoutOptions.Fill,
			AutomationId = "GetDimensionsButton"
		};
		button.Clicked += UpdateDimensions;

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20,
			Children =
			{
				new Label { Text = "Window Dimension Test", AutomationId = "Label" },
				grid,
				button
			}
		};
	}

	private void UpdateDimensions(object sender, EventArgs e)
	{
		WindowWidth.Text = Application.Current.Windows[0].Width.ToString();
		WindowHeight.Text = Application.Current.Windows[0].Height.ToString();
	}
}