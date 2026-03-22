namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29499, "[Android] The number of SearchHandler toolbar item increases abnormally", PlatformAffected.Android)]
public class Issue29499 : Shell
{
	public Issue29499()
	{
		Routing.RegisterRoute(nameof(Issue29499Subpage), typeof(Issue29499Subpage));
		Items.Add(new Issue29499Page());
	}

	public class Issue29499Page : ContentPage
	{
		public Issue29499Page()
		{
			this.SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			SetForegroundColor(this, Colors.Red);
			ToolbarItems.Add(new ToolbarItem { Text = "Item1", Order = ToolbarItemOrder.Secondary });
			ToolbarItems.Add(new ToolbarItem { Text = "Item2", Order = ToolbarItemOrder.Secondary });
			ToolbarItems.Add(new ToolbarItem { Text = "Item3", Order = ToolbarItemOrder.Secondary });
			ToolbarItems.Add(new ToolbarItem { Text = "Item4", Order = ToolbarItemOrder.Secondary });
			SetSearchHandler(this, new MainSearchHandler { SearchBoxVisibility = SearchBoxVisibility.Collapsible });
			Content = new Button
			{
				Text = "Go to subpage page",
				AutomationId = "GotoIssue29499Subpage",
				Command = new Command(async () =>
				{
					await Current.GoToAsync(nameof(Issue29499Subpage));
				})
			};
		}

		public class MainSearchHandler : SearchHandler { }
	}

	public class Issue29499Subpage : ContentPage
	{
		public Issue29499Subpage()
		{
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "Go back",
						AutomationId = "GoBackButton",
						Command = new Command(async () =>
						{
							await Current.GoToAsync("..");
						})
					},
				}
			};
		}
	}
}


