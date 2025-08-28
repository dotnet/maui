using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 31339, "[iOS] CarouselViewHandler2 NSInternalInconsistencyException thrown when setting ItemsSources", PlatformAffected.iOS)]
public class Issue31339 : ContentPage
{
	readonly CarouselView2 _carouselView;
	readonly ObservableCollection<string> _items;
	readonly Button _button;

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

		_button = new Button
		{
			Text = "Update CarouselView ItemsSource",
			AutomationId = "UpdateButton"
		};
		_button.Clicked += OnUpdateClicked;

		var grid = new Grid
		{
			RowDefinitions = 
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Children.Add(_button);
		Grid.SetRow(_carouselView, 1);
		grid.Children.Add(_carouselView);

		Content = grid;
	}

	void OnUpdateClicked(object sender, EventArgs e)
	{
		// Start with high position to simulate the issue
		_carouselView.Position = 15;

		// Create items with random count (5-20) as in reproduction case
		var itemCount = new Random().Next(5, 20);
		var items = Enumerable.Range(0, itemCount).Select(i => $"Item {i}").ToList();

		// This should not throw NSInternalInconsistencyException with our fix
		_carouselView.ItemsSource = items;
	}
}