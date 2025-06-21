using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 26810, "Scroll To first item in CollectionView with vertical orientation when updating the collection with KeepItemsInView",
		PlatformAffected.Android)]

public partial class Issue26810Vertical : ContentPage
{

	private ObservableCollection<Issue26810ItemModel> Items { get; set; } = new ObservableCollection<Issue26810ItemModel>();
	public Issue26810Vertical()
	{
		InitializeComponent();
		for (int i = 1; i <= 30; i++)
		{
			Items.Add(new Issue26810ItemModel { Name = $"Preloaded Item {i}", AutomationId = $"Item{i}" });
		}
		CollectionView.ItemsSource = Items;
		this.BindingContext = this;
	}
	private void ItemsUpdatingScrollMode_Clicked(object sender, EventArgs e)
	{
		var button = (Button)sender;
		if (button.Text == "KeepScrollOffset")
		{
			CollectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
		}
		else if (button.Text == "KeepLastItemInView")
		{
			CollectionView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
		}
	}

	private void AddButton_Clicked(object sender, EventArgs e)
	{
		Items.Add(new Issue26810ItemModel { Name = $"Item {Items.Count + 1}", AutomationId = $"Item{Items.Count + 1}" });
	}
	private void ScrollToButton_Clicked(object sender, EventArgs e)
	{
		if (Items.Count > 0)
		{
			// Scroll to random item
			CollectionView.ScrollTo(19, position: ScrollToPosition.End, animate: true);
		}
	}
}
