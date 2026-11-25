using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21384, "InputTransparent has no effect on Windows", PlatformAffected.WinRT)]
public class Issue21384 : ContentPage
{
	public Issue21384()
	{
		var labelCounterButton = CreateCounterButton("CounterButton");
		var inputTransparentLabel = new Label
		{
			AutomationId = "InputTransparentLabel",
			BackgroundColor = Colors.Transparent,
			InputTransparent = true,
			Text = "Help"
		};

		var layoutCounterButton = CreateCounterButton("LayoutCounterButton");
		var inputTransparentLayout = new Grid
		{
			AutomationId = "InputTransparentLayout",
			CascadeInputTransparent = true,
			Clip = new EllipseGeometry(new Point(100, 30), 100, 30),
			InputTransparent = true,
			Children =
			{
				new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					Text = "Help",
					VerticalOptions = LayoutOptions.Center
				}
			}
		};

		Content = new VerticalStackLayout
		{
			Padding = new Thickness(30, 0),
			Spacing = 25,
			Children =
			{
				CreateOverlay(labelCounterButton, inputTransparentLabel),
				CreateOverlay(layoutCounterButton, inputTransparentLayout)
			}
		};
	}

	static Button CreateCounterButton(string automationId)
	{
		var clickCount = 0;
		var button = new Button
		{
			AutomationId = automationId,
			HorizontalOptions = LayoutOptions.Fill,
			Text = "Click me"
		};

		button.Clicked += (_, _) =>
		{
			clickCount++;
			button.Text = clickCount == 1
				? "Clicked 1 time"
				: $"Clicked {clickCount} times";
		};

		return button;
	}

	static AbsoluteLayout CreateOverlay(Button button, View inputTransparentOverlay)
	{
		AbsoluteLayout.SetLayoutBounds(inputTransparentOverlay, new Rect(0, 0, 1, 1));
		AbsoluteLayout.SetLayoutFlags(inputTransparentOverlay, AbsoluteLayoutFlags.All);

		var overlay = new AbsoluteLayout
		{
			HeightRequest = 60,
			HorizontalOptions = LayoutOptions.Fill
		};
		overlay.Add(button);
		overlay.Add(inputTransparentOverlay);

		return overlay;
	}
}
