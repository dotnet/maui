namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 0, "CollectionView Header/Footer Toggle Issue", PlatformAffected.Android | PlatformAffected.UWP)]
	public class CollectionViewHeaderFooterToggle : TestContentPage
	{
		const string HeaderButtonId = "ToggleHeaderButton";
		const string FooterButtonId = "ToggleFooterButton";
		const string CollectionViewId = "TestCollectionView";
		const string HeaderLabelId = "HeaderLabel";
		const string FooterLabelId = "FooterLabel";

		private CollectionView _collectionView;
		private Button _headerButton;
		private Button _footerButton;
		private Label _headerLabel;
		private Label _footerLabel;

		protected override void Init()
		{
			var grid = new Grid
			{
				Margin = new Thickness(20),
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			// Header Label
			var titleLabel = new Label
			{
				Text = "Test for CollectionView Header/Footer toggle",
				FontSize = 18,
				Margin = new Thickness(0, 0, 0, 10)
			};
			Grid.SetRow(titleLabel, 0);
			grid.Children.Add(titleLabel);

			// Control buttons
			var buttonGrid = new Grid
			{
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = GridLength.Star }
				},
				Margin = new Thickness(0, 10)
			};

			var toggleHeaderButton = new Button
			{
				AutomationId = HeaderButtonId,
				Text = "Remove Header",
			};
			toggleHeaderButton.Clicked += OnToggleHeaderClicked;
			Grid.SetColumn(toggleHeaderButton, 0);
			buttonGrid.Children.Add(toggleHeaderButton);

			var toggleFooterButton = new Button
			{
				AutomationId = FooterButtonId,
				Text = "Remove Footer",
			};
			toggleFooterButton.Clicked += OnToggleFooterClicked;
			Grid.SetColumn(toggleFooterButton, 1);
			buttonGrid.Children.Add(toggleFooterButton);

			Grid.SetRow(buttonGrid, 1);
			grid.Children.Add(buttonGrid);

			// CollectionView
			_collectionView = new CollectionView
			{
				AutomationId = CollectionViewId,
				ItemsSource = new[] { "Item 1", "Item 2", "Item 3" },
				ItemTemplate = new DataTemplate(() =>
				{
					var label = new Label
					{
						TextColor = Colors.Black,
						FontSize = 16,
					};
					label.SetBinding(Label.TextProperty, ".");
					return label;
				})
			};

			// Initial header
			_headerLabel = new Label
			{
				AutomationId = HeaderLabelId,
				BackgroundColor = Colors.LightBlue,
				Text = "This is the Header",
				Padding = new Thickness(10),
			};
			_collectionView.Header = _headerLabel;

			// Initial footer
			_footerLabel = new Label
			{
				AutomationId = FooterLabelId,
				BackgroundColor = Colors.LightCoral,
				Text = "This is the Footer",
				Padding = new Thickness(10),
			};
			_collectionView.Footer = _footerLabel;

			Grid.SetRow(_collectionView, 2);
			grid.Children.Add(_collectionView);

			// Set ContentPage content
			Content = grid;
		}

		private void OnToggleHeaderClicked(object? sender, EventArgs e)
		{
			var button = (Button)sender!;

			if (_collectionView.Header != null)
			{
				_collectionView.Header = null;
				button.Text = "Add Header";
			}
			else
			{
				_collectionView.Header = _headerLabel;
				button.Text = "Remove Header";
			}
		}

		private void OnToggleFooterClicked(object? sender, EventArgs e)
		{
			var button = (Button)sender!;

			if (_collectionView.Footer != null)
			{
				_collectionView.Footer = null;
				button.Text = "Add Footer";
			}
			else
			{
				_collectionView.Footer = _footerLabel;
				button.Text = "Remove Footer";
			}
		}
	}
}
