namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 0, "Entry control text input and clear test", PlatformAffected.All)]
public partial class IssueEntryTest : ContentPage
{
	public IssueEntryTest()
	{
		InitializeComponent();
		
		// Update the label when text changes in the Entry
		TestEntry.TextChanged += OnEntryTextChanged;
	}
	
	private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.NewTextValue))
		{
			ResultLabel.Text = "Entry is empty";
		}
		else
		{
			ResultLabel.Text = $"Entry text: {e.NewTextValue}";
		}
	}
	
	private void OnClearButtonClicked(object sender, EventArgs e)
	{
		TestEntry.Text = string.Empty;
	}
}
