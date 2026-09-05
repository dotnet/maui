namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23921, "If a tap closes a SwipeView, the tap should not reach the children", PlatformAffected.Android)]
public class Issue23921 : ContentPage
{
	public Issue23921()
	{
		var buttonOne = new Button
		{
			Text = "tap me!",
			AutomationId = "buttonOne"
		};
		buttonOne.Clicked += Button_Clicked;

		var swipeItemViewOne = new SwipeItemView
		{
			Content = new Label
			{
				Text = "Action",
				AutomationId = "swipeItemOne",
				BackgroundColor = Colors.LightGreen,
				Padding = 20
			}
		};

		var swipeOne = new SwipeView
		{
			AutomationId = "swipeOne",
			Content = buttonOne
		};
		swipeOne.RightItems.Add(swipeItemViewOne);

		var buttonTwo = new Button
		{
			Text = "tap me!",
			AutomationId = "buttonTwo"
		};
		buttonTwo.Clicked += Button_Clicked;

		var swipeItemViewTwo = new SwipeItemView
		{
			Content = new Label
			{
				Text = "Action",
				AutomationId = "swipeItemTwo",
				BackgroundColor = Colors.LightGreen,
				Padding = 20
			}
		};

		var swipeTwo = new SwipeView
		{
			AutomationId = "swipeTwo",
			Content = buttonTwo
		};
		swipeTwo.RightItems.Add(swipeItemViewTwo);

		Content = new VerticalStackLayout
		{
			Children = { swipeOne, swipeTwo }
		};
	}

	void Button_Clicked(object sender, EventArgs e)
	{
		if (sender is Button button)
			button.Text = "tapped";
	}
}
