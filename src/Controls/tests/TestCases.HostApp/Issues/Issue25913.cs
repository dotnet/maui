namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25913, "Top Tab Visibility Changes Not Reflected Until Tab Switch", PlatformAffected.UWP)]
	public partial class Issue25913 : Shell
	{
		public Issue25913()
		{
			InitializeShell();
		}

		private void InitializeShell()
		{
			var tabBar = new TabBar();

			var tab1 = new Tab { Title = "Tab 1" };
			var topTab1Content = new ContentPage
			{
				Title = "TopTab1",
				Content = CreateControlPanel()
			};

			var topTab2Content = new ContentPage
			{
				Title = "TopTab2",
				Content = CreateControlPanel()
			};

			var topTab3Content = new ContentPage
			{
				Title = "TopTab3",
				Content = CreateControlPanel()
			};

			tab1.Items.Add(topTab1Content);
			tab1.Items.Add(topTab2Content);
			tab1.Items.Add(topTab3Content);

			var tab2 = new Tab
			{
				Title = "Tab 2",
				Items =
				{
					new ShellContent
					{
						Content = new ContentPage
						{
							Content = new Label { Text = "Tab 2 Content" }
						}
					}
				}
			};

			var tab3 = new Tab
			{
				Title = "Tab 3",
				Items =
				{
					new ShellContent
					{
						Content = new ContentPage
						{
							Content = new Label { Text = "Tab 3 Content" }
						}
					}
				}
			};

			tabBar.Items.Add(tab1);
			tabBar.Items.Add(tab2);
			tabBar.Items.Add(tab3);

			Items.Add(tabBar);
		}

		private View CreateControlPanel()
		{
			return new ScrollView
			{
				Content = new StackLayout
				{
					Spacing = 10,
					Padding = new Thickness(20),
					Children =
					{
						new Button
						{
							Text = "Hide Top Tab 3",
							Command = new Command(() =>
							{
								Items[0].Items[0].Items[2].IsVisible = false;
							}),
							AutomationId = "HideTop3"
						}
					}
				}
			};
		}
	}
}