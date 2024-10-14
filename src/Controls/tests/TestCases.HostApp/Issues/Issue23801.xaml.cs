namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23801, "Span GestureRecognizers don't work when the span is wrapped over two lines", PlatformAffected.Android)]
public partial class Issue23801 : ContentPage
{
	public Issue23801()
	{
		InitializeComponent();
	}

	private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
	{
		testLabel.Text = "Label Span tapped at end of first line";
	}
}
