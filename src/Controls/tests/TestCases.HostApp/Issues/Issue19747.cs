using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19747, "Shell BackButtonBehavior TextOverride property not working as expected", PlatformAffected.Android)]
public class Issue19747 : Shell
{
	public Issue19747()
	{
		Items.Add(new ContentPage
		{
			Title = "Main Page",
			Content = new VerticalStackLayout
			{
				new Label
				{
					Text = "Navigate to test TextOverride",
					AutomationId = "MainPageLabel"
				},
				new Button
				{
					Text = "Navigate",
					AutomationId = "NavigateButton",
					Command = new Command(async () => await Navigation.PushAsync(new DetailPage()))
				}
			}
		});
	}

	public class DetailPage : ContentPage
	{
		public DetailPage()
		{
			Title = "Detail Page";
			
			// Set BackButtonBehavior with TextOverride
			Shell.SetBackButtonBehavior(this, new BackButtonBehavior
			{
				TextOverride = "Cancel"
			});

			Content = new VerticalStackLayout
			{
				new Label
				{
					Text = "Check if back button shows 'Cancel' without truncation",
					AutomationId = "DetailLabel"
				},
				new Label
				{
					Text = "On Android, the text should not be truncated (should show 'Cancel', not 'Ca...')",
					AutomationId = "InstructionLabel"
				}
			};
		}
	}
}
