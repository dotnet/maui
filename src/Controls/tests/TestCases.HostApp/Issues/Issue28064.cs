namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28064, "TapGestureRecognizer on ScrollView background does not fire on Android", PlatformAffected.Android)]
public class Issue28064 : ContentPage
{
	public Issue28064()
	{
		var statusLabel = new Label
		{
			Text = "Tap the brown background",
			AutomationId = "StatusLabel"
		};

		var childStatusLabel = new Label
		{
			Text = "Tap a child label",
			AutomationId = "ChildStatusLabel"
		};

		var child1 = new Label
		{
			BackgroundColor = Colors.LightPink,
			WidthRequest = 100,
			HeightRequest = 80,
			Text = "Child1",
			AutomationId = "Child1Label"
		};
		var childTapRecognizer = new TapGestureRecognizer();
		childTapRecognizer.Tapped += (s, e) =>
		{
			childStatusLabel.Text = "Child Tapped";
		};
		child1.GestureRecognizers.Add(childTapRecognizer);

		// ScrollView with Brown background — the area NOT covered by children is the tap target
		var scrollView = new ScrollView
		{
			HeightRequest = 80,
			BackgroundColor = Colors.Brown,
			Orientation = ScrollOrientation.Horizontal,
			AutomationId = "TheScrollView",
			Content = new HorizontalStackLayout
			{
				Children =
				{
					child1,
					new Label
					{
						BackgroundColor = Colors.LightGreen,
						WidthRequest = 100,
						HeightRequest = 80,
						Text = "Child2",
						AutomationId = "Child2Label"
					}
				}
			}
		};

		var tapRecognizer = new TapGestureRecognizer();
		tapRecognizer.Tapped += (s, e) =>
		{
			statusLabel.Text = "ScrollView Tapped";
		};
		scrollView.GestureRecognizers.Add(tapRecognizer);

		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Padding = 10,
			Children =
			{
				statusLabel,
				childStatusLabel,
				scrollView
			}
		};
	}
}
