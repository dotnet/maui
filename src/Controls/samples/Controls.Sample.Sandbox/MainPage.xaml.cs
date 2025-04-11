using System.Collections.ObjectModel;
namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	public ObservableCollection<Item> Items { get; set; }

	public MainPage()
	{
		InitializeComponent();

		Items = new ObservableCollection<Item>
        {
            new Item { Name = "Item 1", Description = "Description for item 1" },
            new Item { Name = "Item 2", Description = "Description for item 2" },
            new Item { Name = "Item 3", Description = "Description for item 3" },
            new Item { Name = "Item 4", Description = "Description for item 4" },
            new Item { Name = "Item 5", Description = "Description for item 5" }
        };

        BindingContext = this;
	}

    void NoneSelectionMode(object sender, EventArgs e)
    {
        collectionView.SelectionMode = SelectionMode.None;
    }

    void SingleSelectionMode(object sender, EventArgs e)
    {
        collectionView.SelectionMode = SelectionMode.Single;
    }

    void MultipleSelectionMode(object sender, EventArgs e)
    {
        collectionView.SelectionMode = SelectionMode.Multiple;
    }

	public class Item
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
    }
}