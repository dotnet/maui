using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28530, "[Catalyst] CanMixGroups Set to False Still Allows Reordering Between Groups in CollectionView", PlatformAffected.macOS)]
public partial class Issue28530 : ContentPage
{
	public Issue28530()
	{
		InitializeComponent();
		BindingContext = new Issue28530CollectionViewViewModel();
	}
}

public class Issue28530CollectionViewViewModel : INotifyPropertyChanged
{
	public ObservableCollection<Issue28530GroupedItem> GroupedItems { get; set; }
	public event PropertyChangedEventHandler PropertyChanged;

	public Issue28530CollectionViewViewModel()
	{
		GroupedItems = new ObservableCollection<Issue28530GroupedItem>
			{
				new Issue28530GroupedItem("Group 1")
				{
					new Issue28530Item { Name = "Item 1" },
					new Issue28530Item { Name = "Item 2" }
				},
				new Issue28530GroupedItem("Group 2")
				{
					new Issue28530Item { Name = "Item 3" },
					new Issue28530Item { Name = "Item 4" }
				}
			};
	}

	protected void OnPropertyChanged(string propertyName)
	{
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}
}

public class Issue28530GroupedItem : ObservableCollection<Issue28530Item>
{
	public string GroupHeaderName { get; set; }

	public Issue28530GroupedItem(string groupHeaderName)
	{
		GroupHeaderName = groupHeaderName;
	}
}

public class Issue28530Item
{
	public string Name { get; set; }
}