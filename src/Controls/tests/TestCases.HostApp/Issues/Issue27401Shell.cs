namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "27401Shell", "Shell.PopToRootOnTabReselect", PlatformAffected.Android | PlatformAffected.iOS)]
	public class Issue27401Shell : TestShell
	{
		protected override void Init()
		{
			SetPopToRootOnTabReselect(this, true);
			var contentPage = AddBottomTab("Tab 1");
			Items[0].CurrentItem.AutomationId = "Tab1AutomationId";
			AddBottomTab("Ignore Me");

			contentPage.Content = new Button
			{
				Text = "Deep 0",
				AutomationId = "Deep0Button",
				VerticalOptions = LayoutOptions.Center,
				Command = new Command(() => Navigation.PushAsync(new ContentPage()
				{
					Content = new Button()
					{
						Text = "Deep 1",
						AutomationId = "Deep1Button",
						VerticalOptions = LayoutOptions.Center,
						Command = new Command(() => Navigation.PushAsync(new ContentPage()
						{
							Content = new Button()
							{
								Text = "Deep 2",
								VerticalOptions = LayoutOptions.Center,
								AutomationId = "Deep2Label"
							}
						}))
					}
				}))
			};
		}
	}
}