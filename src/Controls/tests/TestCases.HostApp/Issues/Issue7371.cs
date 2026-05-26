namespace Maui.Controls.Sample.Issues
{
	[Issue(IssueTracker.Github, 7371, "iOS race condition(or not checking for null) of refreshing(offset animation) causes NullReferenceException", PlatformAffected.iOS)]
	public class Issue7371 : TestContentPage
	{
		ListView ListView => Content as ListView;
		protected override void Init()
		{
			SafeAreaEdges = new SafeAreaEdges(SafeAreaRegions.Container);
			Content = new ListView();
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			ListView.IsRefreshing = true;

			Application.Current.MainPage = new ContentPage() { Content = new Label { AutomationId = "Success", Text = "Success", VerticalOptions = LayoutOptions.Center } };

			ListView.IsRefreshing = false;
		}
	}
}