namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 36955, "[iOS] ViewCellRenderer.UpdateIsEnabled referencing null object", PlatformAffected.iOS)]
public class Bugzilla36955 : TestContentPage
{
	protected override void Init()
	{
		var ts = new TableSection();
		var tr = new TableRoot { ts };
		var tv = new TableView(tr);

		var sc = new SwitchCell
		{
			Text = "Toggle switch; nothing should crash",
			AutomationId = "Switch"
		};

		var button = new Button
		{
			AutomationId = "Button"
		};
		button.SetBinding(Button.TextProperty, new Binding("On", source: sc));

		var vc = new ViewCell
		{
			View = button
		};
		vc.SetBinding(Cell.IsEnabledProperty, new Binding("On", source: sc));

		ts.Add(sc);
		ts.Add(vc);

		Content = tv;
	}
}
