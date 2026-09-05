namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 34273, "Rotating simulator makes Stepper and Label overlap", PlatformAffected.iOS)]
public class Issue34273 : NavigationPage
{
	public Issue34273() : base(new MainPage())
	{
	}

	public class MainPage : ContentPage
	{
		public MainPage()
		{
			var stepper = new Stepper
			{
				AutomationId = "CursorStepper",
				Minimum = 0
			};

			var stepperValueLabel = new Label
			{
				AutomationId = "CursorPositionLabel",
				VerticalOptions = LayoutOptions.Center,
				VerticalTextAlignment = TextAlignment.Center
			};

			var editor = new Editor
			{
				AutomationId = "TestEditor",
				Text = "testing"
			};

			stepper.BindingContext = editor;
			stepper.SetBinding(Stepper.MaximumProperty, new Binding("Text.Length"));

			stepperValueLabel.BindingContext = stepper;
			stepperValueLabel.SetBinding(Label.TextProperty, new Binding("Value"));

			editor.BindingContext = stepper;
			editor.SetBinding(Editor.CursorPositionProperty, new Binding("Value", BindingMode.TwoWay));

			Content = new ScrollView
			{
				Content = new VerticalStackLayout
				{
					Children =
					{
						new Label { Text = "1. Click or tap within the editor below to place the cursor within the middle of the string 'testing'. (NOTE: On iOS, just tapping the entry will place the cursor at the beginning or end. You must touch and hold, and then move the cursor to position it if you want it in the middle of the string.)" },
						new Label { Text = "2. The test fails if the label next to the -/+ controls does not update to the index of the cursor position." },
						new Label { Text = "3. For example, if you place the cursor at the beginning of the editor, the label should display 0." },
						new Label { Text = "4. Play with the - and + buttons and observe if the cursor position within the editor changes." },
						new Label { Text = "5. If the focus has left the editor, press the Tab key to return focus to the editor." },
						new Label { Text = "6. The test fails if the cursor position within the editor has not changed to reflect the index shown." },
						new HorizontalStackLayout
						{
							Children = { stepper, stepperValueLabel }
						},
						editor
					}
				}
			};
		}
	}
}
