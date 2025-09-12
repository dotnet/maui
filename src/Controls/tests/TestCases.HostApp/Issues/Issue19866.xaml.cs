using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, "19866", "[iOS] UICollectionView ScrollToTop does not work", PlatformAffected.iOS)]
	public partial class Issue19866 : ContentPage
	{
		public ObservableCollection<TestItem> Items { get; set; }

		public Issue19866()
		{
			InitializeComponent();
			Items = new ObservableCollection<TestItem>();
			
			// Add many items to allow scrolling
			for (int i = 1; i <= 100; i++)
			{
				Items.Add(new TestItem
				{
					Title = $"Item {i}",
					Description = $"This is item number {i}. Scroll down to test the scroll to top functionality by tapping the status bar.",
					AutomationId = $"Item_{i}"
				});
			}
			
			BindingContext = this;
		}

		private void OnScrollToTopClicked(object sender, System.EventArgs e)
		{
			// Programmatically scroll to the top to test scroll functionality
			if (Items.Count > 0)
			{
				TestCollectionView.ScrollTo(Items[0], position: ScrollToPosition.Start, animate: true);
			}
		}
	}

	public class TestItem
	{
		public string Title { get; set; }
		public string Description { get; set; }
		public string AutomationId { get; set; }
	}
}