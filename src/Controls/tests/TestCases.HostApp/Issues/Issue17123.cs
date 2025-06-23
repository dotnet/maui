using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 17123, "TableSection and TableRoot Title properties are displayed as lower case", PlatformAffected.All)]
	public class Issue17123 : TestContentPage
	{
		protected override void Init()
		{
			var tableRootLabel = new Label() { Text = "TableRoot Title Text", TextColor = Colors.Red, AutomationId = "TableRootLabel" };
			var tableSectionLabel = new Label() { Text = "TableSection Title Text", TextColor = Colors.Red, AutomationId = "TableSectionLabel" };

			var tableView = new TableView() { Intent = TableIntent.Menu, AutomationId = "TableView", HorizontalOptions = LayoutOptions.Center };
			var tableRoot = new TableRoot(tableRootLabel.Text);
			var tableSection = new TableSection(tableSectionLabel.Text)
			{
				new TextCell() { Text = "TextCell Text" , Detail="TextCell Detail" },
				new EntryCell() { Label = "EntryCell Label", Placeholder="EntryCell Placeholder" },
			};

			tableRoot.Add(tableSection);
			tableView.Root = tableRoot;

			var stackLayout = new StackLayout();
			stackLayout.Children.Add(tableRootLabel);
			stackLayout.Children.Add(tableSectionLabel);
			stackLayout.Children.Add(tableView);

			Content = stackLayout;
		}
	}
}