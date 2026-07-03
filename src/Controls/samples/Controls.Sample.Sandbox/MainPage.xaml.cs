using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	readonly ObservableCollection<object?> _reorderItems =
		new() { "Item 1", null, null, "Item 4" };

	public MainPage()
	{
		InitializeComponent();

		// Scenario 1 & 2: null item in single-selection list
		TapCV.ItemsSource = new ObservableCollection<object?> { "Item A", null, "Item C" };
		TapStatus.Text = "Tap the blank row (index 1) — must not crash";

		// Scenario 3: drag-reorder with two null items
		ReorderCV.ItemsSource = _reorderItems;
		ReorderStatus.Text = "Order: " + Summary();
	}

	void TapCV_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		TapStatus.Text = TapCV.SelectedItem is null
			? "✅ null row tapped — no crash, SelectedItem is null"
			: $"Selected: {TapCV.SelectedItem}";
	}

	void SimulateReorder_Clicked(object sender, EventArgs e)
	{
		// Move "Item 1" (index 0) past the two null items to index 2.
		// Expected after move: [null, null, "Item 1", "Item 4"]
		_reorderItems.Move(0, 2);
		ReorderStatus.Text = "After move: " + Summary();
	}

	string Summary() =>
		"[" + string.Join(", ", _reorderItems.Select(x => x?.ToString() ?? "<null>")) + "]";
}