using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 24533, "[iOS] RefreshView causes CollectionView scroll position to reset", PlatformAffected.iOS)]
public class Issue24533 : ContentPage
{
	ObservableCollection<string> _items;
	public ObservableCollection<string> Items
	{
		get => _items;
		set
		{
			_items = value;
			OnPropertyChanged(nameof(Items));
		}
	}

	bool _isLoading;
	public bool IsLoading
	{
		get => _isLoading;
		set
		{
			_isLoading = value;
			OnPropertyChanged(nameof(IsLoading));
		}
	}

	ICommand _refreshCommand;
	public ICommand RefreshCommand
	{
		get => _refreshCommand;
		set
		{
			_refreshCommand = value;
			OnPropertyChanged(nameof(RefreshCommand));
		}
	}

		int count;

	public Issue24533()
	{
		// Initialize properties
		RefreshCommand = new Command(AddItems);
		Items = new ObservableCollection<string>();
		this.On<Microsoft.Maui.Controls.PlatformConfiguration.iOS>().SetUseSafeArea(true);
		// Create CollectionView
		var collectionView = new CollectionView
		{
			SelectionMode = SelectionMode.Single,
			ItemTemplate = new DataTemplate(() =>
			{
				var stack = new VerticalStackLayout();
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				stack.Children.Add(label);
				return stack;
			}),

			
		};

		var btn = new Button { Text = "Load More", AutomationId="Footer" };
		btn.Clicked += Button_Clicked;
		collectionView.Footer =	 btn;
			

		// Bind ItemsSource
		collectionView.SetBinding(ItemsView.ItemsSourceProperty, nameof(Items));

		// Create RefreshView
		var refreshView = new RefreshView
		{
			Margin = 16
		};
		refreshView.SetBinding(RefreshView.IsRefreshingProperty, nameof(IsLoading));
		refreshView.SetBinding(RefreshView.CommandProperty, nameof(RefreshCommand));
		refreshView.Content = collectionView;

		Content = refreshView;

		// Set BindingContext to itself (so bindings work)
		BindingContext = this;

		// Load initial items
		AddItems();
	}

		void Button_Clicked(object sender, EventArgs e)
		{
			IsLoading = true;
		}

		void AddItems()
		{
			// To simulate the reset behavior: Reassign Items collection like XAML scenario
			foreach (int value in Enumerable.Range(count, 25))
			{
				Items.Add(value.ToString());
			}
			count += 20;

			IsLoading = false;
        }
    }