#nullable enable
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 36068, "CollectionView2 (Windows) drag-and-drop reorder with null items in the ItemsSource", PlatformAffected.All)]
public partial class CollectionViewNullItemDragReorder : ContentPage
{
	public ObservableCollection<ReorderItem?> Items { get; }

	public CollectionViewNullItemDragReorder()
	{
		InitializeComponent();

		// A null entry sits between two real items so a drag can either originate from,
		// or land on, the blank row rendered for it. This exercises the container-index
		// and drag-source lookups used for null-data rows during reorder (GetContainerIndex /
		// UpdateAllContainerIndices / the Tag-based blank-container drag detection), not just
		// ObservableCollection.Move() semantics.
		Items = new ObservableCollection<ReorderItem?>
		{
			new ReorderItem(0, "Item A"),
			null,
			new ReorderItem(2, "Item B"),
			new ReorderItem(3, "Item C"),
		};

		BindingContext = this;

		ReorderCollectionView.ReorderCompleted += OnReorderCompleted;

		UpdateStatusLabel();
	}

	void OnReorderCompleted(object? sender, EventArgs e)
	{
		UpdateStatusLabel();
	}

	void UpdateStatusLabel()
	{
		ReorderStatusLabel.Text = string.Join(", ", Items.Select(item => item?.Text ?? "null"));
	}

	public class ReorderItem
	{
		public ReorderItem(int index, string text)
		{
			Text = text;
			ContainerAutomationId = $"ReorderItem{index}";
			LabelAutomationId = $"ReorderItemLabel{index}";
		}

		public string Text { get; set; }

		public string ContainerAutomationId { get; }

		public string LabelAutomationId { get; }
	}
}
