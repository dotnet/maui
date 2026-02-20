using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 17823, "CollectionView reordering last item succeeds when header is present", PlatformAffected.Android)]
public partial class Issue17823 : ContentPage
{
	public ObservableCollection<ReorderItem> Items { get; }

	public Issue17823()
	{
		InitializeComponent();

		Items = new ObservableCollection<ReorderItem>
		{
			new ReorderItem(0, "Item 1"),
			new ReorderItem(1, "Item 2"),
			new ReorderItem(2, "Item 3"),
			new ReorderItem(3, "Item 4"),
		};

		BindingContext = this;

		ReorderCollectionView.ReorderCompleted += OnReorderCompleted;

		UpdateStatusLabel();
	}

	void OnReorderCompleted(object sender, EventArgs e)
	{
		UpdateStatusLabel();
	}

	void UpdateStatusLabel()
	{
		ReorderStatusLabel.Text = string.Join(", ", Items.Select(item => item.Text));
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
