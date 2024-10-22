namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Github, 1614, "iOS 11 prevents InputAccessoryView from showing in landscape mode", PlatformAffected.iOS)]
public class Issue1614 : TestContentPage
{
	protected override void Init()
	{
		var stackLayout = new StackLayout();
		var picker = new Picker
		{
			AutomationId = "Picker"
		};
		var datePicker = new DatePicker
		{
			AutomationId = "DatePicker"
		};
		var timePicker = new TimePicker
		{
			AutomationId = "TimePicker"
		};

		stackLayout.Children.Add(picker);
		stackLayout.Children.Add(datePicker);
		stackLayout.Children.Add(timePicker);

		Content = stackLayout;
	}
}