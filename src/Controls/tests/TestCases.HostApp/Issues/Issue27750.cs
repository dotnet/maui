namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27750, "[iOS] Editor scrolled to the bottom when tapped while inside the ScrollView", PlatformAffected.iOS)]
public partial class Issue27750 : ContentPage
{
	public Issue27750()
	{

		var scrollView = new ScrollView();
		var grid = new Grid
		{
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto }
			},
			RowSpacing = 4
		};
		var boxView = new BoxView
		{
			HeightRequest = 50,
			BackgroundColor = Colors.Aqua,
		};
		var editor = new UITestEditor
		{
			Placeholder = "Editor with 25 Height Request",
			AutomationId = "Editor",
			HeightRequest = 25,
			IsCursorVisible = false,
			TextColor = Colors.Black,
			BackgroundColor = Colors.SpringGreen,
		};
		grid.Children.Add(boxView);
		grid.Children.Add(editor);
		grid.SetRow(editor, 1);

		scrollView.Content = grid;
		Content = scrollView;
	}
}