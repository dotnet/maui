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

		editor.SetBinding(Editor.TextProperty, "Text");
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
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
		Device.BeginInvokeOnMainThread(GarbageCollectionHelper.Collect);
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
	}
}
