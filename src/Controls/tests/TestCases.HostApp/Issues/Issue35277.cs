namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35277, "COMException when restoring a page content after swapping it out", PlatformAffected.UWP)]
public class Issue35277 : ContentPage
{
	ScrollView _originalScrollView;

	public Issue35277()
	{
		var swapButton = new Button
		{
			Text = "Swap and Restore",
			HorizontalOptions = LayoutOptions.Center,
			AutomationId = "SwapAndRestoreButton"
		};
		swapButton.Clicked += OnSwapAndRestoreClicked;

		_originalScrollView = new ScrollView
		{
			Content = new VerticalStackLayout
			{
				Spacing = 20,
				VerticalOptions = LayoutOptions.Center,
				Children =
				{
					swapButton,
					new Label
					{
						Text = "Original Content",
						HorizontalOptions = LayoutOptions.Center,
						AutomationId = "OriginalLabel"
					}
				}
			}
		};

		Content = _originalScrollView;
	}

	void OnSwapAndRestoreClicked(object sender, EventArgs e)
	{
		var savedScrollView = _originalScrollView;
		Content = new Label
		{
			Text = "Temporary Content",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			AutomationId = "TemporaryLabel"
		};

		Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
		{
			Content = savedScrollView;
		});
	}
}
