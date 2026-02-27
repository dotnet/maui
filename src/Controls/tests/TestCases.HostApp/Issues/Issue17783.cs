using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17783, "Pasting long text when the editor has a max length does nothing", PlatformAffected.All)]
public partial class Issue17783 : ContentPage
{
	public Issue17783()
	{
		var stackLayout = new VerticalStackLayout();

		var entry = new Entry
		{
			AutomationId = "Entry",
			MaxLength = 5
		};

		var editor = new Editor
		{
			AutomationId = "Editor",
			MaxLength = 5
		};

		var pasteToEntryButton = new Button
		{
			Text = "Paste to entry",
			AutomationId = "PasteToEntryButton"
		};
		pasteToEntryButton.Clicked += (s, e) => entry.Text = "Hello, Maui!";

		var pasteToEditorButton = new Button
		{
			Text = "Paste to editor",
			AutomationId = "PasteToEditorButton"
		};
		pasteToEditorButton.Clicked += (s, e) => editor.Text = "Hello, Maui!";

		stackLayout.Children.Add(entry);
		stackLayout.Children.Add(editor);
		stackLayout.Children.Add(pasteToEntryButton);
		stackLayout.Children.Add(pasteToEditorButton);

		Content = stackLayout;
	}
}