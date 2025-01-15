using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 26810, "Scroll To first item in CollectionView when updating the collection with KeepItemsInView",
		PlatformAffected.Android)]
public partial class Issue26810 : ContentPage
{
	private ObservableCollection<Issue26810ItemModel> Items { get; set; } = new ObservableCollection<Issue26810ItemModel>();
	private Random _random = new Random();

	public Issue26810()
	{
		InitializeComponent();
		for (int i = 1; i <= 10; i++)
		{
			Items.Add(new Issue26810ItemModel { Name = $"Preloaded Item {i}", AutomationId = $"Item{i}" });
		}
		CollectionView.ItemsSource = Items;
		this.BindingContext = this;
	}

	private async void CollectionView_Loaded(object sender, EventArgs e)
	{
		// Adding items with a small delay in LoadedEvent
		for (int i = 10; i <= 30; i++)
		{
			Items.Add(new Issue26810ItemModel { Name = $" Item {i}", AutomationId = $"Item{i}" });
			await Task.Delay(500); // Simulate delay
		}
	}

	private void ScrollToButton_Clicked(object sender, EventArgs e)
	{
		if (Items.Count > 0)
		{
			int randomIndex = _random.Next(Items.Count);
			CollectionView.ScrollTo(randomIndex);
		}
	}
}

public class Issue26810ItemModel
{
	public string Name { get; set; }
	public string AutomationId { get; set; }
}
