using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25551, "CollectionView SelectedItem binding issue on initial loading",
		PlatformAffected.All)]
	public partial class Issue25551 : ContentPage
	{
		public string SelectedItem { get; set; }

		public ObservableCollection<string> Items { get; set; }

		public Issue25551()
		{
			InitializeComponent();

			Items = new ObservableCollection<string>{ "A","B","C","D","E","F"};
			SelectedItem = Items[0];
			BindingContext = this;
		}
	}
}
