namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23803, "FlyoutItem in overlow menu not fully interactable", PlatformAffected.UWP)]
	public partial class Issue23803 : Shell
	{
		public Issue23803()
		{
			InitializeComponent();
			CreateTabContent();
		}
		private void CreateTabContent()
		{
			// Create the TabBar
			var tabBar = new TabBar
			{
				Title = "Header",
				AutomationId = "TabBar"
			};

			// Loop to create ShellContent with different content for each tab
			for (int i = 1; i <= 20; i++)
			{
				// Create a new ContentPage dynamically
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

				// Create a new ShellContent and set the dynamically created ContentPage as content
				var shellContent = new ShellContent
				{
					Title = $"Tab{i}",
					Content = contentPage // Set the dynamic ContentPage as content
				};

				// Add the ShellContent to the TabBar
				tabBar.Items.Add(shellContent);
			}

			// Add the TabBar to the Shell
			this.Items.Add(tabBar);
		}
	}
}