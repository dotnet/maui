namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 20922, "Shadow Doesn't Work on Grid in scroll view on Android", PlatformAffected.Android)]
	public class Issue20922 : ContentPage
	{
		public Issue20922()
		{
			var scrollView = new ScrollView();

			var grid = new Grid
			{
				HeightRequest = 200,
				WidthRequest = 200,
				AutomationId = "Grid",
				BackgroundColor = Colors.Red
			};

			var shadow = new Shadow
			{
				Radius = 20,
				Brush = new SolidColorBrush(Colors.Gray),
				Offset = new Point(1, 50)
			};

			grid.Shadow = shadow;
			scrollView.Content = grid;
			Content = scrollView;
		}
	}
}
