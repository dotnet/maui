namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, "15301Shell", "Shell.TabActiveTapped", PlatformAffected.Android | PlatformAffected.iOS)]
public class Issue15301Shell : TestShell
{
	protected override void Init()
	{
		TabActiveTapped += (s, e) =>
		{
			Navigation.PopAsync();
		};

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
				Content = new Label()
				{
					Text = "Hello, World!",
					AutomationId = "SubpageLabel",
					VerticalOptions = LayoutOptions.Center,
				}
			}))
		};
	}
}
