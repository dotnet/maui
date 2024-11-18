namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Bugzilla, 38112, "Switch becomes reenabled when previous ViewCell is removed from TableView", PlatformAffected.Android)]
public class Bugzilla38112 : TestContentPage
{
	bool _removed;
	protected override void Init()
	{
		var layout = new StackLayout();
		var button = new Button { Text = "Click", AutomationId = "Click" };
		var tablesection = new TableSection { Title = "Switches" };
		var tableview = new TableView { Intent = TableIntent.Form, Root = new TableRoot { tablesection } };
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		var viewcell1 = new ViewCell
		{
			View = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Orientation = StackOrientation.Horizontal,
				Children = {
					new Label { Text = "Switch 1", HorizontalOptions = LayoutOptions.StartAndExpand },
					new Switch { AutomationId = "switch1", HorizontalOptions = LayoutOptions.End, IsToggled = true }
				}
			}
		};
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		var viewcell2 = new ViewCell
		{
			View = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Orientation = StackOrientation.Horizontal,
				Children = {
					new Label { Text = "Switch 2", HorizontalOptions = LayoutOptions.StartAndExpand },
					new Switch { AutomationId = "switch2", HorizontalOptions = LayoutOptions.End, IsToggled = true }
				}
			}
		};
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
		Label label = new Label { Text = "Switch 3", HorizontalOptions = LayoutOptions.StartAndExpand, AutomationId = "resultlabel" };
#pragma warning restore CS0618 // Type or member is obsolete
		Switch switchie = new Switch { AutomationId = "switch3", HorizontalOptions = LayoutOptions.End, IsToggled = true, IsEnabled = false };
		switchie.Toggled += (sender, e) =>
		{
			label.Text = "FAIL";
		};
#pragma warning disable CS0618 // Type or member is obsolete
		var viewcell3 = new ViewCell
		{
			View = new StackLayout
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				Orientation = StackOrientation.Horizontal,
				Children = {
					label,
					switchie,
				}
			}
		};
#pragma warning restore CS0618 // Type or member is obsolete

		tablesection.Add(viewcell1);
		tablesection.Add(viewcell2);
		tablesection.Add(viewcell3);

		button.Clicked += (sender, e) =>
		{
			if (_removed)
				tablesection.Insert(1, viewcell2);
			else
				tablesection.Remove(viewcell2);

			_removed = !_removed;
		};

		layout.Children.Add(button);
		layout.Children.Add(tableview);

		Content = layout;
	}
}
