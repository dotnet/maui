using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29415, "ItemsUpdatingScrollMode in CarouselView Not Working as expected", PlatformAffected.Android)]
public partial class Issue29415 : ContentPage
{
	public ObservableCollection<string> Items { get; set; }
	public Issue29415()
	{
		InitializeComponent();
		Items = new ObservableCollection<string>
		{
			"Item1",
			"Item2",
		};
		BindingContext = this;
		carouselView.ItemsSource = Items;
	}

	private void OnItemsUpdatingScrollModeChanged(object sender, CheckedChangedEventArgs e)
	{
		if (e.Value && sender is RadioButton radioButton)
		{

			switch (radioButton.Content.ToString())
			{
				case "KeepItemsInView":
					carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepItemsInView;
					break;
				case "KeepLastItemInView":
					carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepLastItemInView;
					break;
				case "KeepScrollOffset":
					carouselView.ItemsUpdatingScrollMode = ItemsUpdatingScrollMode.KeepScrollOffset;
					break;
			}
		}
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		Items.Add($"Item{Items.Count + 1}");
	}
}