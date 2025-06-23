namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28416, "Crash in drag-n-drop with dragged element destroyed before drop is completed", PlatformAffected.iOS)]
public partial class Issue28416 : ContentPage
{
	public Issue28416()
	{
		InitializeComponent();
	}

	private void DropGestureRecognizer_Drop(object sender, DropEventArgs e)
	{
		stackLayout.Remove(dragObject);

		// This causes the crash.
		dragObject.Handler = null;

		stackLayout.Add(new Label { AutomationId = "DropResult", Text = "Dropped!" });
	}

	private void DragGestureRecognizer_DropCompleted(object sender, DropCompletedEventArgs e)
	{
		stackLayout.Add(new Label { AutomationId = "DropCompletedResult", Text = "Completed!" });
	}
}
