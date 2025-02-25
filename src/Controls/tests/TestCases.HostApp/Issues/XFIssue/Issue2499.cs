namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 2499, "Binding Context set to Null in Picker", PlatformAffected.All)]
public class Issue2499 : TestContentPage
{
	protected override void Init()
	{
		var _picker = new Picker()
		{
			ItemsSource = new List<string> { "cat", "mouse", "rabbit" },
			AutomationId = "picker",
		};
		_picker.SelectedIndexChanged += (_, __) => _picker.ItemsSource = null;

		Content = new StackLayout()
		{
			Children =
			{
				_picker
			}
		};
	}
}