using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14141, "Incorrect Intermediate CurrentItem updates with CarouselView Scroll Animation Enabled", PlatformAffected.Android)]
public class Issue14141 : ContentPage
{
	Issue14141ViewModel _viewModel = new();
	string[] _items = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11" };
	public Issue14141()
	{
		BindingContext = _viewModel;

		CarouselView carouselView = new CarouselView
		{
			HeightRequest = 150,
			Loop = true,
			IsScrollAnimated = true,
			ItemsSource = new ObservableCollection<string>(_items),
			ItemTemplate = new DataTemplate(() =>
			{
				Border border = new Border
				{
					BackgroundColor = Colors.LightGray,
					Content = new Label
					{
						VerticalOptions = LayoutOptions.Center,
						HorizontalOptions = LayoutOptions.Center,
						FontSize = 80
					}
				};
				border.Content.SetBinding(Label.TextProperty, ".");
				return border;
			})
		};
		carouselView.SetBinding(CarouselView.CurrentItemProperty, nameof(Issue14141ViewModel.SelectedItem));

		Label selectedItemLabel = new Label
		{
			FontSize = 16
		};
		selectedItemLabel.SetBinding(Label.TextProperty, nameof(Issue14141ViewModel.SelectedItem), stringFormat: "Selected item: {0}");

		Button selectButton = new Button
		{
			AutomationId = "Issue14141ScrollBtn",
			Text = "Select item 4",
			FontSize = 12
		};
		selectButton.Clicked += (s, e) => SelectItem(4);

		Label logLabel = new Label
		{
			FontSize = 14
		};
		logLabel.SetBinding(Label.TextProperty, nameof(Issue14141ViewModel.ActionHistoryText));

		Content = new VerticalStackLayout
		{
			Padding = 25,
			Spacing = 15,
			Children =
			{
				carouselView,
				selectedItemLabel,
				selectButton,
				new Label { Text = "Current Item Update History: ", FontSize = 16 },
				logLabel
			}
		};
	}

    void SelectItem(int index)
    {
        _viewModel.AddLog($"Current Item: {_items[index]}");
        _viewModel.SelectedItem = _items[index];
    }
}

public class Issue14141ViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    string _selectedItem;
    public string SelectedItem
    {
        get => _selectedItem;
        set
        {
            if (_selectedItem != value)
            {
                _selectedItem = value;
                AddLog("Current Item: " + (_selectedItem ?? "null"));
                OnPropertyChanged(nameof(SelectedItem));
            }
        }
    }

    ObservableCollection<string> _logEntries = new();

    public string ActionHistoryText => string.Join(Environment.NewLine, _logEntries);

    public void AddLog(string message)
    {
        _logEntries.Add(message);
        OnPropertyChanged(nameof(ActionHistoryText));
    }

    private void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}