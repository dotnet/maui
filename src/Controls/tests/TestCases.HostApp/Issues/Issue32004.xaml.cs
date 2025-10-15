namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32004, "Entry crashes when setting Text in TextChanged event handler", PlatformAffected.Android)]
public partial class Issue32004 : ContentPage
{
	public Issue32004()
	{
		InitializeComponent();
	}

	private void Quantity_TextChanged(object sender, TextChangedEventArgs e)
	{
		// This reproduces the crash: when text is cleared (empty/null),
		// setting the Text property causes a re-entrant TextWatcher callback
		// while EmojiCompat is processing, leading to "end should be < than charSequence length" crash
		if (string.IsNullOrEmpty(e.NewTextValue))
		{
			((Entry)sender).Text = "0";
			StatusLabel.Text = "Status: Reset to 0";
		}
		else
		{
			StatusLabel.Text = $"Status: Text is '{e.NewTextValue}'";
		}
	}
}
