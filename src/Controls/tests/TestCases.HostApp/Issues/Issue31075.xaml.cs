namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31075, "MediaPicker.CapturePhotoAsync / CaptureVideoAsync causes modal page to dismiss unexpectedly", PlatformAffected.iOS)]
public partial class Issue31075 : ContentPage
{
	public Issue31075()
	{
		InitializeComponent();
	}

	async void OnOpenModalClicked(object sender, System.EventArgs e)
	{
		try
		{
			var modalPage = new Issue31075Modal();
			modalPage.ModalClosed += OnModalClosed;
			await Navigation.PushModalAsync(modalPage);
			StatusLabel.Text = "Modal opened";
		}
		catch (Exception ex)
		{
			StatusLabel.Text = $"Error: {ex.Message}";
		}
	}

	void OnModalClosed(object sender, EventArgs e)
	{
		StatusLabel.Text = "Modal closed successfully";
	}
}