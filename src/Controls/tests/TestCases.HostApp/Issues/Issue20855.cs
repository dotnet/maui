using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Layouts;

namespace Maui.Controls.Sample.Issues
{

	[Issue(IssueTracker.Github, 20855, "Grouped CollectionView items not rendered properly on Android, works on Windows", PlatformAffected.Android)]
	public class Issue20855 : TestContentPage
	{

		protected override void Init()
		{
            var ItemsViewModel = new Issue20855ItemsViewModel();
            this.BindingContext = ItemsViewModel;

		var collectionView = new CollectionView
        {
            IsGrouped = true,
            ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem
        };
        collectionView.GroupHeaderTemplate = new DataTemplate(() =>
        {
            var label = new Label
            {
                FontSize = 18,
                BackgroundColor = Colors.DarkGray,
                TextColor = Colors.White,
                Padding = new Thickness(10, 0)
            };
            label.SetBinding(Label.TextProperty, "Name");

            return  label;
        });
        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var flexLayout = new FlexLayout
            {
                JustifyContent =  FlexJustify.SpaceBetween,
                AlignItems = FlexAlignItems.Center,
                HeightRequest = 100
            };

            var nameLabel = new Label
            {
                Padding = new Thickness(8, 0),
                HeightRequest = 80
            };
            nameLabel.SetBinding(Label.TextProperty, "Name");

            var dateLabel = new Label
            {
                Padding = new Thickness(8, 0),
                HeightRequest = 85
            };
            dateLabel.SetBinding(Label.TextProperty, new Binding("CreateDate"));

            flexLayout.Children.Add(nameLabel);
            flexLayout.Children.Add(dateLabel);

            return flexLayout;
        });
        collectionView.SetBinding(CollectionView.ItemsSourceProperty, new Binding("ItemGroups"));
        Content = collectionView;
        }
	}

public class Issue20855ItemsViewModel : INotifyPropertyChanged
{
    public ObservableCollection<ItemGroup> ItemGroups { get; } = new();

    public ObservableCollection<Item> Items { get; } = new();

    public Issue20855ItemsViewModel()
    {
        var itemMock = new ItemMock();

        foreach (var item in itemMock.Items)
        {
            Items.Add(item);
        }

        foreach (var item in itemMock.ItemGroups)
        {
            ItemGroups.Add(item);
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
public class Item
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime DueDate { get; set; }
}

public class ItemGroup : List<Item>
{
    public string Name { get; private set; }

    public ItemGroup(string name, List<Item> items) : base(items)
    {
        Name = name;
    }
}

public class ItemMock
{
    public List<Item> Items { get; set; } = new List<Item>();
    public List<ItemGroup> ItemGroups { get; set; } = new List<ItemGroup>();

    public ItemMock()
    {
        Init();
    }

    private void Init()
    {
        var dueDate = DateTime.Now.Date;
        for (int i = 0; i < 10; i++)
        {
            if (i % 5 == 0 && i != 0)
            {
                dueDate = new DateTime(2025, 2, 25, 15, 30, 0);
            }
            Item item
                = new Item
            {
                Name = "Foo" + $" {i}",
                CreateDate = new DateTime(2025, 2, 13, 15, 30, 0),
                DueDate = dueDate
            };
            Items.Add(item);
        }

        ProcessGroupItems();
    }

    private void ProcessGroupItems()
    {
        var itemList = Items;
        var now = DateTime.Now.Date;
        var tomorrow = now + TimeSpan.FromDays(1);
        var tomPlusSeven = now + TimeSpan.FromDays(7);
        var todaysItems = itemList.FindAll(x =>
            x.DueDate.Date == now.Date);
        var frem = itemList.RemoveAll(x =>
            x.DueDate.Date == now.Date);
        var nextWeeksItems = itemList.FindAll(x =>
            x.DueDate.Date >= tomorrow.Date &&
            x.DueDate.Date <= tomPlusSeven.Date);
        var srem = itemList.RemoveAll(x =>
            x.DueDate.Date >= tomorrow.Date &&
            x.DueDate.Date <= tomPlusSeven.Date);
        var remainingItems = itemList;
        ItemGroups = new List<ItemGroup>
        {
            new ItemGroup("Today", todaysItems.ToList()),
            new ItemGroup("Next Week", remainingItems.ToList())
        };
    }
}
}
