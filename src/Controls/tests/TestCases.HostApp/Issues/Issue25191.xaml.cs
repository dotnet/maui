using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25191, "CollectionView ItemSizingStrategy:MeasureFirstItem renders labels incorrectly", PlatformAffected.Android)]
	public partial class Issue25191 : ContentPage
	{
		public Issue25191()
		{
			InitializeComponent();
			BindingContext = new MainViewModel();
		}
	}

	public class MainViewModel
	{
		public ObservableCollection<string> Items { get; set; } = new ObservableCollection<string>
		{
			"Item 1",
			"Item 2",
			"Item 3",
			"Item 4"
		};
	}
}