namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 4714, "SingleTapGesture called once on DoubleTap", PlatformAffected.UWP)]
public class Issue4714 : TestContentPage
{
	const string InitialText = "ClickMeToIncrement";

	public Command TapCommand { get; set; }

	protected override void Init()
	{
		int i = 0;

		var tapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 1 };

		tapGesture.SetBinding(TapGestureRecognizer.CommandProperty, "TapCommand");

		var label = new Label()
		{
			AutomationId = InitialText,
			HorizontalOptions = LayoutOptions.Center,
			Text = InitialText,
			GestureRecognizers =
			{
				tapGesture
			}
		};

		TapCommand = new Command(() =>
		{
			i++;
			label.Text = $"{InitialText}: {i}";
		});

		var stackLayout = new StackLayout();
		stackLayout.Add(label);

		Content = new ContentView()
		{
			Content = stackLayout
		};

		BindingContext = this;
	}
}
