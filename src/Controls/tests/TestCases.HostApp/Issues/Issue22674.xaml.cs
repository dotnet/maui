using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Issues
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[Issue(IssueTracker.Github, 22674, "Crash when quickly clicking to delete item", PlatformAffected.iOS)]
	public partial class Issue22674 : ContentPage
	{
		public Issue22674()
		{
			InitializeComponent();

			BindingContext = this;
		}

		public ObservableCollection<string> ItemList { get; } = ["A", "B", "C", "D", "E", "F", "G", "H", "I",];

		void OnCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (sender is CollectionView view && view.SelectedItem is string item)
			{
				ItemList.Remove(item);
			}
		}
	}
}