using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27732, "[Windows] View Position Shifts When Shadows Are Dynamically Removed or Resized", PlatformAffected.UWP)]
public class Issue27732 : TestContentPage
{
	Border borderShadow;
	Image imageShadow;
	Label labelShadow;
	Button shadowButton;
	bool _shadow = false;

	protected override void Init()
	{
		Title = "Issue27732";
		borderShadow = new Border
		{
			AutomationId = "BorderShadow",
			StrokeShape = new RoundRectangle { CornerRadius = 24 },
			Background = Colors.Red,
			WidthRequest = 80
		};

		imageShadow = new Image
		{
			AutomationId = "ImageShadow",
			Aspect = Aspect.AspectFit,
			Source = "oasis.jpg",
			WidthRequest = 80
		};

		labelShadow = new Label
		{
			AutomationId = "LabelShadow",
			Text = "Label",
			FontSize = 18,
			WidthRequest = 80
		};

		shadowButton = new Button
		{
			AutomationId = "ToggleShadowButton",
			Text = "Add Shadow",
			HorizontalOptions = LayoutOptions.Start
		};
		shadowButton.Clicked += OnShadowClicked;

		Content = new StackLayout
		{
			Margin = new Thickness(0, 40),
			HorizontalOptions = LayoutOptions.Center,
			Spacing = 20,
			Children =
			{
				new HorizontalStackLayout
				{
					HorizontalOptions = LayoutOptions.Center,
					Spacing = 10,
					Children = { borderShadow, imageShadow, labelShadow }
				},
				new HorizontalStackLayout
				{
					Spacing = 10,
					Children =
					{
						new Label
						{
							Text = "Toggle Shadow Button:",
							FontSize = 15,
							VerticalOptions = LayoutOptions.Center,
							HorizontalOptions = LayoutOptions.Start
						},
						shadowButton
					}
				}
			}
		};
	}

	void OnShadowClicked(object sender, EventArgs e)
	{
		if (_shadow)
		{
			shadowButton.Text = "Add Shadow";
			borderShadow.Shadow = imageShadow.Shadow = labelShadow.Shadow = null;
			_shadow = false;
		}
		else
		{
			shadowButton.Text = "Remove Shadow";

			var newShadow = new Shadow
			{
				Brush = Brush.Black,
				Offset = new Point(4, 4),
				Radius = 10,
				Opacity = 0.5f
			};

			borderShadow.Shadow = imageShadow.Shadow = labelShadow.Shadow = newShadow;
			_shadow = true;
		}
	}
}