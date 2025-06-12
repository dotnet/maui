namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23803, "FlyoutItem in overflow menu not fully interactable", PlatformAffected.UWP)]
	public partial class Issue23803 : TestShell
	{
		protected override void Init()
		{
			CreateTabContent();
		}

		private void CreateTabContent()
		{
			var flyoutItem = new FlyoutItem { Title = "Header" };
			for (int i = 1; i <= 20; i++)
			{
				var contentPage = new ContentPage
				{
					Content = new Button
					{
						Text = $"Button_{i}",
						AutomationId = $"Button{i}",
						FontSize = 24,
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center
					}
				};

				var shellContent = new ShellContent
				{
					Title = $"Tab{i}",
					Content = contentPage
				};

				flyoutItem.Items.Add(shellContent);
			}
			this.FlyoutBehavior = FlyoutBehavior.Disabled;
			this.Items.Add(flyoutItem);
		}
	}
}