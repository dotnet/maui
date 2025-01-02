namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 5500, "[iOS] Editor with material visuals value binding not working on physical device",
	PlatformAffected.iOS)]
public class Issue5500 : TestContentPage
{
	Editor editor;
	Entry entry;

	protected override void Init()
	{
		Visual = VisualMarker.Material;

		editor = new Editor();
		entry = new Entry();
		// On iOS, the app freezes. Changing the binding mode to one-way resolves the issue. It seems an infinite loop occurs when properties bind to each other.
		editor.SetBinding(Editor.TextProperty, "Text", mode: BindingMode.OneWay);
		editor.BindingContext = entry;
		editor.Placeholder = "Editor";
		editor.AutoSize = EditorAutoSizeOption.TextChanges;
		editor.AutomationId = "EditorAutomationId";

		entry.SetBinding(Entry.TextProperty, "Text");
		entry.BindingContext = editor;
		entry.Placeholder = "Entry";
		entry.AutomationId = "EntryAutomationId";

		Content = new StackLayout()
		{
			new Label(){ Text = "Typing into either text field should change the other field to match" },
			entry,
			editor
		};
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		MainThread.BeginInvokeOnMainThread(GarbageCollectionHelper.Collect);
	}
}
