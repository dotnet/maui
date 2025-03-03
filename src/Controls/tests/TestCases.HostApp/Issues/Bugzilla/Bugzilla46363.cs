namespace Maui.Controls.Sample.Issues;


[Issue(IssueTracker.Bugzilla, 46363, "TapGestureRecognizer blocks List View Context Actions", PlatformAffected.Android)]
public class Bugzilla46363 : TestContentPage
{
	const string Target = "Two";
	const string ContextAction = "Context Action";
	const string TapSuccess = "Tap Success";
	const string TapFailure = "Tap command executed more than once";
	const string ContextSuccess = "Context Menu Success";
	const string Testing = "Testing";

	static Command s_tapCommand;
	static Command s_contextCommand;

	protected override void Init()
	{
		var list = new List<string> { "One", Target, "Three", "Four" };

		var lv = new ListView
		{
			ItemsSource = list,
			ItemTemplate = new DataTemplate(typeof(_46363Template))
		};

		var instructions = new Label();
		var result = new Label { AutomationId = "TestingLabel", Text = Testing };

		s_tapCommand = new Command(() =>
		{
			if (result.Text == TapSuccess || result.Text == TapFailure)
			{
				// We want this test to fail if the tap command is executed more than once
				result.Text = TapFailure;
			}
			else
			{
				result.Text = TapSuccess;
			}
		});

		s_contextCommand = new Command(() =>
		{
			result.Text = ContextSuccess;
		});

		var layout = new StackLayout { VerticalOptions = LayoutOptions.Fill, HorizontalOptions = LayoutOptions.Fill };

		layout.Children.Add(instructions);
		layout.Children.Add(result);
		layout.Children.Add(lv);

		Content = layout;
	}


	class _46363Template : ViewCell
	{
		public _46363Template()
		{
			var label = new Label();
			label.SetBinding(Label.TextProperty, ".");
			View = new StackLayout { Children = { label } };

			ContextActions.Add(new MenuItem
			{
				Text = ContextAction,
				Command = s_contextCommand
			});

			View.GestureRecognizers.Add(new TapGestureRecognizer
			{
				Command = s_tapCommand
			});
		}
	}
}