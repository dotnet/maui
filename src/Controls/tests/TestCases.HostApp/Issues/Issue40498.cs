namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 40498, "Overflow menu icons for Secondary toolbar items are blurry because GetPlatformMenuImage renders FontImageSource at 1x", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue40498 : NavigationPage
{
	public Issue40498() : base(new Issue40498Page())
	{
	}

	public class Issue40498Page : ContentPage
	{
		public Issue40498Page()
		{
			Title = "Menu icons";

			ToolbarItems.Add(new ToolbarItem
			{
				Text = "Info",
				AutomationId = "Issue40498_InfoItem",
				Order = ToolbarItemOrder.Secondary,
				IconImageSource = new FontImageSource { Glyph = "ℹ", Size = 22 }
			});
			ToolbarItems.Add(new ToolbarItem
			{
				Text = "Share",
				Order = ToolbarItemOrder.Secondary,
				IconImageSource = new FontImageSource { Glyph = "↗", Size = 22 }
			});

			Content = new Label
			{
				Text = "Open the overflow menu. The test passes if the menu icons are sharp, not blurry.",
				AutomationId = "Issue40498_DescriptionLabel",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};
		}
	}
}
