using System.Collections.ObjectModel;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 28415, "CollectionView display is broken when setting IsVisible after items are added", PlatformAffected.Android)]
	public class Issue28415 : ContentPage
	{
		CollectionView _collectionView;

		public Issue28415()
		{
			_collectionView = new CollectionView
			{
				IsVisible = false,
				ItemTemplate = new DataTemplate(() =>
				{
					var contentView = new ContentView();

					var border = new Border
					{
						Margin = new Thickness(5, 0),
						StrokeShape = new RoundRectangle { CornerRadius = 10 }
					};

					var grid = new Grid { Margin = new Thickness(15) };
					var label = new Label();
					label.SetBinding(Label.TextProperty, ".");
					label.SetBinding(Label.AutomationIdProperty, ".");

					grid.Add(label);
					border.Content = grid;
					contentView.Content = border;

					return contentView;
				})
			};

			Button button = new Button
			{
				Text = "Load List",
				AutomationId = "LoadListButton",
				Command = new Command(() => LoadList())
			};

			Content = new StackLayout
			{
				Children = { button, _collectionView }
			};
		}

		async void LoadList()
		{
			_collectionView.ItemsSource = new ObservableCollection<string>(Enumerable.Range(0, 40).Select(i => $"Item{i}"));
			await Task.Delay(1000);
			_collectionView.IsVisible = true;
		}
	}
}
