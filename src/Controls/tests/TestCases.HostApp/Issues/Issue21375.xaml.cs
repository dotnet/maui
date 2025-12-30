using System.Collections.ObjectModel;
using System.Text;

#if ANDROID
using AndroidX.RecyclerView.Widget;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 21375, "Selected CollectionView item is not announced", PlatformAffected.iOS & PlatformAffected.macOS & PlatformAffected.Android)]
public partial class Issue21375 : ContentPage
{
	public ObservableCollection<Item> Items { get; set; }

	public Issue21375()
	{
		InitializeComponent();

		Items = new ObservableCollection<Item>
		{
			new Item { Name = "Item 1", Description = "Description for item 1" },
			new Item { Name = "Item 2", Description = "Description for item 2" },
			new Item { Name = "Item 3", Description = "Description for item 3" },
			new Item { Name = "Item 4", Description = "Description for item 4" },
			new Item { Name = "Item 5", Description = "Description for item 5" }
		};

		BindingContext = this;
	}

	void NoneSelectionMode(object sender, EventArgs e)
	{
		collectionView.SelectionMode = SelectionMode.None;
	}

	void SingleSelectionMode(object sender, EventArgs e)
	{
		collectionView.SelectionMode = SelectionMode.Single;
	}

	void MultipleSelectionMode(object sender, EventArgs e)
	{
		collectionView.SelectionMode = SelectionMode.Multiple;
	}

	void Calculate(object sender, EventArgs e)
	{
		var sb = new StringBuilder();
#if IOS || MACCATALYST
		if (collectionView.Handler?.PlatformView is UIKit.UIView vc
			&& vc.Subviews is UIKit.UIView[] subviews && subviews.Length > 0
			&& subviews[0] is UIKit.UICollectionView uiCollectionView)
		{
			for (int i = 0; i < uiCollectionView.VisibleCells.Length; i++)
			{
				var cell = uiCollectionView.VisibleCells[i];
				sb.AppendLine($"Item{i} Cell: {cell.AccessibilityTraits}");
				if (cell.ContentView is not null && cell.ContentView.Subviews.Length > 0)
				{
					var firstChild = cell.ContentView.Subviews[0];
					sb.AppendLine($"Item{i} FirstChild: {firstChild.AccessibilityTraits}");
				}
			}
		}
#elif ANDROID
		var plat = collectionView.Handler?.PlatformView;
		if (plat is RecyclerView recyclerView)
		{
			var adapter = recyclerView.GetAdapter();
			var layoutManager = recyclerView.GetLayoutManager();
			var childCount = layoutManager.ChildCount;
			for (int i = 0; i < childCount; i++)
			{
				var viewHolder = recyclerView.GetChildAt(i);
				if (viewHolder is null)
					continue;

				var position = recyclerView.GetChildAdapterPosition(viewHolder);
				sb.AppendLine($"Item{position} Cell: {viewHolder.Selected}");
			}
		}
#elif WINDOWS
		var plat = collectionView.Handler?.PlatformView;
		if (plat is Microsoft.UI.Xaml.Controls.ListView listView)
		{
			foreach (var item in listView.Items)
			{
				var container = listView.ContainerFromItem(item) as Microsoft.UI.Xaml.Controls.ListViewItem;
				if (container != null)
				{
					sb.AppendLine($"Item: {item} IsSelected: {container.IsSelected}");
				}
			}
		}
#endif
		Output.Text = sb.ToString();
	}

	void ClearSelection(object sender, EventArgs e)
	{
		collectionView.SelectedItem = null;
	}

	public class Item
	{
		public string Name { get; set; }
		public string Description { get; set; }
	}
}
