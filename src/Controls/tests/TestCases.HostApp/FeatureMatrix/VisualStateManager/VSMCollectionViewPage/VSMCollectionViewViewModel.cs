using System.Collections.ObjectModel;

namespace Maui.Controls.Sample;

public class VSMCollectionViewItem
{
	public string Name { get; set; } = string.Empty;
}

public class VSMCollectionViewViewModel
{
	public ObservableCollection<VSMCollectionViewItem> Items { get; }

	public VSMCollectionViewViewModel()
	{
		Items = new ObservableCollection<VSMCollectionViewItem>
		{
			new VSMCollectionViewItem { Name = "Banana" },
			new VSMCollectionViewItem { Name = "Cherry" },
			new VSMCollectionViewItem { Name = "Date" },
			new VSMCollectionViewItem { Name = "Elderberry" },
			new VSMCollectionViewItem { Name = "Fig" },
			new VSMCollectionViewItem { Name = "Grape" },
			new VSMCollectionViewItem { Name = "Honeydew" }
		};
	}
}

