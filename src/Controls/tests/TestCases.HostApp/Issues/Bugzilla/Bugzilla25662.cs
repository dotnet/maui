namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Bugzilla, 25662, "Setting IsEnabled does not disable SwitchCell")]
public class Bugzilla25662 : TestContentPage
{
	class MySwitch : SwitchCell
	{
		public MySwitch()
		{
			IsEnabled = false;
			SetBinding(SwitchCell.TextProperty, new Binding("."));
			SetBinding(SwitchCell.AutomationIdProperty, new Binding("."));
			OnChanged += (sender, e) => Text = "FAIL";
		}
	}

	protected override void Init()
	{
		var list = new ListView
		{
			ItemsSource = new[] {
				"One", "Two", "Three"
			},
			ItemTemplate = new DataTemplate(typeof(MySwitch))
		};

		Content = list;
	}
}
