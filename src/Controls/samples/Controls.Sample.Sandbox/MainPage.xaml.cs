using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample;

public partial class MainPage : ContentPage
{
	// Items list intentionally contains null entries to expose the null-item bugs:
	//   • Null drag stale index  – dragging a null item used to cancel immediately
	//   • Null-item false selection – null items incorrectly appeared pre-selected
	readonly ObservableCollection<string?> _items = new()
	{
		"Apple",
		null,          // null item at index 1 – first drag target for the bug
		"Banana",
		null,          // second null item – shows stale-index issue after reorder
		"Cherry",
		"Date",
		null,          // third null – veri
		// fies multiple nulls work
		"Elderberry",
	};

	[UnconditionalSuppressMessage("Trimming", "IL2026", Justification = "Sandbox sample intentionally loads XAML at runtime for test flexibility.")]
	public MainPage()
	{
		this.LoadFromXaml(typeof(MainPage));
		TheCollectionView.ItemsSource = _items;
	}

	CollectionView TheCollectionView =>
		this.FindByName<CollectionView>(nameof(TheCollectionView))
		?? throw new InvalidOperationException($"Missing named element: {nameof(TheCollectionView)}");

	Label StatusLabel =>
		this.FindByName<Label>(nameof(StatusLabel))
		?? throw new InvalidOperationException($"Missing named element: {nameof(StatusLabel)}");

	// ─── Selection ───────────────────────────────────────────────────────────

	void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		var selected = e.CurrentSelection.FirstOrDefault();
		StatusLabel.Text = selected is null
			? "Selected: (null item)  ← should only show after an explicit tap"
			: $"Selected: \"{selected}\"";
		StatusLabel.TextColor = Colors.DarkBlue;
	}

	// ─── Reorder ─────────────────────────────────────────────────────────────

	void OnReorderCompleted(object? sender, EventArgs e)
	{
		StatusLabel.Text = $"Reorder complete. Order: {string.Join(", ", _items.Select(x => x ?? "(null)"))}";
		StatusLabel.TextColor = Colors.DarkGreen;
	}

	// ─── Buttons ─────────────────────────────────────────────────────────────

	/// <summary>
	/// Insert a new null item at position 2 so it sits between non-null items.
	/// Exercises the null-drag path on a freshly inserted null item whose Tag
	/// was never set by a previous drag cycle.
	/// </summary>
	void OnInsertNull(object? sender, EventArgs e)
	{
		_items.Insert(2, null);
		StatusLabel.Text = $"Inserted null at index 2. Count={_items.Count}";
		StatusLabel.TextColor = Colors.DarkOrange;
	}

	/// <summary>
	/// Replace the first non-null item with null.
	/// Exercises the grouped-null-Replace desync path in GroupedItemTemplateCollection2
	/// (and also the flat Replace path in ObservableItemTemplateCollection2).
	/// </summary>
	void OnReplaceWithNull(object? sender, EventArgs e)
	{
		int idx = _items.IndexOf(_items.FirstOrDefault(x => x is not null) ?? _items[0]);
		if (idx >= 0)
		{
			_items[idx] = null;
			StatusLabel.Text = $"Replaced index {idx} with null. Order: {string.Join(", ", _items.Select(x => x ?? "(null)"))}";
			StatusLabel.TextColor = Colors.Purple;
		}
	}

	/// <summary>
	/// Reset to the original list so all three bugs can be reproduced fresh.
	/// </summary>
	void OnReset(object? sender, EventArgs e)
	{
		_items.Clear();
		foreach (var item in new string?[] { "Apple", null, "Banana", null, "Cherry", "Date", null, "Elderberry" })
		{
			_items.Add(item);
		}
		StatusLabel.Text = "List reset.";
		StatusLabel.TextColor = Colors.DarkGreen;
	}
}
