namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2414, "NullReferenceException when swiping over Context Actions", PlatformAffected.WinPhone)]
public class Issue2414 : TestContentPage
{
	protected override void Init()
	{
		var tableView = new TableView
		{
			Intent = TableIntent.Settings,
			Root = new TableRoot("TableView Title")
			{
				new TableSection("Table Section 2")
				{
					new TextCell
					{
						Text = "Swipe ME",
						Detail = "And I will crash!",
						ContextActions = {
							new MenuItem
							{
								Text = "Text0"
							},new MenuItem
							{
								Text = "Text1"
							},
							new MenuItem
							{
								Text = "Text2"
							},
							new MenuItem
							{
								Text = "Text3"
							},
							new MenuItem
							{
								Text = "Text4",
								IsDestructive = true,
							}}
					},
				}
			}
		};
		Content = tableView;
	}
}
