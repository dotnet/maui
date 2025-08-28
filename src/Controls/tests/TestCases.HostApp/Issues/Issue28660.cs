namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28660, "Label text gets cropped when a width request is specified on the label inside a VerticalStackLayout", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue28660 : ContentPage
{
	public Issue28660()
	{
		Grid gridLayout = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },

			},
		};

		var label = new Label()
		{
			Text = "At any time, but not later than one month before the expiration date.",
			AutomationId = "WithoutExplicitLabelSize",
			FontSize = 16,
		};

		VerticalStackLayout layoutWithExplicitWidth = new VerticalStackLayout
		{
			WidthRequest = 350,
			Children =
			{
				new Label
				{
					Text="With Background Color",
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
				},
				new Label
				{
					Text="At any time, but not later than one month before the expiration date.",
					WidthRequest=100.94,
					FontSize = 16,
					Background = Colors.Pink,
					AutomationId = "WithBackgroundColorLabel",
				},
				new Label
				{
					Text="With explicit width request",
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,

				},
				new Label
				{
					Text="At any time, but not later than one month before the expiration date.",
					WidthRequest=100.94,
					FontSize = 16,
					AutomationId = "ExplicitLabelSize",
				},
				new Label
				{
					Text="With explicit height request",
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,

				},
				new Label
				{
					Text="At any time, but not later than one month before the expiration date.",
					HeightRequest=100,
					FontSize = 16,
					AutomationId = "ExplicitLabelSize",
				},
			}
		};

		VerticalStackLayout layoutWithoutExplicitWidth = new VerticalStackLayout
		{
			Children =
			{
				new Label
				{
					Text="Without explicit width and height request",
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
				},
				label,
				new Button
				{
					Text = "Change label text",
					AutomationId = "ChangeLabelTextButton",
					FontSize = 16,
					FontAttributes = FontAttributes.Bold,
					Command = new Command(() =>
					{
						label.Text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed eiusmod tempor incidunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquid ex ea commodi consequat. Quis aute iure reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint obcaecat cupiditat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";
					})
				}
			}
		};
		gridLayout.SetRow(layoutWithExplicitWidth, 0);
		gridLayout.Children.Add(layoutWithExplicitWidth);
		gridLayout.SetRow(layoutWithoutExplicitWidth, 1);
		gridLayout.Children.Add(layoutWithoutExplicitWidth);
		Content = new ScrollView
		{
			Content = gridLayout,
			AutomationId = "ScrollView"
		};
	}
}