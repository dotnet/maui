namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 28330, "Stepper allows to increment when value equals to maximum", PlatformAffected.All)]
public class Issue28330 : TestContentPage
{
	protected override void Init()
	{
		var layout = new StackLayout { };

		Stepper Incrementstepper = new Stepper
		{
			AutomationId = "Incrementstepper",
			HorizontalOptions = LayoutOptions.Center,
			Increment = 1,
			Minimum = 1,
			Maximum = 1,
			Value = 1
		};

		Label Incrementlabel = new Label
		{
			AutomationId = "Incrementlabel",
			HorizontalOptions = LayoutOptions.Center,
			FontSize = 32
		};
		Incrementlabel.SetBinding(Label.TextProperty, new Binding("Value", source: Incrementstepper));

		Stepper Decrementstepper = new Stepper
		{
			AutomationId = "Decrementstepper",
			HorizontalOptions = LayoutOptions.Center,
			Increment = 1,
			Maximum = 1,
			Minimum = 1,
			Value = 1
		};

		Label Decrementlabel = new Label
		{
			AutomationId = "Decrementlabel",
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