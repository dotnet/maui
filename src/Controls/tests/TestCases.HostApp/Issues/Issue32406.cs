namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32406, "LayoutCycleException caused by nested Borders in ControlTemplates", PlatformAffected.UWP)]
public class Issue32406 : ContentPage
{
	public Issue32406()
	{
		var resultLabel = new Label
		{
			Text = "Waiting",
			AutomationId = "ResultLabel"
		};

		var container = new VerticalStackLayout();

		// Create enough nested Border elements to trigger the layout cycle.
		// The original report shows crashes at ~300 elements with 3+ nesting levels.
		// Each element creates nestingDepth Borders (350 * 3 = 1050 total Borders).
		const int elementCount = 350;
		const int nestingDepth = 3;

		for (int i = 0; i < elementCount; i++)
		{
			View innermost = new Label { Text = $"Item {i}" };

			for (int j = 0; j < nestingDepth; j++)
			{
				innermost = new Border
				{
					StrokeThickness = 1,
					Stroke = Colors.Gray,
					Content = new HorizontalStackLayout
					{
						Children = { innermost }
					}
				};
			}

			container.Children.Add(innermost);
		}

		var scrollView = new ScrollView { Content = container };

		Content = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Auto),
				new RowDefinition(GridLength.Star)
			},
			Children =
			{
				resultLabel,
				scrollView
			}
		};

		Grid.SetRow(scrollView, 1);

		// If we reach this point without a LayoutCycleException, the test passes.
		// Use Loaded to confirm the page rendered successfully.
		Loaded += (s, e) => resultLabel.Text = "Success";
	}
}
