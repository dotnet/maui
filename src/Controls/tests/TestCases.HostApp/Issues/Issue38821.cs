using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 38821, "CurrentItem is incorrect after removing an item from the CarouselView (Loop = false)", PlatformAffected.Android)]
public class Issue38821 : ContentPage
{
	CarouselView _carouselView;
	Issue38821_ViewModel _viewModel;

	public Issue38821()
	{
		_viewModel = new Issue38821_ViewModel();

		BindingContext = _viewModel;

		_carouselView = new CarouselView
		{
			AutomationId = "Issue38821_CarouselView",
			ItemsSource = _viewModel.Items,
			Loop = false,
			ItemTemplate = new DataTemplate(() =>
			{
				var grid = new Grid();

				var label = new Label
				{
					FontSize = 40,
					VerticalOptions = LayoutOptions.Center,
					HorizontalOptions = LayoutOptions.Center
				};

				label.SetBinding(Label.TextProperty, ".");

				grid.Add(label);

				return grid;
			})
		};

		var button = new Button
		{
			Text = "Remove current Item",
			AutomationId = "Issue38821_Button"
		};

		button.Clicked += Button_Clicked;

		var grid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition(GridLength.Star),
				new RowDefinition(GridLength.Auto)
			}
		};

		grid.Add(_carouselView);
		Grid.SetRow(button, 1);
		grid.Add(button);

		Content = grid;
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		var currentItem = _carouselView.CurrentItem?.ToString();

		if (currentItem is not null)
			_viewModel.Items?.Remove(currentItem);

		_carouselView.CurrentItem = "2";
	}
}

public class Issue38821_ViewModel
{
	public ObservableCollection<string> Items { get; } = new()
	{
		"Item0",
		"Item1",
		"Item2",
	};
}
