using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 26810, "Scroll To first item in CollectionView when updating the collection with KeepItemsInView",
		PlatformAffected.Android)]
public partial class Issue26810 : ContentPage
{
	private ObservableCollection<Issue26810ItemModel> Items { get; set; } = new ObservableCollection<Issue26810ItemModel>();
	public Issue26810()
	{
		InitializeComponent();
		for (int i = 1; i <= 20; i++)
		{
			Items.Add(new Issue26810ItemModel { Name = $"Preloaded Item {i}", AutomationId = $"Item{i}" });
		}
		CollectionView.ItemsSource = Items;
		this.BindingContext = this;
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
			CollectionView.ScrollTo(18, position: ScrollToPosition.End, animate: true);
		}
	}
}

public class Issue26810ItemModel
{
	public string Name { get; set; }
	public string AutomationId { get; set; }
}
