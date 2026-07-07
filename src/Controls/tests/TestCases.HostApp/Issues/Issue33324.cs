using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33324, "CollectionView.EmptyView does not remeasure its height when the parent layout changes dynamically", PlatformAffected.Android)]
public class Issue33324 : ContentPage
{
	readonly Issue33324ViewModel _viewModel;

	public Issue33324()
	{
		Title = "Issue 33324 - EmptyView Height";

		_viewModel = new Issue33324ViewModel();
		BindingContext = _viewModel;

		var firstCollectionView = new CollectionView2
		{
			AutomationId = "FirstCollectionView",
			HeightRequest = 380,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Margin = new Thickness(10, 5) };
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};
		firstCollectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(Issue33324ViewModel.MyItems));

		var itemsContainer = new VerticalStackLayout();
		itemsContainer.SetBinding(VisualElement.IsVisibleProperty, nameof(Issue33324ViewModel.HasItems));
		itemsContainer.Children.Add(new Label
		{
			Text = "Items Loaded:",
			FontAttributes = FontAttributes.Bold,
			Margin = new Thickness(10, 0, 0, 0)
		});
		itemsContainer.Children.Add(firstCollectionView);

		var loadButton = new Button
		{
			Text = "Load Items",
			AutomationId = "LoadItemsButton",
			Margin = new Thickness(10)
		};
		loadButton.SetBinding(Button.CommandProperty, nameof(Issue33324ViewModel.LoadItemsCommand));

		var topContainer = new VerticalStackLayout();
		topContainer.Children.Add(itemsContainer);
		topContainer.Children.Add(loadButton);

		var emptyViewLabel = new Label
		{
			Text = "No players available",
			AutomationId = "EmptyViewLabel",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			FontSize = 18,
			TextColor = Colors.Black
		};

		var emptyViewGrid = new Grid
		{
			AutomationId = "EmptyViewGrid"
		};
		emptyViewGrid.Children.Add(new BoxView
		{
			Color = Colors.Pink,
			AutomationId = "EmptyViewBackground"
		});
		emptyViewGrid.Children.Add(emptyViewLabel);

		var secondCollectionView = new CollectionView2
		{
			AutomationId = "SecondCollectionView",
			EmptyView = emptyViewGrid
		};
		secondCollectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(Issue33324ViewModel.Players));

		var mainGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			},
			RowSpacing = 20
		};

		mainGrid.Add(topContainer, 0, 0);
		mainGrid.Add(secondCollectionView, 0, 1);

		Content = mainGrid;
	}
}

public class Issue33324ViewModel : INotifyPropertyChanged
{
	bool _hasItems;
	ObservableCollection<string> _myItems;
	ObservableCollection<string> _players;

	public Issue33324ViewModel()
	{
		_myItems = new ObservableCollection<string>();
		_players = new ObservableCollection<string>();
		_hasItems = false;

		LoadItemsCommand = new Command(LoadItems);
	}

	public bool HasItems
	{
		get => _hasItems;
		set
		{
			if (_hasItems != value)
			{
				_hasItems = value;
				OnPropertyChanged();
			}
		}
	}

	public ObservableCollection<string> MyItems
	{
		get => _myItems;
		set
		{
			if (_myItems != value)
			{
				_myItems = value;
				OnPropertyChanged();
			}
		}
	}

	public ObservableCollection<string> Players
	{
		get => _players;
		set
		{
			if (_players != value)
			{
				_players = value;
				OnPropertyChanged();
			}
		}
	}

	public Command LoadItemsCommand { get; }

	void LoadItems()
	{
		MyItems.Clear();
		for (int i = 1; i <= 10; i++)
		{
			MyItems.Add($"Item {i}");
		}
		HasItems = true;
	}

	public event PropertyChangedEventHandler PropertyChanged;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}