namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 7311, "[Bug] [Android] Error back hardware button with Picker", PlatformAffected.Android)]
public class Issue7311 : TestContentPage
{
	const string FirstPickerItem = "Uno";
	const string PickerId = "CaptainPickard";
	readonly string[] _items = { FirstPickerItem, "Dos", "Tres" };

	protected override void Init()
	{
		var picker = new Picker
		{
			ItemsSource = _items,
			AutomationId = PickerId
		};

		Content = new StackLayout()
		{
			new Label()
			{
				Text = "Open Picker. Click hardware back button to close picker. Click hardware button a second time and it should navigate back to gallery"
			},
			picker
		};
	}
}