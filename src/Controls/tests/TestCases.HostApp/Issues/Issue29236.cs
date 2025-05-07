namespace Maui.Controls.Sample.Issues;
using Microsoft.Maui.Controls;
using System;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 29236, "Window dimensions not updating after device orientation changes on iOS", PlatformAffected.iOS)]
public partial class Issue29236 : ContentPage
{
	private Label WindowWidth, WindowHeight, PageWidth, PageHeight;

	public Issue29236()
	{
		WindowWidth = new Label {AutomationId = "WindowWidth"};
		WindowHeight = new Label {AutomationId = "WindowHeight"};
		PageWidth = new Label {AutomationId = "PageWidth"};
		PageHeight = new Label {AutomationId = "PageHeight"};

		var grid = new Grid
		{
			ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition() },
			RowDefinitions = { new RowDefinition(), new RowDefinition(), new RowDefinition() }
		};

		grid.Add(new Label { Text = "Width:" }, 0, 1);
		grid.Add(new Label { Text = "Height:" }, 0, 2);
		grid.Add(new Label { Text = "Window" }, 1, 0);
		grid.Add(new Label { Text = "Page" }, 2, 0);

		grid.Add(WindowWidth, 1, 1);
		grid.Add(WindowHeight, 1, 2);
		grid.Add(PageWidth, 2, 1);
		grid.Add(PageHeight, 2, 2);

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
					new Label { Text = "Window Dimension Test" , AutomationId = "Label"},
					grid,
					button
				}
		};
	}

	private void UpdateDimensions(object sender, EventArgs e)
	{
		WindowWidth.Text = Application.Current.Windows[0].Width.ToString();
		WindowHeight.Text = Application.Current.Windows[0].Height.ToString();
		PageWidth.Text = Width.ToString();
		PageHeight.Text = Height.ToString();
	}
}
