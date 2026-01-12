using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33401, "CollectionView's SelectionChanged isn't fired on iOS when it's inside a grid with TapGestureRecognizer", PlatformAffected.iOS)]
public partial class Issue33401 : ContentPage
{
	private int _selectionChangedCount = 0;
	private int _gridTappedCount = 0;

	public Issue33401()
	{
		InitializeComponent();

		var items = new ObservableCollection<string>
		{
			"Test 1",
			"Test 2",
			"Test 3",
			"Test 4",
			"Test 5",
			"Test 6",
			"Test 7",
			"Test 8",
			"Test 9",
			"Test 10"
		};

		TestCollectionView.ItemsSource = items;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		_selectionChangedCount++;
		var selectedItem = e.CurrentSelection.FirstOrDefault()?.ToString() ?? "None";
		Console.WriteLine($"[ISSUE-33401] SelectionChanged fired - Count: {_selectionChangedCount}, Selected: {selectedItem}");
		StatusLabel.Text = $"SelectionChanged: {_selectionChangedCount} times - Selected: {selectedItem}";
	}

	private void OnGridTapped(object sender, EventArgs e)
	{
		_gridTappedCount++;
		Console.WriteLine($"[ISSUE-33401] Grid TapGestureRecognizer fired - Count: {_gridTappedCount}");
	}
}
