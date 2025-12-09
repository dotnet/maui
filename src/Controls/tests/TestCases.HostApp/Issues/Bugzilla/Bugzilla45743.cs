namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 45743, "[iOS] Calling DisplayAlert via BeginInvokeOnMainThread blocking other calls on iOS", PlatformAffected.iOS)]
public class Bugzilla45743 : TestNavigationPage
{
	protected override void Init()
	{
		PushAsync(new ContentPage
		{
			Content = new StackLayout
			{
				AutomationId = "Page1",
				Children =
				{
					new Label { Text = "Page 1" }
				}
			}
		});

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.BeginInvokeOnMainThread(async () =>
		{
			await DisplayAlertAsync("Title", "Message", "Accept", "Cancel");
		});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.BeginInvokeOnMainThread(async () =>
		{
			await PushAsync(new ContentPage
			{
				AutomationId = "Page2",
				Content = new StackLayout
				{
					Children =
					{
						new Label { Text = "Page 2", AutomationId = "Page 2" }
					}
				}
			});
		});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.BeginInvokeOnMainThread(async () =>
		{
			await DisplayAlertAsync("Title 2", "Message", "Accept", "Cancel");
		});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.BeginInvokeOnMainThread(async () =>
		{
			await DisplayActionSheetAsync("ActionSheet Title", "Cancel", "Close", new string[] { "Test", "Test 2" });
		});
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
	}
}