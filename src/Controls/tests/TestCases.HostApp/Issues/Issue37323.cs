namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 37323, "Setting the padding value through binding or by using x:Name does not update the ScrollView padding", PlatformAffected.Android)]
	public class Issue37323 : ContentPage
	{
		Thickness _scrollViewPadding;

		public Thickness ScrollViewPadding
		{
			get => _scrollViewPadding;
			set
			{
				if (_scrollViewPadding != value)
				{
					_scrollViewPadding = value;
					OnPropertyChanged();
				}
			}
		}

		public Issue37323()
		{
			BindingContext = this;

			var topEdgeIndicator = new Label
			{
				AutomationId = "TopEdgeIndicator",
				BackgroundColor = Colors.Yellow,
				Text = "ScrollView content"
			};

			var paddingButton = new Button
			{
				AutomationId = "Padding",
				HorizontalOptions = LayoutOptions.Start,
				Text = "Set Padding to 20"
			};

			paddingButton.Clicked += OnPaddingClicked;

			var verticalStackLayout = new VerticalStackLayout
			{
				BackgroundColor = Colors.LightGray,
				Children =
			{
				topEdgeIndicator,
				paddingButton,
				new Label
				{
					Text = "Test Item 1",
					HeightRequest = 100
				},
				new Label
				{
					Text = "Test Item 2",
					HeightRequest = 100
				},
				new Label
				{
					Text = "Test Item 3",
					HeightRequest = 100
				},
				new Label
				{
					Text = "Test Item 4",
					HeightRequest = 100
				},
				new Label
				{
					Text = "Test Item 5",
					HeightRequest = 100
				},
				new Label
				{
					Text = "Test Item 6",
					HeightRequest = 100
				},
				new Label
				{
					Text = "Test Item 7",
					HeightRequest = 100
				},
				new Label
				{
					Text = "Test Item 8",
					HeightRequest = 100
				}
			}
			};

			var scrollView = new ScrollView
			{
				AutomationId = "TestScrollView",
				BackgroundColor = Colors.Red,
				Content = verticalStackLayout
			};

			scrollView.SetBinding(
				ScrollView.PaddingProperty,
				nameof(ScrollViewPadding));

			var grid = new Grid
			{
				BackgroundColor = Colors.Blue,
				Children =
			{
				scrollView
			}
			};

			Content = grid;
		}

		private void OnPaddingClicked(object sender, EventArgs e)
		{
			ScrollViewPadding = new Thickness(20);
		}
	}
}
