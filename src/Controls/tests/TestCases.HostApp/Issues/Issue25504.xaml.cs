namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 25504, "ListView crashes when changing the root page inside the ItemSelected event", PlatformAffected.UWP)]
	public partial class Issue25504 : ContentPage
	{
		public Issue25504()
		{
			InitializeComponent();
			this.BindingContext = this;
		}

		private void OnItemSelected(object sender, SelectedItemChangedEventArgs args)
		{
			if (Application.Current?.Windows.Count > 0)
			{
				Application.Current.Windows[0].Page = new DetailsPage();
			}
		}

		public class DetailsPage : ContentPage
		{
			public DetailsPage()
			{
				Content = new Label { Text = "Details Page", AutomationId = "DetailsPage" };
			}
		}
	}
}