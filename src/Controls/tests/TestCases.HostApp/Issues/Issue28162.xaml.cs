using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 28162, "Crash occurs when switching CollectionView.IsVisible right after setting ItemsSource", PlatformAffected.iOS)]
	public partial class Issue28162 : ContentPage
	{
		public Issue28162()
		{
			InitializeComponent();
		}

		private void Button_Clicked(object sender, EventArgs e)
		{
			CollectionView.ItemsSource = new ObservableCollection<string> { "Item 1", "Item 2", "Item 3" };
			CollectionView.IsVisible = !CollectionView.IsVisible;
		}
	}
}
