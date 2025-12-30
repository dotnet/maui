namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 43469, "Calling DisplayAlert twice in WinRT causes a crash", PlatformAffected.WinRT)]

public class Bugzilla43469 : TestContentPage
{
	const string kButtonText = "Click to call DisplayAlert six times. Click as fast as you can to close them as they popup to ensure it doesn't crash.";
	protected override void Init()
	{
		var button = new Button { Text = kButtonText, AutomationId = "kButton" };

		button.Clicked += async (sender, args) =>
		{
			await DisplayAlertAsync("First", "Text", "OK", "Cancel");
			await DisplayAlertAsync("Second", "Text", "OK", "Cancel");
			await DisplayAlertAsync("Three", "Text", "OK", "Cancel");
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
			Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				await DisplayAlertAsync("Fourth", "Text", "OK", "Cancel");
			}));
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
			Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				await DisplayAlertAsync("Fifth", "Text", "OK", "Cancel");
			}));
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
			Device.BeginInvokeOnMainThread(new Action(async () =>
			{
				await DisplayAlertAsync("Sixth", "Text", "OK", "Cancel");
			}));
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
		};

		Content = button;
	}
}