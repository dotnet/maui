namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.ManualTest, "EntrySelectionTest", "Entry select all text issue", PlatformAffected.UWP)]
public partial class EntrySelectionTest : ContentPage
{
	public EntrySelectionTest()
	{
		InitializeComponent();
	}

	void OnTextBoxFocused(object sender, FocusEventArgs e)
	{
		if (TextBox.Text != null)
		{
			TextBox.CursorPosition = 0;
			TextBox.SelectionLength = TextBox.Text.Length;
		}
	}
}
