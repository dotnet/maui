namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25433, "Collection view with horizontal grid layout has extra space on right end", PlatformAffected.iOS)]
public partial class Issue25433 : ContentPage
{
	public IList<Issue25433TestItem> Items { get; set; }

	public Issue25433()
	{
		InitializeComponent();
		BindingContext = this;
		Items =
		[
			new Issue25433TestItem() { Name = "Item 1" },
			new Issue25433TestItem() { Name = "Item 2" },
			new Issue25433TestItem() { Name = "Item 3" },
		];
		collectionView.ItemsSource = Items;
	}

	public class Issue25433TestItem
	{
		public string Name { get; set; }
	}
}