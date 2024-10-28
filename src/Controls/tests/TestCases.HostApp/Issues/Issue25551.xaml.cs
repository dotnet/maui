using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.None, 25551, "CollectionView Programmatic-Selection is not updated when binding the selectedItem",
		PlatformAffected.All)]
	public partial class Issue25551 : ContentPage
	{
		public Issue25551()
		{
			InitializeComponent();
		}
	}

	public class MainPageViewModel
	{

		public string SelectedItem { get; set; }

		public ObservableCollection<string> Items { get; set; }

		public MainPageViewModel()
		{
			Items = new ObservableCollection<string>
		{
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4",
			"Item 5"

		};
			SelectedItem = Items[0];
		}
	}
}
