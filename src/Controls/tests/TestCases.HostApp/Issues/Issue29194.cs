namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29194, "[Android][Label] Setting a Label visible after having had focus on a InputView will increase the Label's height", PlatformAffected.Android)]
public class Issue29194 : ContentPage
{
	public Issue29194()
	{
		var mySwitch = new Switch() { AutomationId = "Switch" };

		var label = new Label
		{
			Text = "Hello, World!",
			BackgroundColor = Colors.Red,
			AutomationId = "Label"
		};

		label.SetBinding(Label.IsVisibleProperty, new Binding(nameof(Switch.IsToggled), source: mySwitch));

		var verticalStack = new VerticalStackLayout
		{
			Children =
			{
				label,
				mySwitch,
				new UITestEntry() {AutomationId = "Entry",IsCursorVisible=false},
			}
		};

		var scrollView = new ScrollView
		{
			Content = verticalStack
		};

		Content = scrollView;

	}
}