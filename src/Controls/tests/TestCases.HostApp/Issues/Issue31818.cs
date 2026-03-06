namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31818, "The MauiAsset does not work when both LogicalName and Link are specified", PlatformAffected.UWP)]
public class Issue31818 : ContentPage
{
	Label statusLabel;

	public Issue31818()
	{
		Label resultLabel = new Label
		{
			Text = "Test passes if the MauiAsset file with both LogicalName and Link can be found; otherwise, it fails."
		};

		statusLabel = new Label
		{
			AutomationId = "Issue31818_StatusLabel",
			Text = "Not Started"
		};

		Button checkFileBtn = new Button
		{
			AutomationId = "CheckFileButton",
			Text = "Check if file exists",
			HorizontalOptions = LayoutOptions.Fill
		};
		checkFileBtn.Clicked += OnCheckFileClicked;

		Content = new VerticalStackLayout
		{
			Padding = 30,
			Spacing = 25,
			Children =
			{
				resultLabel,
				checkFileBtn,
				statusLabel
			}
		};
	}

	async void OnCheckFileClicked(object sender, EventArgs e)
	{
		if (await FileSystem.Current.AppPackageFileExistsAsync("test.txt"))
		{
			statusLabel.Text = "Success";
		}
		else
		{
			statusLabel.Text = "Failure";
		}
	}
}