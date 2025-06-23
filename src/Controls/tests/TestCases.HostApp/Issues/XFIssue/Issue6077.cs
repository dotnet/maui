using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 6077, "CollectionView (iOS) using horizontal grid does not display last column of uneven item count", PlatformAffected.iOS)]
public class Issue6077 : TestNavigationPage
{
	public class MainViewModel : INotifyPropertyChanged
	{
		readonly IList<ItemModel> _items;
		public ObservableCollection<ItemModel> Items { get; private set; }

		public MainViewModel()
		{
			_items = new List<ItemModel>();
			CreateItemsCollection();
		}

		void CreateItemsCollection(int items = 5)
		{
			for (int n = 0; n < items; n++)
			{
				_items.Add(new ItemModel
				{
					Title = $"Item {n + 1}",
				});
			}

			Items = new ObservableCollection<ItemModel>(_items);
		}

		protected bool SetProperty<T>(ref T backingStore, T value,
			[CallerMemberName] string propertyName = "",
			Action onChanged = null)
		{
			if (EqualityComparer<T>.Default.Equals(backingStore, value))
				return false;

			backingStore = value;
			onChanged?.Invoke();
			OnPropertyChanged(propertyName);
			return true;
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			var changed = PropertyChanged;
			if (changed == null)
				return;

			changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
		#endregion
	}

	public class ItemModel
	{
		public string Title { get; set; }
	}

	ContentPage CreateRoot()
	{
		var page = new ContentPage { Title = "Issue6077" };

		var cv = new CollectionView { ItemSizingStrategy = ItemSizingStrategy.MeasureAllItems };

		var itemsLayout = new GridItemsLayout(3, ItemsLayoutOrientation.Horizontal);


		cv.ItemsLayout = itemsLayout;

		var template = new DataTemplate(() =>
		{
			var grid = new Grid { HeightRequest = 100, WidthRequest = 50, BackgroundColor = Colors.AliceBlue };

			grid.RowDefinitions = new RowDefinitionCollection { new RowDefinition { Height = new GridLength(100) } };
			grid.ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition { Width = new GridLength(50) } };

			var label = new Label { };

			label.SetBinding(Label.TextProperty, new Binding("Title"));

			var content = new ContentView { Content = label };

			grid.Children.Add(content);

			return grid;
		});

		cv.ItemTemplate = template;
		cv.SetBinding(ItemsView.ItemsSourceProperty, new Binding("Items"));

		page.Content = cv;

		BindingContext = new MainViewModel();

		return page;
	}

	protected override void Init()
	{

		PushAsync(CreateRoot());

	}
}
