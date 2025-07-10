using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Maui.Controls.Sample.Issues;

[XamlCompilation(XamlCompilationOptions.Compile)]
[Issue(IssueTracker.Github, 28023, "I2 Entering the vertical and horizontal lists again Monkeys still retain the spacing value changed last time", PlatformAffected.All)]
public partial  class Issue28023: NavigationPage
{
	public Issue28023() : base(new _28023Page())
	{

	}
}

public class _28023Page : ContentPage
{
	public _28023Page()
	{
		var button = new Button
		{
			AutomationId = "NavigatedPageButton",
			Text = "GoTo Main Page"
		};

		button.Clicked += (sender, e) =>
		{
			Navigation.PushAsync(new _28023MainPage());
		};

		Content = new StackLayout
		{
			Children =
			{
				button
			}
		};
	}
}

public class _28023MainPage : ContentPage
{
	CollectionView collectionView;

	public _28023MainPage()
	{
		BindingContext = new _28023ViewModel();

		var updateButton = new Button
		{
			AutomationId = "MainPageButton",
			Text = "Update Item Spacing",
			Margin=5,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		updateButton.Clicked += ButtonClicked;

		var navigationButton = new Button
		{
			AutomationId = "PopButton",
			Text = "GoTo Previous page",
			Margin=5,
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center
		};
		navigationButton.Clicked += NavigationButtonClicked;

		collectionView = new CollectionView
		{
			ItemTemplate = new DataTemplate(() =>
			{
				var image = new Image { HeightRequest = 100 };
				image.SetBinding(Image.SourceProperty, "Source");

				var label = new Label
				{
					HorizontalTextAlignment = TextAlignment.Center
				};
				label.SetBinding(Label.TextProperty, "Text");

				return new StackLayout
				{
					Orientation = StackOrientation.Vertical,
					Spacing = 10,
					BackgroundColor = Colors.Beige,
					Padding = 10,
					Children = { image, label }
				};
			})
		};
		collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Items");

		var stackLayout = new StackLayout
		{
			Children =
			{
				updateButton,
				navigationButton,
				collectionView
			}
		};

		Content = stackLayout;
	}

	void ButtonClicked(object sender, EventArgs e)
	{
		if (collectionView?.ItemsLayout is LinearItemsLayout linearItemsLayout)
		{
			linearItemsLayout.ItemSpacing = 100;
        }
	}

	async void NavigationButtonClicked(object sender, EventArgs e)
	{
		await Navigation.PopAsync();
	}

	public class _28023ViewModel
	{
		public ObservableCollection<_28023Model> Items { get; set; }

		public _28023ViewModel()
		{
			var collection = new ObservableCollection<_28023Model>();
			for (var i = 0; i < 50; i++)
			{
				collection.Add(new _28023Model
				{
					Text = "Image" + i,
					Source = "dotnet_bot.png"
				});
			}
			Items = collection;
		}
	}

	public class _28023Model
	{
		public string Text { get; set; }
		public string Source { get; set; }
	}
}