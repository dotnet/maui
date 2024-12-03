namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 3342, "[Android] BoxView BackgroundColor not working on 3.2.0-pre1", PlatformAffected.Android)]
	public class Issue3342 : TestContentPage
	{
		protected override void Init()
		{
			var instructions = new Label
			{
				Text = "You should see a green BoxView. You should not see the text that says FAIL."
			};

			var hiddenLabel = new Label
			{
				Text = "FAIL",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			var target = new BoxView
			{
				HeightRequest = 200,
				WidthRequest = 200,
				BackgroundColor = Colors.Green,
				CornerRadius = new CornerRadius(3)
			};

			var grid = new Grid
			{
				ColumnDefinitions = { new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) } },
				RowDefinitions = {  new RowDefinition { Height = new GridLength(1, GridUnitType.Star) },
									new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }}
			};

			grid.Add(instructions);
			grid.Add(hiddenLabel, 0, 1);
			grid.Add(target, 0, 1);

			Content = grid;
		}
	}
}
