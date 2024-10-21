namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Bugzilla, 53445, "Setting Grid.IsEnabled to false does not disable child controls", PlatformAffected.All)]
	public class Bugzilla53445 : TestContentPage
	{
		protected override void Init()
		{
			var layout = new StackLayout { VerticalOptions = LayoutOptions.Fill, Spacing = 20 };

			var status = new Label { AutomationId = "Success", Text = "Success" };

			var instructions = new Label { Text = "Disable all of the layouts by clicking the Toggle button. Then click the buttons inside each layout. If the status changes from Success to Fail, this test has failed." };

			var grid = new Grid
			{
				BackgroundColor = Colors.Blue,
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "grid"
			};

			var gridButton = new Button { AutomationId = "gridbutton", Text = "Test", WidthRequest = 50 };
			grid.Children.Add(gridButton);
			gridButton.Clicked += (sender, args) => status.Text = "Fail";

			var contentView = new ContentView
			{
				BackgroundColor = Colors.Green,
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "contentView"
			};

			var contentViewButton = new Button { AutomationId = "contentviewbutton", Text = "Test", WidthRequest = 50 };
			contentView.Content = contentViewButton;
			contentViewButton.Clicked += (sender, args) => status.Text = "Fail";

			var stackLayout = new StackLayout
			{
				BackgroundColor = Colors.Orange,
				IsEnabled = true,
				WidthRequest = 250,
				HeightRequest = 50,
				AutomationId = "stackLayout"
			};

			var stackLayoutButton = new Button { AutomationId = "stacklayoutbutton", Text = "Test", WidthRequest = 50 };
			stackLayout.Children.Add(stackLayoutButton);
			stackLayoutButton.Clicked += (sender, args) => status.Text = "Fail";

			var toggleButton = new Button { AutomationId = "toggle", Text = $"Toggle IsEnabled (currently {grid.IsEnabled})" };
			toggleButton.Clicked += (sender, args) =>
			{
				grid.IsEnabled = !grid.IsEnabled;
				contentView.IsEnabled = !contentView.IsEnabled;
				stackLayout.IsEnabled = !stackLayout.IsEnabled;
				toggleButton.Text = $"Toggle IsEnabled (currently {grid.IsEnabled})";
			};

			layout.Children.Add(instructions);
			layout.Children.Add(status);
			layout.Children.Add(toggleButton);
			layout.Children.Add(grid);
			layout.Children.Add(contentView);
			layout.Children.Add(stackLayout);

			Content = layout;
		}
	}
}