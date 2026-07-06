namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 36267, "[iOS] IsSwipeEnabled Not Working on CarouselView (CV2) when set to false at load time", PlatformAffected.iOS | PlatformAffected.macOS | PlatformAffected.Android)]
	public class Issue36267 : TestContentPage
	{
		CarouselView _carouselView;
		Label _positionLabel;

		protected override void Init()
		{
			var items = new List<string> { "Item1", "Item2", "Item3" };

			_positionLabel = new Label
			{
				AutomationId = "PositionLabel",
				Text = "Position: 0"
			};

			_carouselView = new CarouselView
			{
				AutomationId = "CarouselView",
				// IsSwipeEnabled is set to false from the moment the CarouselView is
				// created (not toggled afterward) to reproduce
				// https://github.com/dotnet/maui/issues/36267, where CV2 on iOS
				// ignores IsSwipeEnabled when it starts out as false.
				IsSwipeEnabled = false,
				ItemsSource = items,
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						HorizontalOptions = LayoutOptions.Center,
						VerticalOptions = LayoutOptions.Center,
						FontSize = 24,
						TextColor = Colors.Black
					};

					label.SetBinding(Label.TextProperty, ".");
					label.SetBinding(Label.AutomationIdProperty, ".");

					return new StackLayout
					{
						Children = { label }
					};
				}),
				HeightRequest = 300
			};

			_carouselView.PositionChanged += OnPositionChanged;

			var toggleButton = new Button
			{
				Text = "Enable IsSwipeEnabled",
				AutomationId = "ToggleSwipeButton"
			};
			toggleButton.Clicked += (s, e) => _carouselView.IsSwipeEnabled = !_carouselView.IsSwipeEnabled;

			Content = new StackLayout
			{
				Padding = 20,
				Children =
				{
					toggleButton,
					_positionLabel,
					_carouselView
				}
			};
		}

		void OnPositionChanged(object sender, PositionChangedEventArgs e)
		{
			_positionLabel.Text = $"Position: {e.CurrentPosition}";
		}
	}
}
