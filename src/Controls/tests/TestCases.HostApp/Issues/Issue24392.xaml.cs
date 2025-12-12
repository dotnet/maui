using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 24392, "Items in CollectionView take up large vertical space in iOS", PlatformAffected.iOS | PlatformAffected.macOS)]
	public partial class Issue24392 : ContentPage
	{
		public ObservableCollection<string> MyItemList { get; set; }

		public Issue24392()
		{
			InitializeComponent();
			
			MyItemList = new ObservableCollection<string>
			{
				"Item 1",
				"Item 2",
				"Item 3",
				"Item 4",
				"Item 5"
			};
			
			BindingContext = this;
		}
	}
}
