using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.CustomAttributes;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Issues
{
	[Preserve(AllMembers = true)]
	[Issue(IssueTracker.Bugzilla, 36559, "[WP] Navigating to a ContentPage with a Grid inside a TableView affects Entry heights")]
	public class Bugzilla36559 : TestContentPage
	{
		protected override void Init()
		{
			var label = new Label { Text = "Label" };
			var entry = new Entry { AutomationId = "entry" };
			var grid = new Grid();

			grid.Add(label, 0, 0);
			grid.Add(entry, 1, 0);
			var tableView = new TableView
			{
				Root = new TableRoot
				{
					new TableSection
					{
						new ViewCell
						{
							View = grid
						}
					}
				}
			};

			Content = new StackLayout
			{
				Children = { tableView }
			};
		}
	}
}
