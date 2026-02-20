using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.None, 30118, "IndicatorView does not visually update dot indicators when the ItemsSource count changes", PlatformAffected.iOS)]
public partial class Issue30118 : ContentPage
{
	ObservableCollection<string> _items = [];

	public Issue30118()
	{
		InitializeComponent();
		carouselView.ItemsSource = _items;
	}

	private void Button_Clicked(object sender, EventArgs e)
	{
		_items.Add($"Item {_items.Count + 1}");
	}
}