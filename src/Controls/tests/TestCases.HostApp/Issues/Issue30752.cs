using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 30752, "Default MinWidth in WinUI RadioButton interferes with custom ControlTemplates", PlatformAffected.UWP)]
public class Issue30752 : ContentPage
{
	public Issue30752()
	{
		ControlTemplate radioButtonTemplate = new ControlTemplate(() =>
		{
			Grid outerGrid = new Grid
			{
				Padding = new Thickness(5),
				VerticalOptions = LayoutOptions.Center,
				BackgroundColor = Colors.Red
			};

			HorizontalStackLayout horizontalLayout = new HorizontalStackLayout
			{
				HorizontalOptions = LayoutOptions.Start
			};

			Grid radioCircleGrid = new Grid
			{
				Margin = new Thickness(0, 0, 4, 0),
				WidthRequest = 21,
				HeightRequest = 21,
				HorizontalOptions = LayoutOptions.Start,
				VerticalOptions = LayoutOptions.Start
			};

			Ellipse outerEllipse = new Ellipse
			{
				Stroke = Colors.White,
				Fill = Brush.Transparent,
				WidthRequest = 20,
				HeightRequest = 20,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			Ellipse innerEllipse = new Ellipse
			{
				WidthRequest = 12,
				HeightRequest = 12,
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			};

			radioCircleGrid.Children.Add(outerEllipse);
			radioCircleGrid.Children.Add(innerEllipse);

			ContentPresenter contentPresenter = new ContentPresenter();

			horizontalLayout.Children.Add(radioCircleGrid);
			horizontalLayout.Children.Add(contentPresenter);

			outerGrid.Children.Add(horizontalLayout);

			return outerGrid;
		});

		Style radioButtonStyle = new Style(typeof(RadioButton))
		{
			Setters =
			{
				new Setter
				{
					Property = RadioButton.ControlTemplateProperty,
					Value = radioButtonTemplate
				}
			}
		};

		RadioButton radioButton = new RadioButton
		{
			AutomationId = "Issue30752RadioButton",
			TextColor = Colors.White,
			IsChecked = true,
			Content = "Bill",
			Style = radioButtonStyle
		};

		HorizontalStackLayout stack = new HorizontalStackLayout
		{
			Spacing = 4,
			HorizontalOptions = LayoutOptions.Center,
			Children = { radioButton }
		};

		Content = stack;
	}
}