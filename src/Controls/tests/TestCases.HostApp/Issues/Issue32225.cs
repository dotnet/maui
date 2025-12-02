using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 32225, "CollectionView FlowDirection RightToLeft not working in iOS horizontal layouts",
		PlatformAffected.iOS)]
	public class Issue32225 : ContentPage
	{
		private readonly CollectionView _horizontalCollectionView;
		private readonly Label _statusLabel;

		public Issue32225()
		{
			Title = "Issue 32225 - CollectionView RTL Fix";

			var items = new ObservableCollection<string>
			{
				"First", "Second", "Third", "Fourth", "Fifth"
			};

		

			// Create horizontal CollectionView (main test target)
			_horizontalCollectionView = new CollectionView
			{
				AutomationId = "Issue32225HorizontalCollectionView",
				ItemsSource = items,
				ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal),
				FlowDirection = FlowDirection.LeftToRight,
				BackgroundColor = Colors.LightBlue,
				HeightRequest = 150,
				ItemTemplate = new DataTemplate(() =>
				{
					var grid = new Grid
					{
						BackgroundColor = Colors.Orange,
						Margin = new Thickness(5)
					};

					var label = new Label
					{
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						Padding = new Thickness(20, 10),
						TextColor = Colors.White
					};
					label.SetBinding(Label.TextProperty, ".");
					label.SetBinding(AutomationProperties.NameProperty, ".");

					grid.Children.Add(label);
					return grid;
				})
			};

			// Create vertical CollectionView for comparison
		
			// Create control buttons
			var setRtlButton = new Button
			{
				Text = "Set RTL",
				AutomationId = "Issue32225SetRtlButton"
			};
			setRtlButton.Clicked += OnSetRtlButtonClicked;

			var setLtrButton = new Button
			{
				Text = "Set LTR",
				AutomationId = "Issue32225SetLtrButton"
			};
			setLtrButton.Clicked += OnSetLtrButtonClicked;

			// Create status label
			_statusLabel = new Label
			{
				Text = "Current: LeftToRight",
				AutomationId = "Issue32225StatusLabel",
				HorizontalOptions = LayoutOptions.Center,
				Margin = new Thickness(0, 10)
			};

			// Create button grid
			var buttonGrid = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Star }
				}
			};
			Grid.SetColumn(setRtlButton, 0);
			Grid.SetColumn(setLtrButton, 1);
			buttonGrid.Children.Add(setRtlButton);
			buttonGrid.Children.Add(setLtrButton);

			// Create main layout
			var mainGrid = new Grid
			{
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },    // Buttons
					new RowDefinition { Height = GridLength.Auto },    // Status
					new RowDefinition { Height = new GridLength(150) }, // Horizontal CollectionView
					new RowDefinition { Height = GridLength.Star }     // Vertical CollectionView
				}
			};

			Grid.SetRow(buttonGrid, 0);
			Grid.SetRow(_statusLabel, 1);
			Grid.SetRow(_horizontalCollectionView, 2);


			mainGrid.Children.Add(buttonGrid);
			mainGrid.Children.Add(_statusLabel);
			mainGrid.Children.Add(_horizontalCollectionView);

			Content = mainGrid;
		}

		private void OnSetRtlButtonClicked(object sender, EventArgs e)
		{
			_horizontalCollectionView.FlowDirection = FlowDirection.RightToLeft;
			_statusLabel.Text = "Current: RightToLeft";
		}

		private void OnSetLtrButtonClicked(object sender, EventArgs e)
		{
			_horizontalCollectionView.FlowDirection = FlowDirection.LeftToRight;
			_statusLabel.Text = "Current: LeftToRight";
		}
	}
}