namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 25181, "CollectionView item alignment issue in HorizontalGrid layout when only one item is present", PlatformAffected.iOS)]
public partial class Issue25181 : ContentPage
{
	public IList<TestItem> Items { get; set; }

	public Issue25181()
    {
        InitializeComponent();
		BindingContext = this;
		Items = new List<TestItem>();
		Items.Add(new TestItem() { Name = "Test Item 1" });
		collectionview.ItemsSource = Items;
	}
	
	public class TestItem
	{
		public string Name { get; set; }
	}
}