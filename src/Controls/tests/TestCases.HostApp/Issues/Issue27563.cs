using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 27563, "[Windows] CarouselView Scrolling Issue", PlatformAffected.UWP)]
public partial class Issue27563 : ContentPage
{
	public Issue27563()
	{
		var verticalStackLayout = new VerticalStackLayout();

		var carouselItems = new ObservableCollection<string>
		{
			"Remain View",
			"Actual View",
			"Percentage View",
		};

		CarouselView carousel = new CarouselView
		{
			ItemsSource = carouselItems,
			AutomationId = "carouselview",
			HeightRequest = 200,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid
				{
					Padding = 10
				};

				var label = new Label
				{
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center,
					FontSize = 18,
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");

				grid.Children.Add(label);
				return grid;
			}),
			HorizontalOptions = LayoutOptions.Fill,
		};

		var indicatorView = new IndicatorView
		{
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};

		carousel.IndicatorView = indicatorView;

		var carouselPositionLabel = new Label
		{
			Text = "CarouselPos:0",
			AutomationId = "carouselPositionLabel",
			HorizontalOptions = LayoutOptions.Center,
		};

		var indicatorPositionLabel = new Label
		{
			Text = "IndicatorPos:0",
			AutomationId = "indicatorPositionLabel",
			HorizontalOptions = LayoutOptions.Center,
		};

		var pingLabel = new Label
		{
			Text = "Ping:0",
			AutomationId = "pingLabel",
			HorizontalOptions = LayoutOptions.Center,
		};

		var currentItemLabel = new Label
		{
			Text = "CurrentItem:Remain View",
			AutomationId = "currentItemLabel",
			HorizontalOptions = LayoutOptions.Center,
		};

		var scrollToSecondButton = new Button
		{
			Text = "Scroll To Second Item",
			AutomationId = "ScrollToSecondButton",
			Margin = new Thickness(20, 10),
		};

		scrollToSecondButton.Clicked += (sender, e) =>
		{
			carousel.ScrollTo(1, position: ScrollToPosition.Center, animate: false);
		};

		var positionButton = new Button
		{
			Text = "Change IndicatorView Position",
			AutomationId = "PositionButton",
			Margin = new Thickness(20, 10),
		};

		positionButton.Clicked += (sender, e) =>
		{
			indicatorView.Position = 2;
		};

		var pingCount = 0;
		var pingButton = new Button
		{
			Text = "Ping",
			AutomationId = "PingButton",
			Margin = new Thickness(20, 10),
		};

		pingButton.Clicked += (sender, e) =>
		{
			pingCount++;
			pingLabel.Text = $"Ping:{pingCount}";
		};

		carousel.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName == CarouselView.PositionProperty.PropertyName)
				carouselPositionLabel.Text = $"CarouselPos:{carousel.Position}";
		};

		indicatorView.PropertyChanged += (sender, e) =>
		{
			if (e.PropertyName == IndicatorView.PositionProperty.PropertyName)
				indicatorPositionLabel.Text = $"IndicatorPos:{indicatorView.Position}";
		};

		carousel.CurrentItemChanged += (sender, e) =>
		{
			currentItemLabel.Text = $"CurrentItem:{carousel.CurrentItem}";
		};

		verticalStackLayout.Children.Add(carousel);
		verticalStackLayout.Children.Add(indicatorView);
		verticalStackLayout.Children.Add(scrollToSecondButton);
		verticalStackLayout.Children.Add(positionButton);
		verticalStackLayout.Children.Add(pingButton);
		verticalStackLayout.Children.Add(carouselPositionLabel);
		verticalStackLayout.Children.Add(indicatorPositionLabel);
		verticalStackLayout.Children.Add(currentItemLabel);
		verticalStackLayout.Children.Add(pingLabel);

		Content = verticalStackLayout;
	}
}
