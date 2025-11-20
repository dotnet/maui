namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14184, "Setting the IsEnabled property of the CarouselView to false does not prevent swiping through items", PlatformAffected.Android | PlatformAffected.iOS )]
public class Issue14184 : ContentPage
{
	CarouselView _carouselView;
	Label _statusLabel;
	public Issue14184()
	{
		_carouselView = new CarouselView
		{
			AutomationId = "DisabledCarouselView",
			HeightRequest = 300,
			IsEnabled = false,
			Loop = false,
			ItemsSource = new string[] { "Item 1", "Item 2", "Item 3", "Item 4", "Item 5" },
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontSize = 32,
					BackgroundColor = Colors.LightGray,
					Padding = 20,
					HorizontalOptions = LayoutOptions.Fill,
					VerticalOptions = LayoutOptions.Fill
				};
				label.SetBinding(Label.TextProperty, ".");

				return label;
			})
		};

		_carouselView.CurrentItemChanged += OnCarouselViewCurrentItemChanged;

		_statusLabel = new Label
		{
			AutomationId = "Issue14184StatusLabel",
			Text = "Success",
		};

		Grid grid = new Grid
		{
			Padding = 20,
			RowSpacing = 20,
			RowDefinitions =
			{
				new RowDefinition { Height = new GridLength(300) },
				new RowDefinition { Height = GridLength.Auto }
			}
		};

		grid.Add(_carouselView, 0, 0);
		grid.Add(_statusLabel, 0, 1);

		Content = grid;
	}

	void OnCarouselViewCurrentItemChanged(object sender, CurrentItemChangedEventArgs e)
	{
		if (e.CurrentItem?.ToString() != "Item 1")
		{
			_statusLabel.Text = "Failure";
		}
	}
}