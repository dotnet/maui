using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39450, "Button gradient background should respect CornerRadius", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue39450 : ContentPage
{
	public Issue39450()
	{
		BackgroundColor = Colors.White;

		var linearButton = CreateButton("Linear gradient", "LinearGradientButton", CreateLinearGradient());
		var radialButton = CreateButton("Radial gradient", "RadialGradientButton", new RadialGradientBrush
		{
			Center = new Point(0.5, 0.5),
			Radius = 0.5,
			GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Colors.CornflowerBlue, Offset = 0 },
				new GradientStop { Color = Colors.Navy, Offset = 1 }
			}
		});
		var stateButton = CreateButton("VisualState background", "VisualStateButton", new SolidColorBrush(Colors.CornflowerBlue));

		VisualStateManager.SetVisualStateGroups(stateButton, new VisualStateGroupList
		{
			new VisualStateGroup
			{
				Name = "CommonStates",
				States =
				{
					new VisualState { Name = "Normal" },
					new VisualState
					{
						Name = "Gradient",
						Setters =
						{
							new Setter { Property = Button.BackgroundProperty, Value = CreateLinearGradient() }
						}
					}
				}
			}
		});
		VisualStateManager.GoToState(stateButton, "Normal");

		var statusLabel = new Label
		{
			AutomationId = "StateLabel",
			Text = "Normal",
			TextColor = Colors.Black,
			HorizontalOptions = LayoutOptions.Center
		};
		var toggleButton = new Button
		{
			AutomationId = "ToggleGradientStateButton",
			Text = "Toggle gradient state"
		};
		bool gradientState = false;
		toggleButton.Clicked += (sender, args) =>
		{
			gradientState = !gradientState;
			var state = gradientState ? "Gradient" : "Normal";
			VisualStateManager.GoToState(stateButton, state);
			statusLabel.Text = state;
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 15,
			Children =
			{
				new Label
				{
					Text = "All button backgrounds should stay inside their rounded borders, including after toggling the gradient state.",
					TextColor = Colors.Black
				},
				linearButton,
				radialButton,
				stateButton,
				toggleButton,
				statusLabel
			}
		};
	}

	static Button CreateButton(string text, string automationId, Brush background) => new Button
	{
		Text = text,
		AutomationId = automationId,
		Background = background,
		TextColor = Colors.White,
		BorderColor = Colors.Gray,
		BorderWidth = 1,
		CornerRadius = 20,
		HeightRequest = 60,
		WidthRequest = 260,
		HorizontalOptions = LayoutOptions.Center
	};

	static LinearGradientBrush CreateLinearGradient() => new LinearGradientBrush
	{
		StartPoint = new Point(0, 0),
		EndPoint = new Point(0, 1),
		GradientStops = new GradientStopCollection
		{
			new GradientStop { Color = Colors.CornflowerBlue, Offset = 0 },
			new GradientStop { Color = Colors.Navy, Offset = 1 }
		}
	};
}
