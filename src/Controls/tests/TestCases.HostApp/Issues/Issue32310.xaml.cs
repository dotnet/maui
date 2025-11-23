namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32310, "App hangs if PopModalAsync is called after PushModalAsync with single await Task.Yield()", PlatformAffected.Android)]
public partial class Issue32310 : ContentPage
{
	public Issue32310()
	{
		InitializeComponent();
	}

	async void OnTestButtonClicked(object sender, EventArgs e)
	{
		try
		{
			StatusLabel.Text = "Testing...";

			// This is the exact scenario from the bug report:
			// PushModalAsync followed by PopModalAsync with only one Task.Yield in between
			// Without the fix, this would hang the app with a blank screen
			await Navigation.PushModalAsync(new ContentPage() { Content = new Label() { Text = "Modal Content" } }, false);

			await Task.Yield();

			await Navigation.PopModalAsync();

			// If we get here, the test passed
			StatusLabel.Text = "Success";
		}
		catch (Exception ex)
		{
			StatusLabel.Text = $"Failed: {ex.Message}";
		}
	}
}
