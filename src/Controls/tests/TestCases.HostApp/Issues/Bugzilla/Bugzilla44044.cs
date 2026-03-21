using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using Button = Microsoft.Maui.Controls.Button;

namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 44044, "TabbedPage steals swipe gestures", PlatformAffected.Android)]
public class Bugzilla44044 : TestTabbedPage
{
	string _btnToggleSwipe = "btnToggleSwipe";
	string _btnDisplayAlert = "btnDisplayAlert";

	protected override void Init()
	{
		Children.Add(new ContentPage()
		{
			Title = "Page 1",
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "Click to Toggle Swipe Paging",
						Command = new Command(() => On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(!On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().IsSwipePagingEnabled())),
						AutomationId = _btnToggleSwipe
					}
				}
			}
		});

		Children.Add(new ContentPage()
		{
			Title = "Page 2",
			Content = new StackLayout
			{
				Children =
				{
					new Button
					{
						Text = "Click to DisplayAlert",
						Command = new Command(() => DisplayAlertAsync("Page 2", "Message", "Cancel")),
						AutomationId = _btnDisplayAlert
					}
				}
			}
		});
	}
}