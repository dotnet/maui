using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 18657, "CollectionView.EmptyView can not be removed by setting it to Null", PlatformAffected.UWP)]

public class Issue18657 : ContentPage
{
	Issue18657EmptyViewModel ViewModel;
	CollectionView collectionView;

	public Issue18657()
	{
		ViewModel = new Issue18657EmptyViewModel();
		BindingContext = ViewModel;

		var button = new Button
		{
			Text = "Remove EmptyView",
			AutomationId = "Button",
		};
		button.Clicked += Button_Clicked;

		collectionView = new CollectionView
		{
			ItemsSource = ViewModel.ItemList,
			EmptyView = new VerticalStackLayout
			{
				AutomationId = "EmptyView",
				BackgroundColor = Colors.AliceBlue,
				Children =
				{ 
					new Label
					{
						Text = "Empty View",
					}
				}
			},
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(button, 0, 0);
		grid.Add(collectionView, 0, 1);

		Content = grid;
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		collectionView.EmptyView = null;
	}
}

public class Issue18657EmptyViewModel
{
	public ObservableCollection<string> ItemList { get; set; }

	public Issue18657EmptyViewModel()
	{
		ItemList = new ObservableCollection<string>();
	}
}