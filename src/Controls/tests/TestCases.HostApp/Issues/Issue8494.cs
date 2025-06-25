using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 8494, "Margin doesn't work inside CollectionView EmptyView", PlatformAffected.UWP)]

public class Issue8494 : ContentPage
{
	Issue8494EmptyViewModel ViewModel;

	public Issue8494()
	{
		ViewModel = new Issue8494EmptyViewModel();
		BindingContext = ViewModel;
		var label = new Label
		{
			Text = "EmptyView should be laid out with respect to the Specified margin",
			AutomationId = "EmptyViewDescriptionLabel",
		};

		var emptyViewLayout = new StackLayout
		{
			Margin = new Thickness(40),
			Background = Colors.Yellow,
			Children =
				{
					new Label
					{
						Text = "EmptyView with Margin.",
						BackgroundColor = Colors.Blue,
						HorizontalTextAlignment = TextAlignment.Center,
					},
				}
		};

		var collectionView = new CollectionView
		{
			ItemsSource = ViewModel.ItemList,
			BackgroundColor = Colors.Green,
			EmptyView = emptyViewLayout,
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star },
			}
		};

		grid.Add(label, 0, 0);
		grid.Add(collectionView, 0, 1);
		Content = grid;
	}
}

public class Issue8494EmptyViewModel
{
	public ObservableCollection<string> ItemList { get; set; }

	public Issue8494EmptyViewModel()
	{
		ItemList = new ObservableCollection<string>();
	}
}