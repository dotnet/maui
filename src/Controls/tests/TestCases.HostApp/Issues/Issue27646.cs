namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27646, "AdaptiveTrigger not firing when changing window width programmatically only", PlatformAffected.UWP)]
public class Issue27646 : ContentPage
{
	Label indicatorLabel;
	Label statusLabel;

	public Issue27646()
	{
		var stackLayout = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10
		};

		var instructionLabel = new Label
		{
			Text = "Click button to resize window. Label text should change at 600px window width.",
			AutomationId = "InstructionLabel"
		};
		stackLayout.Add(instructionLabel);

		statusLabel = new Label
		{
			Text = "Window width: Unknown",
			AutomationId = "StatusLabel",
			FontAttributes = FontAttributes.Bold
		};
		stackLayout.Add(statusLabel);

		var resizeButton = new Button
		{
			Text = "Resize Window",
			AutomationId = "ResizeButton"
		};
		resizeButton.Clicked += OnResizeClicked;
		stackLayout.Add(resizeButton);

		indicatorLabel = new Label
		{
			Text = "Initial",
			WidthRequest = 200,
			HeightRequest = 100,
			HorizontalTextAlignment = TextAlignment.Center,
			VerticalTextAlignment = TextAlignment.Center,
			AutomationId = "IndicatorLabel"
		};
		stackLayout.Add(indicatorLabel);

		Content = stackLayout;

		VisualStateManager.SetVisualStateGroups(indicatorLabel, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				Name = "WindowWidthStates",
				States =
				{
					new VisualState
					{
						Name = "Narrow",
						StateTriggers =
						{
							new AdaptiveTrigger { MinWindowWidth = 0 }
						},
						Setters =
						{
							new Setter { Property = Label.TextProperty, Value = "Narrow Window" }
						}
					},
					new VisualState
					{
						Name = "Wide",
						StateTriggers =
						{
							new AdaptiveTrigger { MinWindowWidth = 600 }
						},
						Setters =
						{
							new Setter { Property = Label.TextProperty, Value = "Wide Window" }
						}
					}
				}
			}
		});
	}

	void OnResizeClicked(object sender, EventArgs e)
	{
		if (Window is not null)
		{
			double newWidth = Window.Width >= 600 ? 550 : 650;
			Window.Width = newWidth;

			statusLabel.Text = $"Window width: {newWidth}px";
		}
	}
}
