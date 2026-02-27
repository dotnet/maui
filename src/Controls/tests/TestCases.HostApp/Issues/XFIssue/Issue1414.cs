namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1414, "InvalidCastException when scrolling and refreshing TableView", PlatformAffected.iOS)]
public class Issue1414 : TestContentPage
{
	ViewCell BuildCell(int sectionIndex, int cellIndex)
	{
		var grid = new Grid
		{
			ColumnDefinitions = {
				new ColumnDefinition { Width = GridLength.Star },
				new ColumnDefinition { Width = GridLength.Star }
			},
			AutomationId = $"Row-{sectionIndex}-{cellIndex}"
		};

		if (cellIndex % 2 == 0)
		{
			grid.AddChild(new Label { Text = $"Cell {sectionIndex}-{cellIndex}" }, 0, 0);
			grid.AddChild(new Label { Text = $"Label" }, 1, 0);
			grid.BackgroundColor = Colors.Fuchsia;
		}
		else
		{
			grid.AddChild(new Label { Text = $"Cell {sectionIndex}-{cellIndex}" }, 0, 0);
			grid.AddChild(new Entry { Text = $"Entry" }, 1, 0);
			grid.BackgroundColor = Colors.Yellow;
		}

		return new ViewCell
		{
			View = grid,
		};
	}

	protected override void Init()
	{
		var tableView = new Microsoft.Maui.Controls.TableView
		{
			HasUnevenRows = true,
			Intent = TableIntent.Form,
			Root = new TableRoot(),
			AutomationId = "TableView"
		};

		for (int sectionIndex = 0; sectionIndex < 5; sectionIndex++)
		{
			var section = new TableSection($"Section {sectionIndex}");

			for (int cellIndex = 0; cellIndex < 25; cellIndex++)
			{
				section.Add(BuildCell(sectionIndex, cellIndex));
			}

			tableView.Root.Add(section);
		}
		SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
		Content = tableView;
	}
}