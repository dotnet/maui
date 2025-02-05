namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23158, "Respect Entry.ClearButtonVisibility on Windows", PlatformAffected.UWP)]
public partial class Issue23158 : ContentPage
{
	public Issue23158()
	{
		InitializeComponent();
	}

	private void AddEntryButton_Clicked(object sender, EventArgs e)
	{
		Entry entry = new Entry()
		{
			Text = "Some Text",
			AutomationId = "Entry3"
		};

		Entry3Container.Children.Add(entry);

		// Intentionally, after the entry is added to its layout container.
		entry.ClearButtonVisibility = ClearButtonVisibility.Never;
		entry.Focus();
	}
}