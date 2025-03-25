namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 18204, "[iOS] Drawing of Borders lags behind other elements creating bizarre overlaps and glitches", PlatformAffected.iOS)]

public partial class Issue18204 : ContentPage
{
	public Issue18204()
	{
		InitializeComponent();
	}

	private void ChangeSizeClicked(object sender, EventArgs e)
	{
		var button = TheButton;
		button.CancelAnimations();
		var targetHeight = button.HeightRequest == 200.0 ? 400.0 : 200.0;
		button.Animate("Height", new Animation(v => button.HeightRequest = v, button.Height, targetHeight, Easing.Linear));
	}

	private void ShowHideClicked(object sender, EventArgs e)
	{
		var button = TheOtherButton;
		button.IsVisible = !button.IsVisible;
	}
}
