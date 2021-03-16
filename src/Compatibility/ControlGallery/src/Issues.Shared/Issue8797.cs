using Microsoft.Maui.Controls.CustomAttributes;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.Issues
{
	[Issue(IssueTracker.Github, 8797, "[Bug] Word wrapped Label not measured correctly",
		PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue8797 : TestContentPage
	{
		protected override void Init()
		{
			var scrollView = new ScrollView();
			var layout = new StackLayout { Margin = new Thickness(5, 80, 5, 0) };

			var instructions = new Label { Text = "The text visible in the Grid should end with 'finding a good way to spend it'. If that text is cut off, this test has failed." };

			BackgroundColor = Color.BlanchedAlmond;

			var grid = new Grid
			{
				VerticalOptions = LayoutOptions.Start,
				BackgroundColor = Color.Bisque,
				Margin = new Thickness(0, 40, 0, 0),
				ColumnSpacing = 6
			};

			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Star });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5, GridUnitType.Star) });

			grid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });

			var label = new Label
			{
				VerticalOptions = LayoutOptions.Start,
				LineBreakMode = LineBreakMode.WordWrap,
				BackgroundColor = Color.CornflowerBlue,
				FontSize = 10,
				Text = "There's a 104 days of summer vacation 'til school comes along just to end it. So the annual problem for our generation is finding a good way to spend it."
			};

			grid.Children.Add(label, 1, 0);

			layout.Children.Add(instructions);
			layout.Children.Add(grid);

			scrollView.Content = layout;

			Content = scrollView;
		}
	}
}
