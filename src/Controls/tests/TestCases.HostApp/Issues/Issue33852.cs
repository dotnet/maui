using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33852, "CollectionView ScrollTo does not work with horizontal layout", PlatformAffected.iOS | PlatformAffected.macOS)]
public class Issue33852 : ContentPage
{
	public ObservableCollection<string> Items { get; set; }
	private Label _indexLabel;
	private CollectionView2 _collectionView;
	public Issue33852()
	{
		Items = new ObservableCollection<string>();
		for (int i = 0; i <= 50; i++)
		{
			Items.Add($"Item_{i}");
		}

		_indexLabel = new Label
		{
			AutomationId = "IndexLabel",
			Text = "ItemIndex: 0"
		};

		var scrollToButton = new Button
		{
			AutomationId = "ScrollToButton",
			Text = "Scroll to Item 15",
			WidthRequest = 150
		};
		scrollToButton.Clicked += OnScrollToButtonClicked;

		_collectionView = new CollectionView2
		{
			AutomationId = "TestCollectionView",
			ItemsSource = Items,
			HeightRequest = 600,
			ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Horizontal)
			{
				ItemSpacing = 10
			},
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label();
				label.SetBinding(Label.TextProperty, ".");
				return new Border
				{
					Margin = new Thickness(5),
					Padding = new Thickness(10),
					Stroke = Colors.Gray,
					Content = label
				};
			})
		};

		_collectionView.Scrolled += OnCollectionViewScrolled;

		Content = new StackLayout
		{
			Children =
			{
				_indexLabel,
				 scrollToButton,
				_collectionView
			}
		};
	}

	private void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
	{
		if (e.FirstVisibleItemIndex > 0)
		{
			_indexLabel.Text = $"The CollectionView is scrolled";
		}
		else
		{
			_indexLabel.Text = "The CollectionView does not scroll";
		}
	}

	private void OnScrollToButtonClicked(object sender, EventArgs e)
	{
		_collectionView.ScrollTo(15, position: ScrollToPosition.Start, animate: true);
	}
}