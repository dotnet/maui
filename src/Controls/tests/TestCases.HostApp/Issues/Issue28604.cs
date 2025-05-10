using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 28604, "Footer Not Displayed at the Bottom When EmptyView is Active in CV2", PlatformAffected.iOS)]
public class Issue28604 : ContentPage
{
	private CollectionViewViewModel ViewModel;

	public Issue28604()
	{
		ViewModel = new CollectionViewViewModel();
		BindingContext = ViewModel;

		var grid = new Grid();

		var collectionView = new CollectionView2
		{
			AutomationId = "CollectionView",
			ItemsSource = ViewModel.ItemList,
			EmptyView = "EmptyView",
			Background = Colors.AliceBlue,
			Header = new VerticalStackLayout
			{
				BackgroundColor = Colors.Red,
				Children =
					{
						new Label
						{
							Text = "Header",
							FontAttributes = FontAttributes.Bold
						}
					}
			},
			Footer = new VerticalStackLayout
			{
				BackgroundColor = Colors.Yellow,
				Children =
					{
						new Label
						{
							Text = "Footer",
							FontAttributes = FontAttributes.Bold
						}
					}
			}
		};
		grid.Children.Add(collectionView);

		Content = grid;
	}
}

public class CollectionViewViewModel
{
	public ObservableCollection<string> ItemList { get; set; }

	public CollectionViewViewModel()
	{
		ItemList = new ObservableCollection<string>();
	}
}
