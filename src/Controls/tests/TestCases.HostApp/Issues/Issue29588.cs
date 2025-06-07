using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.Issues;

[Issue(IssueTracker.Github, 29588, "CollectionView RemainingItemsThresholdReachedcommand should trigger on scroll near end", PlatformAffected.Android)]
public class Issue29588 : ContentPage
{
	public Issue29588()
	{
		BindingContext = new Issue29588ViewModel();

		var thresholdLabel = new Label
		{
			AutomationId = "29588ThresholdLabel",
			HorizontalOptions = LayoutOptions.Center,
			HeightRequest = 50,
			FontSize = 18
		};
		thresholdLabel.SetBinding(Label.TextProperty, nameof(Issue29588ViewModel.ThresholdStatus));

		var collectionView = new CollectionView
		{
			AutomationId = "29588CollectionView",
			ItemsSource = ((Issue29588ViewModel)BindingContext).Items,
			RemainingItemsThreshold = 1,
			RemainingItemsThresholdReachedCommand = ((Issue29588ViewModel)BindingContext).RemainingItemReachedCommand,
			Header = new Grid
			{
				BackgroundColor = Colors.Bisque,
				Children =
					{
						new Label
						{
							Margin = new Thickness(20,30),
							FontSize = 22,
							Text = "CollectionView does not fire RemainingItemsThresholdReachedCommand when Header and Footer both are set."
						}
					}
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					Margin = new Thickness(20, 30),
					FontSize = 25
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),

		};

		var activityIndicator = new ActivityIndicator
		{
			Margin = new Thickness(0, 20)
		};
		activityIndicator.SetBinding(ActivityIndicator.IsRunningProperty, "IsLoadingMore");
		activityIndicator.SetBinding(ActivityIndicator.IsVisibleProperty, "IsLoadingMore");
		collectionView.Footer = activityIndicator;


		var grid = new Grid
		{
			Padding = 20,
			RowDefinitions =
				{
					new RowDefinition { Height = 50 }, // Threshold label
                    new RowDefinition { Height = GridLength.Star }  // CollectionView
                }
		};

		grid.Add(thresholdLabel, 0, 0);
		grid.Add(collectionView, 0, 1);

		Content = grid;
	}
}

public class Issue29588ViewModel : INotifyPropertyChanged
{
	private bool _isLoadingMore;
	private int _loadCount = 0;
	private string thresholdStatus;

	public event PropertyChangedEventHandler PropertyChanged;
	public ObservableCollection<string> Items { get; } = new ObservableCollection<string>();

	public ICommand RemainingItemReachedCommand { get; }

	public bool IsLoadingMore
	{
		get => _isLoadingMore;
		set
		{
			if (_isLoadingMore != value)
			{
				_isLoadingMore = value;
				OnPropertyChanged();
			}
		}
	}
	public string ThresholdStatus
	{
		get => thresholdStatus;
		set
		{
			if (thresholdStatus != value)
			{
				thresholdStatus = value;
				OnPropertyChanged();
			}
		}
	}

	public Issue29588ViewModel()
	{
		ThresholdStatus = "Threshold not reached";
		RemainingItemReachedCommand = new Command(async () => await LoadMoreItemsAsync());
		LoadInitialItems();
	}

	private void LoadInitialItems()
	{
		for (int i = 1; i <= 20; i++)
		{
			Items.Add($"Item {i}");
		}
	}

	private async Task LoadMoreItemsAsync()
	{
		if (IsLoadingMore)
			return;

		IsLoadingMore = true;

		await Task.Delay(1500); // Simulate API call or long operation

		for (int i = 1; i <= 10; i++)
		{
			Items.Add($"Loaded Item {_loadCount * 10 + i + 20}");
		}

		_loadCount++;
		IsLoadingMore = false;
		ThresholdStatus = "Threshold reached";
	}

	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

