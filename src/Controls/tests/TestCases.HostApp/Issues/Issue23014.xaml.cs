using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 23014, "App crashes when calling ItemsView.ScrollTo on unloaded CollectionView", PlatformAffected.All)]
	public partial class Issue23014 : ContentPage
	{
		public Issue23014()
		{
			InitializeComponent();
			BindingContext = new Issue23014ViewModel();
		}

		private void OnRemoveAndScrollToClicked(object sender, EventArgs e)
		{
			Stack.Remove(ItemList);

			try
			{
				ItemList.ScrollTo(0);
				ItemList.ScrollTo("Foo");
				StatusLabel.Text = "Success";
			}
			catch (Exception ex)
			{
				StatusLabel.Text = $"Fail: {ex.Message}";
			}
		}
	}

	public class Issue23014ViewModel
	{
		public ObservableCollection<string> Items { get; } = ["Foo", "Bar", "Baz", "Goo"];
	}
}
