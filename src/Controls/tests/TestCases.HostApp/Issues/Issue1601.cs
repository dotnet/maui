namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 1601, "Exception thrown when `Removing Content Using LayoutCompression", PlatformAffected.Android)]
	public class Issue1601 : TestContentPage
	{
		protected override void Init()
		{
			var grid = new Grid();
			Microsoft.Maui.Controls.CompressedLayout.SetIsHeadless(grid, true);
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Star });
			grid.RowDefinitions.Add(new RowDefinition { Height = 40 });

			var boxView = new BoxView { BackgroundColor = Colors.Red };
			var backgroundContainer = new Grid();
			Microsoft.Maui.Controls.CompressedLayout.SetIsHeadless(backgroundContainer, true);
			backgroundContainer.Children.Add(boxView);
			grid.Children.Add(backgroundContainer);

			var stack = new StackLayout
			{
				Orientation = StackOrientation.Horizontal
			};
			Microsoft.Maui.Controls.CompressedLayout.SetIsHeadless(stack, true);
			var button = new Button { AutomationId = "CrashButton", Text = "CRASH!" };
			stack.Children.Add(button);
			grid.Add(stack, 0, 1);

			button.Clicked += (s, e) => Content = null;

			Content = grid;
		}
	}
}
