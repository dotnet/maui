namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 23329, "Entry select all text on refocus does not work on WinUI", PlatformAffected.Windows)]
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
