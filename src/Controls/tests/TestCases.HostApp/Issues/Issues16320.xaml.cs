using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 16320, "Adding an item to a CollectionView with linear layout crashes", PlatformAffected.UWP)]
	public partial class Issue16320 : ContentPage
	{
		private ObservableCollection<string> items = new();

		public Issue16320()
		{
			InitializeComponent();

			items.Add("item: " + items.Count);

			cv1.ItemsSource = items;
		}

		private void ButtonAdd_Clicked(object sender, System.EventArgs e)
		{
			items.Add("item: " + items.Count);
		}
	}
}