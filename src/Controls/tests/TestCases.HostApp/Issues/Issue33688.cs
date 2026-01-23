using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33688, "BackButtonBehavior is no longer triggered once a ContentPage contains a CollectionView and the ItemsSource has been changed", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue33688 : Shell
{
	public Issue33688()
	{
		var mainContent = new ShellContent
		{
			ContentTemplate = new DataTemplate(() => new Issue33688MainPage()),
			Title = "Main",
			Route = "main"
		};

		Items.Add(mainContent);
		Routing.RegisterRoute("Issue33688_second", typeof(Issue33688SecondPage));
	}
}

file class Issue33688MainPage : ContentPage
{
	public Issue33688MainPage()
	{
		Padding = 24;
		
		var resultLabel = new Label
		{
			Text = "Waiting for back button...",
			AutomationId = "ResultLabel"
		};
		
		// Store reference so ViewModel can update it
		Issue33688ViewModel.SetResultLabelRef(resultLabel);
		
		Content = new VerticalStackLayout
		{
			Spacing = 10,
			Children =
			{
				new Label
				{
					Text = "Tap the button to navigate to a page with a CollectionView. Then press back - the BackButtonBehavior command should fire and update the label below.",
					AutomationId = "InstructionLabel"
				},
				new Button
				{
					Text = "Navigate to other Page",
					AutomationId = "NavigateButton",
					Command = new Command(() => Shell.Current.GoToAsync("Issue33688_second"))
				},
				resultLabel
			}
		};
	}
}

file class Issue33688SecondPage : ContentPage
{
	public Issue33688SecondPage()
	{
		// Use a ViewModel pattern with binding - this is the scenario from the issue
		var viewModel = new Issue33688ViewModel();
		BindingContext = viewModel;

		// BackButtonBehavior with BOUND Command (key to reproduction)
		var backButtonBehavior = new BackButtonBehavior();
		backButtonBehavior.SetBinding(BackButtonBehavior.CommandProperty, nameof(Issue33688ViewModel.SaveAndNavigateBackCommand));
		Shell.SetBackButtonBehavior(this, backButtonBehavior);

		var collectionView = new CollectionView
		{
			AutomationId = "TestCollectionView",
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, "Name");
				return label;
			})
		};
		collectionView.SetBinding(CollectionView.ItemsSourceProperty, nameof(Issue33688ViewModel.Items));

		var filterButton = new Button
		{
			Text = "Load Items (triggers bug)",
			AutomationId = "FilterButton"
		};
		filterButton.SetBinding(Button.CommandProperty, nameof(Issue33688ViewModel.FilterCommand));

		var statusLabel = new Label
		{
			Text = "Tap 'Load Items' then press back button.",
			AutomationId = "StatusLabel"
		};

		Content = new VerticalStackLayout
		{
			Padding = 24,
			Spacing = 10,
			Children =
			{
				statusLabel,
				filterButton,
				collectionView
			}
		};
	}
}

file class Issue33688ViewModel : INotifyPropertyChanged
{
	static Label _resultLabelRef = null!;
	
	public static void SetResultLabelRef(Label label) => _resultLabelRef = label;

	private ObservableCollection<Issue33688Item> _items = new();

	public ObservableCollection<Issue33688Item> Items
	{
		get => _items;
		set
		{
			if (_items != value)
			{
				_items = value;
				OnPropertyChanged();
			}
		}
	}

	public ICommand SaveAndNavigateBackCommand { get; }
	public ICommand FilterCommand { get; }

	public Issue33688ViewModel()
	{
		SaveAndNavigateBackCommand = new Command(() =>
		{
			// Update the result label, then navigate back
			if (_resultLabelRef != null)
			{
				_resultLabelRef.Text = "BackButtonBehavior triggered!";
			}
			Shell.Current.GoToAsync("..");
		});

		FilterCommand = new Command(() =>
		{
			// This is the key action that triggers the bug:
			// Setting Items to a new ObservableCollection AFTER the page is displayed
			Items = new ObservableCollection<Issue33688Item>
			{
				new Issue33688Item { Name = "Item 1" },
				new Issue33688Item { Name = "Item 2" },
				new Issue33688Item { Name = "Item 3" }
			};
		});
	}

	public event PropertyChangedEventHandler PropertyChanged = null!;

	protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

file class Issue33688Item
{
	public string Name { get; set; } = string.Empty;
}
