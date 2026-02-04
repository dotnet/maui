using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public class Item
{
	public string Name { get; set; } = string.Empty;
}

public class VSMCollectionViewViewModel
{
	public ObservableCollection<Item> Items { get; }

	public VSMCollectionViewViewModel()
	{
		Items = new ObservableCollection<Item>
		{
			new Item { Name = "Banana" },
			new Item { Name = "Cherry" },
			new Item { Name = "Date" },
			new Item { Name = "Elderberry" },
			new Item { Name = "Fig" },
			new Item { Name = "Grape" },
			new Item { Name = "Honeydew" }
		};
	}
}