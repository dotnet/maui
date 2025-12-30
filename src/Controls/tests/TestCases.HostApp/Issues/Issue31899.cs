using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 31899, "Header/Footer removed at runtime leaves empty space and EmptyView not resized in CollectionView", PlatformAffected.iOS | PlatformAffected.macOS)]

public class Issue31899 : ContentPage
{
	CollectionView _collectionView;
	Button _headerButton;
	Button _footerButton;

	public Issue31899()
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
		var headerLabel = new Label
		{
			Text = "Test for CollectionView empty view positioning",
			AutomationId = "HeaderLabel",
		};
		Grid.SetRow(headerLabel, 0);
		grid.Children.Add(headerLabel);

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
			AutomationId = "ToggleHeaderButton",
			Text = "Remove Header",
		};
		toggleHeaderButton.Clicked += OnToggleHeaderClicked;
		Grid.SetColumn(toggleHeaderButton, 0);
		buttonGrid.Children.Add(toggleHeaderButton);

		var toggleFooterButton = new Button
		{
			AutomationId = "ToggleFooterButton",
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
			AutomationId = "CollectionView",
			ItemsSource = Array.Empty<string>(), // empty array to trigger EmptyView
			ItemTemplate = new DataTemplate(() =>
			{
				return new Label
				{
					TextColor = Colors.Black,
					FontSize = 16,
					// This binding works with string items
					BindingContext = "{Binding .}"
				};
			})
		};

		// EmptyView
		_collectionView.EmptyView = new Label
		{
			AutomationId = "EmptyViewLabel",
			BackgroundColor = Color.FromArgb("#FFE40606"),
			Text = "EmptyView: This should show when no data.",
		};

		// Initial header
		_headerButton = new Button
		{
			AutomationId = "CollectionViewHeader",
			BackgroundColor = Colors.LightBlue,
			Text = "Header",
		};
		_collectionView.Header = _headerButton;

		// Initial footer
		_footerButton = new Button
		{
			AutomationId = "CollectionViewFooter",
			BackgroundColor = Colors.LightCoral,
			Text = "Footer",
		};
		_collectionView.Footer = _footerButton;

		Grid.SetRow(_collectionView, 2);
		grid.Children.Add(_collectionView);

		// Set ContentPage content
		Content = grid;
	}

	void OnToggleHeaderClicked(object sender, EventArgs e)
	{
		var button = (Button)sender!;

		if (_collectionView.Header != null)
		{
			_collectionView.Header = null;
			button.Text = "Add Header";
		}
		else
		{
			_collectionView.Header = _headerButton;
			button.Text = "Remove Header";
		}
	}

	void OnToggleFooterClicked(object sender, EventArgs e)
	{
		var button = (Button)sender!;

		if (_collectionView.Footer != null)
		{
			_collectionView.Footer = null;
			button.Text = "Add Footer";
		}
		else
		{
			_collectionView.Footer = _footerButton;
			button.Text = "Remove Footer";
		}
	}
}