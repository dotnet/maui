using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 23023, "CarouselView does not scroll to the specified item at the end after resetting data even though the CurrentItem is updated correctly", PlatformAffected.Android)]
public class Issue23023 : ContentPage
{
	ObservableCollection<string> _carouselItems = new();
	string[] _items = ["0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10"];
	Label _selectedLabel;
	CarouselView2 _carouselView;
	public Issue23023()
	{
		AddItems();

		_selectedLabel = new Label { Text = _items[0] };

		_carouselView = new CarouselView2
		{
			HeightRequest = 150,
			Loop = true,
			ItemsSource = _carouselItems,
			ItemTemplate = new DataTemplate(() =>
			{
				Label label = new Label
				{
					FontSize = 80,
					HorizontalOptions = LayoutOptions.Center,
					VerticalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");

				return new Border
				{
					BackgroundColor = Colors.LightGray,
					Content = label
				};
			})
		};

		Button scrollToLastItem = new Button
		{
			AutomationId = "Issue23023_ScrollToLastItem",
			Text = "ScrollTo Last Item",
			Command = new Command(() => SelectItem(10))
		};

		Button reloadItems = new Button
		{
			AutomationId = "Issue23023_ReloadItems",
			Text = "Reload Items",
			Command = new Command(ReloadItems)
		};

		Content = new VerticalStackLayout
		{
			Padding = 30,
			Spacing = 15,
			Children =
			{
				_carouselView,
				new HorizontalStackLayout
				{
					Children =
					{
						new Label { Text = "Selected item: ", FontSize = 16 },
						_selectedLabel
					}
				},
				new HorizontalStackLayout
				{
					Spacing = 5,
					Children = { scrollToLastItem, reloadItems }
				}
			}
		};
	}

	void SelectItem(int index)
	{
		_carouselView.CurrentItem = _items[index];
		_selectedLabel.Text = _items[index];
	}

	void ReloadItems()
	{
		AddItems();
		_selectedLabel.Text = _items[0];
	}

	void AddItems()
	{
		_carouselItems.Clear();
		foreach (var item in _items)
		{
			_carouselItems.Add(item);
		}
	}
}