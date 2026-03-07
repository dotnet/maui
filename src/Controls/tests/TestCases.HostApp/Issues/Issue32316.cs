#if TEST_FAILS_ON_ANDROID
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32316, "RTL mode: Padding for the label is not mirroring properly", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue32316 : ContentPage
{
	private readonly HorizontalStackLayout _row1;
	private readonly HorizontalStackLayout _row2;

	public Issue32316()
	{
		var toggleButton = new Button
		{
			Text = "Toggle FlowDirection",
			AutomationId = "ToggleFlowDirectionButton",
			HorizontalOptions = LayoutOptions.Center
		};
		
		toggleButton.Clicked += OnToggleClicked;

		_row1 = BuildRow();
		_row2 = BuildRow();

		Content = new VerticalStackLayout
		{
			Spacing = 12,
			Padding = 12,
			Children =
			{
				toggleButton,
				_row1,
				_row2,
			}
		};
	}

	private void OnToggleClicked(object sender, EventArgs e)
	{
		_row2.FlowDirection = FlowDirection.RightToLeft;
	}

	private static HorizontalStackLayout BuildRow()
	{
		var stackLayout = new HorizontalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
		};

		stackLayout.Children.Add(CreateIconBorder("✂"));
		stackLayout.Children.Add(CreateIconBorder("✖"));

		return stackLayout;
	}

	private static View CreateIconBorder(string icon)
	{
		return new Border
		{
			BackgroundColor = Colors.LightGray,
			Stroke = Colors.Gray,
			StrokeThickness = 1,
			WidthRequest = 60,
			HeightRequest = 40,
			InputTransparent = true,
			Content = CreateIconView(icon)
		};
	}

	private static View CreateIconView(string icon)
	{
		var iconLabel = new Label
		{
			Text = icon,
			FontSize = 16,
			FontAutoScalingEnabled = false,
			Padding = new Thickness(10, 0, 0, 0),
			HorizontalOptions = LayoutOptions.Start,
			VerticalOptions = LayoutOptions.Center,
			VerticalTextAlignment = TextAlignment.Center,
			BackgroundColor = Colors.Transparent,
			HeightRequest = 40
		};

		var dropDownLabel = new Label
		{
			Text = "V",
			FontSize = 16,
			Margin = new Thickness(0, 0, 10, 0),
			FontFamily = "MauiMaterialAssets",
			VerticalTextAlignment = TextAlignment.Center,
			VerticalOptions = LayoutOptions.Center,
			BackgroundColor = Colors.Transparent
		};

		return new HorizontalStackLayout
		{
			HorizontalOptions = LayoutOptions.Center,
			HeightRequest = 40,
			Children = { iconLabel, dropDownLabel }
		};
	}
}
#endif