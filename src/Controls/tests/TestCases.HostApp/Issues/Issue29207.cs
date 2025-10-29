using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29207, "KeepLastItemInView Does Not Scroll to Last Item When Adding Items at Top, Instead Scrolls to SecondLast Item", PlatformAffected.UWP)]
public class Issue29207 : ContentPage
{
	Issue29207_ItemsViewModel viewModel;

	public Issue29207()
	{
		viewModel = new Issue29207_ItemsViewModel();
		BindingContext = viewModel;

		CollectionView collectionView = new CollectionView
		{
			ItemsSource = viewModel.ItemsSource,
			ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView,
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					FontSize = 20,
					Padding = 10
				};
				label.SetBinding(Label.TextProperty, ".");
				return label;
			})
		};

		var addButton = new Button
		{
			Text = "Add",
			WidthRequest = 120,
			AutomationId = "InsertItemButton"
		};
		addButton.Clicked += Button_Clicked_1;

		var buttonLayout = new HorizontalStackLayout
		{
			Padding = 40,
			Spacing = 40,
			Children = { addButton }
		};

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		grid.Add(buttonLayout, 0, 0);
		grid.Add(collectionView, 0, 1);

		Content = grid;
	}

	void Button_Clicked_1(object sender, EventArgs e)
	{
		viewModel.ItemsSource.Insert(0, $"NewItem");
	}
}

public class Issue29207_ItemsViewModel
{
	public ObservableCollection<string> ItemsSource = new ObservableCollection<string>();

	public Issue29207_ItemsViewModel()
	{
		for (int i = 0; i < 20; i++)
		{
			ItemsSource.Add($"Item {i}");
		}
	}
}