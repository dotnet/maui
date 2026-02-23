using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 31339, "[iOS] CarouselViewHandler2 - NSInternalInconsistencyException thrown when setting ItemsSources", PlatformAffected.iOS)]
public class Issue31339 : ContentPage
{
	CarouselView2 _carouselView;
	ObservableCollection<string> _items;
	Button _updateButton;
	Button _nullButton;
	Button _emptyButton;
	Button _largeCollectionButton;
	Button _updatePositionWithItemSourceButton;

	public Issue31339()
	{
		_items = new ObservableCollection<string>();

		_carouselView = new CarouselView2
		{
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					BackgroundColor = Colors.Red,
					HorizontalTextAlignment = TextAlignment.Center,
					VerticalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, new Binding("."));
				return label;
			}),
			AutomationId = "TestCarouselView"
		};

		_updateButton = new Button
		{
			Text = "Update CarouselView ItemsSource",
			AutomationId = "UpdateButton"
		};
		_updateButton.Clicked += OnItemSourceUpdated;

		_largeCollectionButton = new Button
		{
			Text = "Set ItemsSource to Large Collection",
			AutomationId = "LargeCollectionButton"
		};
		_largeCollectionButton.Clicked += OnSetItemsSourceToLargeCollection;

		_nullButton = new Button
		{
			Text = "Set ItemsSource to Null",
			AutomationId = "NullButton"
		};
		_nullButton.Clicked += OnSetItemsSourceToNull;

		_emptyButton = new Button
		{
			Text = "Set ItemsSource to Empty",
			AutomationId = "EmptyButton"
		};
		_emptyButton.Clicked += OnSetItemsSourceToEmpty;

		_updatePositionWithItemSourceButton = new Button
		{
			Text = "Update Position and ItemSource",
			AutomationId = "UpdatePositionWithItemSourceButton"
		};
		_updatePositionWithItemSourceButton.Clicked += OnUpdateItemsSourceAndPosition;

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Children.Add(_updateButton);
		Grid.SetRow(_largeCollectionButton, 1);
		grid.Children.Add(_largeCollectionButton);
		Grid.SetRow(_nullButton, 2);
		grid.Children.Add(_nullButton);
		Grid.SetRow(_emptyButton, 3);
		grid.Children.Add(_emptyButton);
		Grid.SetRow(_updatePositionWithItemSourceButton, 4);
		grid.Children.Add(_updatePositionWithItemSourceButton);
		Grid.SetRow(_carouselView, 5);
		grid.Children.Add(_carouselView);

		Content = grid;
	}

	void OnItemSourceUpdated(object sender, EventArgs e)
	{
		var items = Enumerable.Range(0, 6);
		_carouselView.ItemsSource = items;
	}

	void OnSetItemsSourceToLargeCollection(object sender, EventArgs e)
	{
		var items = Enumerable.Range(0, 1000);
		_carouselView.ItemsSource = items;
	}

	void OnSetItemsSourceToNull(object sender, EventArgs e)
	{
		_carouselView.ItemsSource = null;
	}

	void OnSetItemsSourceToEmpty(object sender, EventArgs e)
	{
		_carouselView.ItemsSource = new List<string>();
	}

	void OnUpdateItemsSourceAndPosition(object sender, EventArgs e)
	{
		_carouselView.Position = 3;
		var items = Enumerable.Range(0, 10);
		_carouselView.ItemsSource = items;
	}

}