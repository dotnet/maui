using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25551, "CollectionView SelectedItem binding issue on initial loading",
		PlatformAffected.All)]
	public partial class Issue25551 : ContentPage
	{
		public string SelectedItem { get; set; }

		public ObservableCollection<string> Items { get; set; }
		public List<object> SelectedItems { get; set; }

		public Issue25551()
		{
			InitializeComponent();
			SelectedItems = new List<object>();
			Items = new ObservableCollection<string> { "A", "B", "C", "D", "E", "F" };
			SelectedItem = Items[0];
			SelectedItems.Add(Items[1]);
			SelectedItems.Add(Items[3]);
			BindingContext = this;
		}
	}
}
