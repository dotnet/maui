using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24533, "[iOS] RefreshView causes CollectionView scroll position to reset", PlatformAffected.iOS)]
public class Issue24533 : ContentPage
{
	public ObservableCollection<string> Items { get; set; } = new();
	public bool IsLoading { get; set; }
	public ICommand RefreshCommand { get; set; }

	int count = 0;

	public Issue24533()
	{
		// Initialize the RefreshCommand
		RefreshCommand = new Command(OnRefresh);

		// Create UI controls entirely in C#
		var loadMoreButton = new Button
		{
			Text = "Load More"
		};
		loadMoreButton.Clicked += OnLoadMoreClicked;

		var collectionView = new CollectionView
		{
			ItemsSource = Items,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label { Padding = 10 };
				label.SetBinding(Label.TextProperty, ".");
				return label;
			}),
			Footer = loadMoreButton
		};

		var refreshView = new RefreshView
		{
			Command = RefreshCommand
		};
		refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(IsLoading));
		refreshView.Content = collectionView;

		Content = refreshView;

		// Set the binding context and load initial data
		BindingContext = this;
		LoadItems();
	}

	void OnRefresh()
	{
		LoadItems();
	}

	void LoadItems()
	{
		for (int i = 0; i < 20; i++)
			Items.Add($"Item {count + i}");
		count += 20;

		IsLoading = false;
		OnPropertyChanged(nameof(IsLoading));
	}

	void OnLoadMoreClicked(object sender, EventArgs e)
	{
		LoadItems();
	}
}

