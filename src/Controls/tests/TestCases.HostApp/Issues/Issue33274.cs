namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33274, "Windows Maui Stepper is not clamped to minimum or maximum internally", PlatformAffected.WinRT)]
public class Issue33274 : TestContentPage
{
	protected override void Init()
	{
		var layout = new StackLayout { };

		Stepper Incrementstepper = new Stepper
		{
			AutomationId = "Maximumstepper",
			HorizontalOptions = LayoutOptions.Center,
			Increment = 1,
			Minimum = 0,
			Maximum = 1,
			Value = 1
		};

		Label Incrementlabel = new Label
		{
			AutomationId = "Maximumlabel",
			HorizontalOptions = LayoutOptions.Center,
			FontSize = 32
		};
		Incrementlabel.SetBinding(Label.TextProperty, new Binding("Value", source: Incrementstepper));

		Stepper Decrementstepper = new Stepper
		{
			AutomationId = "Minimumstepper",
			HorizontalOptions = LayoutOptions.Center,
			Increment = 1,
			Maximum = 1,
			Minimum = 0,
			Value = 0
		};

		Label Decrementlabel = new Label
		{
			AutomationId = "Minimumlabel",
			HorizontalOptions = LayoutOptions.Center,
			FontSize = 32
		};
		Decrementlabel.SetBinding(Label.TextProperty, new Binding("Value", source: Decrementstepper));

		layout.Children.Add(Incrementstepper);
		layout.Children.Add(Incrementlabel);
		layout.Children.Add(Decrementstepper);
		layout.Children.Add(Decrementlabel);

		Content = layout;
	}
}