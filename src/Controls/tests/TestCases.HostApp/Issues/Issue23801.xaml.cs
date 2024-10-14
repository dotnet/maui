namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23801, "Span GestureRecognizers don't work when the span is wrapped over two lines", PlatformAffected.Android)]
public partial class Issue23801 : ContentPage
{
	public Issue23801()
	{
		InitializeComponent();

		if (DeviceInfo.Platform == DevicePlatform.MacCatalyst || DeviceInfo.Platform == DevicePlatform.WinUI)
		{
			LabelSpan.Text = "https://en.wikipedia.org/wiki/Maui-Lable-Control-Span-Gesture-Test-This-Is-A-Long-Link-That-Will-Wrap-Over-Two-Lines-On-Windows-And-MacCatalyst-And-It-Should-Be-Tappable-At-The-End-Of-The-First-Line-And-Should-Display-A-Message";
		}
		else if (DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.Android)
		{
			LabelSpan.Text = "https://en.wikipedia.org/wiki/Maui-Lable-Control-Span-Gesture-Test";
		}
	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		testLabel.Text = "Label Span tapped at end of first line";
	}
}
