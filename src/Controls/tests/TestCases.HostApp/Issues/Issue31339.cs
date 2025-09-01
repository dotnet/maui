using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 31339, "[iOS] CarouselViewHandler2 - NSInternalInconsistencyException thrown when setting ItemsSources", PlatformAffected.iOS)]
public class Issue31339 : ContentPage
{
	CarouselView2 _carouselView;
	ObservableCollection<string> _items;
	Button _button;

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
		_button.Clicked += OnItemSourceUpdated;

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

	void OnItemSourceUpdated(object sender, EventArgs e)
	{
		var items = Enumerable.Range(0, 6);
		_carouselView.ItemsSource = items;
	}
}