using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 39295, "CollectionView inside RefreshView becomes blank when scrolling on Mac Catalyst", PlatformAffected.macOS)]

public partial class Issue39295 : Shell
{
	Button _button;
	public Issue39295()
	{
		_button = new Button
		{
			Text = "Go To CollectionView Test",
			AutomationId = "GoToCollectionViewTestButton"
		};

		_button.Clicked += (s, e) =>
		{
			Shell.Current.GoToAsync("Issue39295MainPage");
		};
		Items.Add(new ContentPage
		{
			Content = new VerticalStackLayout
			{
				Children =
				{
					_button,
				}
			}
		});

		Routing.RegisterRoute("Issue39295MainPage", typeof(Issue39295MainPage));
	}
}

public class Issue39295MainPage : ContentPage
{
	CollectionView collectionView;
	public Issue39295MainPage()
	{
		BindingContext = new MonkeysViewModel();

		var instruction1 = new Label
		{
			Text = "CollectionView inside RefreshView becomes blank when scrolling on Mac Catalyst"
		};

		var button = new Button
		{
			Text = "ScrollTo 10th item",
			AutomationId = "ScrollTo10thItemButton"
		};

		button.Clicked += (s, e) =>
		{
			collectionView.ScrollTo(10);
		};

		collectionView = new CollectionView
		{
			VerticalScrollBarVisibility = ScrollBarVisibility.Always
		};

		collectionView.SetBinding(
			ItemsView.ItemsSourceProperty,
			nameof(MonkeysViewModel.Monkeys));

		collectionView.ItemTemplate = new DataTemplate(() =>
		{
			var grid = new Grid
			{
				Padding = 10,
				RowDefinitions =
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto }
				},
				ColumnDefinitions =
				{
					new ColumnDefinition { Width = GridLength.Auto },
					new ColumnDefinition { Width = GridLength.Star }
				}
			};

			var image = new Image
			{
				HeightRequest = 60,
				WidthRequest = 60,
				Aspect = Aspect.AspectFill
			};
			image.SetBinding(Image.SourceProperty, nameof(Monkey.ImageUrl));
			Grid.SetRowSpan(image, 2);

			var nameLabel = new Label
			{
				FontAttributes = FontAttributes.Bold
			};
			nameLabel.SetBinding(Label.TextProperty, nameof(Monkey.Name));
			Grid.SetColumn(nameLabel, 1);

			var locationLabel = new Label
			{
				FontAttributes = FontAttributes.Italic,
				VerticalOptions = LayoutOptions.End
			};
			locationLabel.SetBinding(Label.TextProperty, nameof(Monkey.Location));
			Grid.SetRow(locationLabel, 1);
			Grid.SetColumn(locationLabel, 1);

			grid.Children.Add(image);
			grid.Children.Add(nameLabel);
			grid.Children.Add(locationLabel);

			return grid;
		});

		var refreshView = new RefreshView
		{
			Content = collectionView
		};

		var rootGrid = new Grid
		{
			RowDefinitions =
			{
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Auto },
				new RowDefinition { Height = GridLength.Star }
			}
		};

		Grid.SetRow(instruction1, 0);
		Grid.SetRow(button, 1);
		Grid.SetRow(refreshView, 2);

		rootGrid.Children.Add(instruction1);
		rootGrid.Children.Add(button);
		rootGrid.Children.Add(refreshView);

		Content = rootGrid;
	}
}