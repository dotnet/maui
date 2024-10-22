namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21728, "CollectionView item alignment issue when a single item is present with a footer", PlatformAffected.iOS)]
public partial class Issue21728 : ContentPage
{
	public IList<Issue21728TestItem> Items { get; set; }

	public Issue21728()
	{
		InitializeComponent();
		BindingContext = this;
		Items = new List<Issue21728TestItem>();
		Items.Add(new Issue21728TestItem() { Name = "Test Item 1" });
		collectionview.ItemsSource = Items;
	}

	public class Issue21728TestItem
	{
		public string Name { get; set; }
	}
}