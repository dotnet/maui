namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 3139, "DisplayActionSheet is hiding behind Dialogs", PlatformAffected.UWP)]
public class Issue3139 : TestContentPage
{
	protected override async void Init()
	{
		var statusLabel = new Label()
		{
			FontSize = 40,
			TextColor = Colors.White
		};
		Content = new StackLayout()
		{
			Children = {
				statusLabel,
				new Label {
					Text = "Pop-ups should appear on top of the dialog. And it's got any button pressed.",
					TextColor = Colors.Yellow
				}
			}
		};

		var alertTask = DisplayAlert("AlertDialog", "Close me", "Close");
		await Task.Delay(200);
		var result1 = await DisplayActionSheet("ActionSheet", "Also Yes", "Click Yes", "Yes", "Yes Yes") ?? string.Empty;
		var result2 = await Application.Current.MainPage.DisplayActionSheet("Main page ActionSheet", "Again Yes", "Click Yes", "Yes", "Yes Yes") ?? string.Empty;
		var testPassed = result1.Contains("Yes", StringComparison.OrdinalIgnoreCase) && result2.Contains("Yes", StringComparison.OrdinalIgnoreCase) && !alertTask.IsCompleted;
		statusLabel.Text = "Test " + (testPassed ? "passed" : "failed");
		BackgroundColor = !testPassed ? Colors.DarkRed : Colors.DarkGreen;
		await alertTask;
	}
}