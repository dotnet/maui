using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25859, "Item spacing not properly applied between items in CollectionView Horizontal LinearItemsLayout", PlatformAffected.iOS)]
public partial class Issue25859 : ContentPage
{
	public List<Issue25859ItemGroup> GroupItems { get; private set; } = new List<Issue25859ItemGroup>();

	public Issue25859()
	{
		InitializeComponent();
		BindingContext = this;
		GroupItems.Add(new Issue25859ItemGroup("Fruits", new List<Issue25859TestItem> { new Issue25859TestItem { Name = "Apple" }, new Issue25859TestItem { Name = "Banana" } }));
		GroupItems.Add(new Issue25859ItemGroup("Vegetables", new List<Issue25859TestItem> { new Issue25859TestItem { Name = "Carrot" }, new Issue25859TestItem { Name = "Broccoli" } }));
		GroupItems.Add(new Issue25859ItemGroup("Beverages", new List<Issue25859TestItem> { new Issue25859TestItem { Name = "Coffee" }, new Issue25859TestItem { Name = "Tea" } }));

		collectionView.ItemsSource = GroupItems;
		collectionView.IsGrouped = true;
	}
}

public class Issue25859ItemGroup : List<Issue25859TestItem>
{
	public string Name { get; private set; }

	public Issue25859ItemGroup(string name, List<Issue25859TestItem> items) : base(items)
	{
		Name = name;
	}
}

public class Issue25859TestItem
{
	public string Name { get; set; }
}