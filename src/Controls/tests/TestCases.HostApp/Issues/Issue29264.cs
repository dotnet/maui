namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29264, "Rotating or resizing My Recipes changes the selected item", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue29264 : ContentPage
{
	readonly CarouselView _carousel;
	readonly Label _orientationLabel;
	readonly Label _positionLabel;
	readonly Label _boundsResizeLabel;

	public Issue29264()
	{
		_carousel = new CarouselView
		{
			AutomationId = "RecipeCarousel",
			ItemsSource = new[] { "First Recipe", "Second Recipe", "Third Recipe", "Fourth Recipe", "Fifth Recipe" },
			WidthRequest = 350,
			HeightRequest = 550,
			HorizontalOptions = LayoutOptions.Fill,
			Loop = true,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				label.SetBinding(Label.AutomationIdProperty, ".");
				return new Grid
				{
					WidthRequest = 340,
					Padding = new Thickness(0, 0, 10, 0),
					Children = { label }
				};
			})
		};

		_orientationLabel = new Label { AutomationId = "OrientationLabel" };
		_positionLabel = new Label
		{
			AutomationId = "RecipePositionLabel",
			Text = "Position: 0"
		};
		var loopLabel = new Label
		{
			AutomationId = "RecipeLoopLabel",
			Text = "Loop: True"
		};
		var loopSwitch = new Switch
		{
			AutomationId = "RecipeLoopSwitch",
			IsToggled = true
		};
		loopSwitch.Toggled += (_, e) =>
		{
			_carousel.Loop = e.Value;
			loopLabel.Text = $"Loop: {e.Value}";
		};
		var currentItemLabel = new Label { AutomationId = "RecipeCurrentItemLabel" };
		currentItemLabel.SetBinding(Label.TextProperty, new Binding(nameof(CarouselView.CurrentItem), source: _carousel));
		var positionChangedLabel = new Label
		{
			AutomationId = "RecipePositionChangedLabel",
			Text = "Changes: 0"
		};
		int positionChanges = 0;
		_carousel.PositionChanged += (_, e) =>
		{
			_positionLabel.Text = $"Position: {e.CurrentPosition}";
			positionChangedLabel.Text = $"Changes: {++positionChanges}";
		};
		_boundsResizeLabel = new Label
		{
			AutomationId = "BoundsResizeLabel",
			Text = "Bounds unchanged"
		};
		var resizeButton = new Button
		{
			AutomationId = "ResizeCarouselButton",
			Text = "Resize carousel"
		};
		resizeButton.Clicked += (_, _) =>
		{
			_carousel.HeightRequest = Math.Max(100, _carousel.HeightRequest - 40);
			_boundsResizeLabel.Text = "Bounds resized";
		};

		var layout = new Grid
		{
			Margin = 20,
			RowDefinitions = new RowDefinitionCollection
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			}
		};
		layout.Add(_orientationLabel, row: 0);
		layout.Add(_positionLabel, row: 1);
		layout.Add(loopLabel, row: 2);
		layout.Add(loopSwitch, row: 3);
		layout.Add(currentItemLabel, row: 4);
		layout.Add(positionChangedLabel, row: 5);
		layout.Add(new HorizontalStackLayout
		{
			Spacing = 8,
			Children = { resizeButton, _boundsResizeLabel }
		}, row: 6);
		layout.Add(_carousel, row: 7);
		Content = layout;
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		base.OnSizeAllocated(width, height);

		if (width <= 0 || height <= 0)
			return;

		if (width > height)
		{
			_carousel.HeightRequest = 200;
			_carousel.WidthRequest = width - 100;
			_orientationLabel.Text = "Landscape";
		}
		else
		{
			_carousel.HeightRequest = height - 300;
			_carousel.WidthRequest = 350;
			_orientationLabel.Text = "Portrait";
		}
	}
}
